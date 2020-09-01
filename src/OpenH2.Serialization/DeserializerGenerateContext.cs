using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Serialization.Layout;
using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="instanceStart"></param>
    /// <param name="staticOffset">
    /// A static offset that is subtracted from each offset that is found in the data.
    /// This allows relocating/rebasing the data
    /// </param>
    /// <returns></returns>
    public delegate T Deserializer<T>(Span<byte> data, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null);
    public delegate object Deserializer(Span<byte> data, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null);

    internal class DeserializerGenerateContext
    {
        private static SyntaxToken dataParam = ParseToken("data");
        private static SyntaxToken startParam = ParseToken("instanceStart");
        private static SyntaxToken offsetParam = ParseToken("staticOffset");
        private static SyntaxToken stringsParam = ParseToken("stringProvider");
        private static SyntaxToken itemLocal = ParseToken("item");

        private readonly SemanticModel model;
        private readonly INamedTypeSymbol serializableType;
        private readonly WellKnown wellKnown;

        public DeserializerGenerateContext(SemanticModel model, INamedTypeSymbol serializableType, WellKnown wellKnown)
        {
            this.model = model;
            this.serializableType = serializableType;
            this.wellKnown = wellKnown;
        }

        internal void Generate(ref ClassDeclarationSyntax cls, LayoutInfo layoutInfo)
        {
            var statements = new List<StatementSyntax>();

            statements.Add(GenerateLocalCreation(serializableType));

            var sortedMembers = layoutInfo.MemberInfos.OrderBy(a => a.LayoutAttribute.Offset);

            foreach (var member in sortedMembers)
            {
                var memberStatements = member.LayoutAttribute switch
                {
                    PrimitiveValueAttribute prim => GeneratePrimitiveValueRead(member, prim),
                    PrimitiveArrayAttribute arr => GeneratePrimitiveArrayRead(member, arr),
                    ReferenceArrayAttribute reference => GenerateReferenceArrayRead(member, reference),
                    StringValueAttribute str => GenerateStringValueRead(member, str),

                    _ => Array.Empty<StatementSyntax>(),
                };

                statements.AddRange(memberStatements);
            }

            statements.AddRange(GenerateInternedStringMembers(sortedMembers));

            statements.Add(GenerateReturn());

            var method = MethodDeclaration(ParseTypeName(serializableType.ToDisplayString()), SerializationClassAttribute.DeserializeMethod)
                .AddParameterListParameters(
                    Parameter(dataParam).WithType(GenericName("Span").AddTypeArgumentListArguments(PredefinedType(Token(SyntaxKind.ByteKeyword)))),
                    Parameter(startParam).WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))),
                    Parameter(offsetParam).WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))),
                    Parameter(stringsParam).WithType(ParseTypeName(nameof(IInternedStringProvider)))
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .WithBody(Block(statements));

            cls = cls.AddMembers(method);
        }

        private StatementSyntax GenerateLocalCreation(INamedTypeSymbol type)
        {
            var formatterType = model.Compilation.GetTypeSymbol(typeof(FormatterServices));

            var creation = CastExpression(ParseTypeName(type.ToDisplayString()), InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    ParseName(typeof(FormatterServices).FullName),
                    IdentifierName(nameof(FormatterServices.GetUninitializedObject))))
                 .AddArgumentListArguments(Argument(TypeOfExpression(ParseTypeName(type.ToDisplayString())))));

            return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).AddVariables(
                VariableDeclarator(itemLocal, null, EqualsValueClause(creation)))
            );
        }

        private StatementSyntax GenerateReturn()
        {
            return ReturnStatement(IdentifierName(itemLocal));
        }

        private IEnumerable<StatementSyntax> GeneratePrimitiveValueRead(LayoutInfo.MemberInfo member, PrimitiveValueAttribute prim)
        {
            var type = member.Type;
            bool castToFinal = false;

            if (member.Type.TypeKind == TypeKind.Enum)
            {
                type = (member.Type as INamedTypeSymbol).EnumUnderlyingType;
                castToFinal = true;
            }

            if(type is INamedTypeSymbol named && named.IsGenericType)
            {
                type = named.ConstructUnboundGenericType();
            }

            if (this.wellKnown.PrimitiveReaders.TryGetValue(type, out var mi))
            {
                ExpressionSyntax getExp = mi(IdentifierName(dataParam), 
                    Argument(BinaryExpression(SyntaxKind.AddExpression,
                        IdentifierName(startParam),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(prim.Offset)))));

                if(castToFinal)
                {
                    getExp = CastExpression(ParseTypeName(member.Type.ToDisplayString()), getExp);
                }

                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(itemLocal),
                        IdentifierName(member.PropertySymbol.Name)),
                    getExp))
                    .WithTrailingTrivia(CarriageReturnLineFeed);
            }
            else
            {
                yield return EmptyStatement().WithTrailingTrivia(Comment($"// Unable to find reader for '{type.ToDisplayString()}'"));
            }
        }


        private IEnumerable<StatementSyntax> GeneratePrimitiveArrayRead(LayoutInfo.MemberInfo member, PrimitiveArrayAttribute arr)
        {
            var arrType = member.Type as IArrayTypeSymbol;

            if (arrType == null)
            {
                yield return ExpressionStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("System.Diagnostics.Debug"),
                        IdentifierName("Fail"))).AddArgumentListArguments(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("PrimitiveArray properties must be array types")))));
                
                yield break;
            }

            var elemType = arrType.ElementType;
            var castElem = false;

            if(elemType.TypeKind == TypeKind.Enum)
            {
                elemType = (member.Type as INamedTypeSymbol).EnumUnderlyingType;
                castElem = true;
            }

            if (elemType is INamedTypeSymbol named && named.IsGenericType)
            {
                elemType = named.ConstructUnboundGenericType();
            }

            if (this.wellKnown.PrimitiveReaders.TryGetValue(elemType, out var mi))
            {
                var propAccessor = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(itemLocal),
                        IdentifierName(member.PropertySymbol.Name));

                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    propAccessor,
                    ArrayCreationExpression(ArrayType(ParseTypeName(arrType.ElementType.ToDisplayString()))
                        .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(arr.Count))))))));

                var loopBody = new List<StatementSyntax>();
                var loopVar = "i";

                ExpressionSyntax sizeofExpression = SizeOfExpression(ParseTypeName(arrType.ElementType.ToDisplayString()));

                if(wellKnown.SizeOf.TryGetValue(arrType.ElementType, out var size))
                {
                    sizeofExpression = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(size));
                }

                var index = BinaryExpression(SyntaxKind.AddExpression,
                    BinaryExpression(SyntaxKind.MultiplyExpression,
                        IdentifierName(loopVar),
                        sizeofExpression),
                    BinaryExpression(SyntaxKind.AddExpression,
                        IdentifierName(startParam),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(arr.Offset))));

                ExpressionSyntax getExp = mi(IdentifierName(dataParam), Argument(index));

                if (castElem)
                {
                    getExp = CastExpression(ParseTypeName(arrType.ElementType.ToDisplayString()), getExp);
                }

                // set array index to getExp
                var elementAccess = ElementAccessExpression(propAccessor).AddArgumentListArguments(
                    Argument(IdentifierName(loopVar)));

                loopBody.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, elementAccess, getExp)));

                yield return SyntaxUtils.ForLoop(Block(loopBody), 0, arr.Count, loopVar)
                    .WithTrailingTrivia(CarriageReturnLineFeed);
            }
            else
            {
                yield return EmptyStatement().WithTrailingTrivia(Comment($"// Unable to find reader for '{elemType.ToDisplayString()}'"));
            }
        }

        private IEnumerable<StatementSyntax> GenerateReferenceArrayRead(LayoutInfo.MemberInfo member, ReferenceArrayAttribute arr)
        {
            var arrType = member.Type as IArrayTypeSymbol;

            if (arrType == null)
            {
                yield return ExpressionStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("System.Diagnostics.Debug"),
                        IdentifierName("Fail"))).AddArgumentListArguments(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("ReferenceArray properties must be array types")))));

                yield break;
            }

            var elemType = arrType.ElementType;

            var propAccessor = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(itemLocal),
                        IdentifierName(member.PropertySymbol.Name));

            var count = ParseToken("count" + arr.Offset);
            yield return SyntaxUtils.LocalVar(count, SyntaxUtils.ReadSpanInt32(dataParam, startParam, arr.Offset));

            var offset = ParseToken("offset" + arr.Offset);
            yield return SyntaxUtils.LocalVar(offset, BinaryExpression(SyntaxKind.SubtractExpression,
                SyntaxUtils.ReadSpanInt32(dataParam, startParam, arr.Offset+4),
                IdentifierName(offsetParam)));


            yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                propAccessor,
                ArrayCreationExpression(ArrayType(ParseTypeName(arrType.ElementType.ToDisplayString()))
                    .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                        IdentifierName(count)))))));

            var loopBody = new List<StatementSyntax>();
            var loopVar = "i";

            ExpressionSyntax sizeofExpression = SizeOfExpression(ParseTypeName(arrType.ElementType.ToDisplayString()));

            if (wellKnown.SizeOf.TryGetValue(arrType.ElementType, out var size))
            {
                sizeofExpression = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(size));
            }

            var itemStartOffset = BinaryExpression(SyntaxKind.AddExpression,
                IdentifierName(offset),
                BinaryExpression(SyntaxKind.MultiplyExpression,
                    IdentifierName(loopVar),
                    sizeofExpression));

            ExpressionSyntax getExp;

            if (this.wellKnown.PrimitiveReaders.TryGetValue(elemType, out var mi))
            {
                getExp = mi(IdentifierName(dataParam), Argument(itemStartOffset));
            }
            else if(elemType is INamedTypeSymbol named && named.IsGenericType
                && this.wellKnown.PrimitiveReaders.TryGetValue(named.ConstructUnboundGenericType(), out var genericMi))
            {
                getExp = genericMi(IdentifierName(dataParam), Argument(itemStartOffset));
            }
            else
            {
                getExp = InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(nameof(BlamSerializer)),
                        GenericName(nameof(BlamSerializer.Deserialize))
                            .AddTypeArgumentListArguments(ParseTypeName(elemType.ToDisplayString()))))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(dataParam)),
                        Argument(itemStartOffset),
                        Argument(IdentifierName(offsetParam)),
                        Argument(IdentifierName(stringsParam)));
            }

            // set array index to getExp
            var elementAccess = ElementAccessExpression(propAccessor).AddArgumentListArguments(
                Argument(IdentifierName(loopVar)));

            loopBody.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, elementAccess, getExp)));

            yield return SyntaxUtils.ForLoop(Block(loopBody), 0, IdentifierName(count), loopVar)
                .WithTrailingTrivia(CarriageReturnLineFeed);
        }


        private IEnumerable<StatementSyntax> GenerateStringValueRead(LayoutInfo.MemberInfo member, StringValueAttribute str)
        {
            var type = member.Type;

            Debug.Assert(type.ToDisplayString().Equals(typeof(string).Name, StringComparison.OrdinalIgnoreCase));

            if (this.wellKnown.PrimitiveReaders.TryGetValue(type, out var mi))
            {
                ExpressionSyntax getExp = mi(IdentifierName(dataParam), 
                        Argument(BinaryExpression(SyntaxKind.AddExpression,
                            IdentifierName(startParam),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(str.Offset)))),
                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(str.MaxLength))));

                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(itemLocal),
                        IdentifierName(member.PropertySymbol.Name)),
                    getExp))
                    .WithTrailingTrivia(CarriageReturnLineFeed);
            }
            else
            {
                yield return EmptyStatement().WithTrailingTrivia(Comment($"// Unable to find reader for '{type.ToDisplayString()}'"));
            }
        }


        private IEnumerable<StatementSyntax> GenerateInternedStringMembers(IOrderedEnumerable<LayoutInfo.MemberInfo> sortedMembers)
        {
            var internedStatements = new List<StatementSyntax>();

            foreach (var member in sortedMembers)
            {
                if (member.LayoutAttribute is InternedStringAttribute interned)
                {
                    internedStatements.AddRange(GenerateInternedStringRead(member, interned));
                }
            }

            if (internedStatements.Count == 0)
                yield break;

            yield return IfStatement(BinaryExpression(SyntaxKind.NotEqualsExpression,
                IdentifierName(stringsParam),
                LiteralExpression(SyntaxKind.NullLiteralExpression)),
                Block(internedStatements));
        }

        private IEnumerable<StatementSyntax> GenerateInternedStringRead(LayoutInfo.MemberInfo member, InternedStringAttribute interned)
        {
            var type = member.Type;

            Debug.Assert(type.ToDisplayString().Equals(typeof(string).Name, StringComparison.OrdinalIgnoreCase));

            if (this.wellKnown.PrimitiveReaders.TryGetValue(type, out var mi))
            {
                var internedDataLocal = "interned" + interned.Offset;
                var internedData = SyntaxUtils.ReadSpanInt32(dataParam, startParam, interned.Offset);
                yield return SyntaxUtils.LocalVar(Identifier(internedDataLocal), internedData);

                var stringIndex = BinaryExpression(SyntaxKind.MultiplyExpression,
                    ParenthesizedExpression(BinaryExpression(SyntaxKind.BitwiseAndExpression,
                        IdentifierName(internedDataLocal),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, ParseToken("0xFFFFFF")))),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(4)));

                var stringLength = BinaryExpression(SyntaxKind.RightShiftExpression,
                    IdentifierName(internedDataLocal),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(24)));

                var dataOffset = SyntaxUtils.ReadSpanInt32(dataParam, BinaryExpression(SyntaxKind.AddExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(stringsParam),
                        IdentifierName(nameof(IInternedStringProvider.IndexOffset))),
                    stringIndex));

                ExpressionSyntax getExp = mi(IdentifierName(dataParam),
                        Argument(BinaryExpression(SyntaxKind.AddExpression,
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(stringsParam),
                                IdentifierName(nameof(IInternedStringProvider.DataOffset))),
                            dataOffset)),
                        Argument(stringLength));

                yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(itemLocal),
                        IdentifierName(member.PropertySymbol.Name)),
                    getExp))
                    .WithTrailingTrivia(CarriageReturnLineFeed);
            }
            else
            {
                yield return EmptyStatement().WithTrailingTrivia(Comment($"// Unable to find reader for '{type.ToDisplayString()}'"));
            }



        }
    }
}
