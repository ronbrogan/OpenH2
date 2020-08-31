using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Serialization.Layout;
using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Generic;
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
    public delegate object Deserializer(Span<byte> data, int instanceStart = 0, int staticOffset = 0);

    internal class DeserializerGenerateContext
    {
        private static SyntaxToken dataParam = ParseToken("data");
        private static SyntaxToken startParam = ParseToken("instanceStart");
        private static SyntaxToken offsetParam = ParseToken("staticOffset");
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

            foreach (var member in layoutInfo.MemberInfos.OrderBy(a => a.LayoutAttribute.Offset))
            {
                var memberStatements = member.LayoutAttribute switch
                {
                    PrimitiveValueAttribute prim => GeneratePrimitiveValueRead(member, prim),
                    PrimitiveArrayAttribute arr => GeneratePrimitiveArrayRead(member, arr),
                    ReferenceArrayAttribute reference => GenerateReferenceArrayRead(member, reference),
                    //StringValueAttribute str => Generate_Read(),
                    //InternedStringAttribute interned => Generate_Read(),

                    _ => Array.Empty<StatementSyntax>(),
                };

                statements.AddRange(memberStatements);
            }

            statements.Add(GenerateReturn());

            var method = MethodDeclaration(ParseTypeName(serializableType.ToDisplayString()), SerializationClassAttribute.DeserializeMethod)
                .AddParameterListParameters(
                    Parameter(dataParam).WithType(GenericName("Span").AddTypeArgumentListArguments(PredefinedType(Token(SyntaxKind.ByteKeyword)))),
                    Parameter(startParam).WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))),
                    Parameter(offsetParam).WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))
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

            if (this.wellKnown.PrimitiveReaders.TryGetValue(type, out var mi))
            {
                ExpressionSyntax getExp = InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(dataParam),
                        IdentifierName(mi)))
                    .AddArgumentListArguments(
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

                ExpressionSyntax getExp = InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(dataParam),
                        IdentifierName(mi)))
                    .AddArgumentListArguments(
                        Argument(index));

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
                getExp = InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(dataParam),
                        IdentifierName(mi)))
                    .AddArgumentListArguments(
                        Argument(itemStartOffset));
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
                        Argument(IdentifierName(startParam)),
                        Argument(itemStartOffset));
            }

            // set array index to getExp
            var elementAccess = ElementAccessExpression(propAccessor).AddArgumentListArguments(
                Argument(IdentifierName(loopVar)));

            loopBody.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, elementAccess, getExp)));

            yield return SyntaxUtils.ForLoop(Block(loopBody), 0, IdentifierName(count), loopVar)
                .WithTrailingTrivia(CarriageReturnLineFeed);
        }
    }
}
