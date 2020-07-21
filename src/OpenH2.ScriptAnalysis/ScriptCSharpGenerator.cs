using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic;
using OpenH2.Core.Extensions;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    public class ScriptCSharpGenerator
    {
        private readonly ScenarioTag scenario;
        private readonly ClassDeclarationSyntax classDecl;
        private readonly NamespaceDeclarationSyntax nsDecl;
        private List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();

        private Stack<GenerationState> state = new Stack<GenerationState>();
        private Stack<object> stateData = new Stack<object>();
        private Stack<BlockSyntax> scopes = new Stack<BlockSyntax>();

        public const string EngineImplementationClass = "Engine";

        public ScriptCSharpGenerator(ScenarioTag scnr, string[] refrences = null)
        {
            this.scenario = scnr;
            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var ns = "OpenH2.Scripts." + string.Join('.', scenarioParts.Take(2));

            this.nsDecl = NamespaceDeclaration(ParseName(ns));

            var classModifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword))
                .Add(Token(SyntaxKind.StaticKeyword));

            // TODO: shorthand attribute creation
            var attr = Attribute(ParseName("OriginScenario"), AttributeArgumentList(SeparatedList(new[] {
                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(scnr.Name)))
            })));

            var classAttrs = List<AttributeListSyntax>()
                .Add(AttributeList(SeparatedList(new[] { attr })));

            this.classDecl = ClassDeclaration("scnr_" + scenarioParts.Last())
                .WithModifiers(classModifiers)
                .WithAttributeLists(classAttrs);
        }


        public void AddMethod(ScenarioTag.ScriptMethodDefinition scriptMethod)
        {
            var modifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword))
                .Add(Token(SyntaxKind.StaticKeyword));

            // TODO: method return type? decorate with additional info?
            var method = MethodDeclaration(ParseTypeName("void"), scriptMethod.Description)
                .WithModifiers(modifiers);

            // Push root method body as first scope
            scopes.Push(Block());

            var childIndices = new ContinuationStack<int>();
            childIndices.Push(scriptMethod.ValueA);



            while (childIndices.TryPop(out var currentIndex, out var isContinuation))
            {
                var node = scenario.ScriptSyntaxNodes[currentIndex];
                
                if(isContinuation == false)
                {
                    HandleNodeStart(node, childIndices);
                }
                else
                {
                    HandleNodeEnd(node);
                }
            }

            methods.Add(method.WithBody(scopes.Pop()));

            Debug.Assert(scopes.Count == 0, "Extra scopes on stack");
        }

        private void HandleNodeStart(ScenarioTag.ScriptSyntaxNode node,
            ContinuationStack<int> indices)
        {
            switch (node.NodeType)
            {
                case NodeType.ExpressionScope:

                    break;
                case NodeType.Statement:
                    HandleStatementStart(node);
                    break;
                case NodeType.VariableAccess:
                    break;
            }


            
            //parent.Children.Add(current);

            //var nextNodeParent = current;

            // Expression scope seems to use NodeData to specify what is inside the scope
            // and the Next value is used to specify the scope's next sibling instead
            // This is how the linear-ish node structure can expand into a more traditional AST
            if (node.NodeType == NodeType.ExpressionScope)
            {
                // Use scope's parent as next node's parent instead of the scope
                // This makes the 'next' into a 'sibling'
                //nextNodeParent = parent;

                Debug.Assert(scenario.ScriptSyntaxNodes[node.NodeData_H16].Checkval == node.NodeData_L16, "Scope's next node checkval didn't match");
                indices.Push(node.NodeData_H16);
            }

            // Push NextIndex using the appropriate parent node
            if (node.NextIndex != ushort.MaxValue)
            {
                Debug.Assert(scenario.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                indices.Push(node.NextIndex/*, nextNodeParent*/);
            }
        }

        private void HandleStatementStart(ScenarioTag.ScriptSyntaxNode node)
        {
            Span<byte> strings = scenario.ScriptStrings;

            switch (node.DataType)
            {
                case NodeDataType.StatementStart:
                    throw new Exception("StatementStart is not a valid statement type");
                case NodeDataType.MethodOrOperator:
                    HandleMethodStart(node);
                    break;
                case NodeDataType.ReferenceGet:
                    break;
                case NodeDataType.AI:
                case NodeDataType.AIScript:
                case NodeDataType.Device:
                case NodeDataType.EntityIdentifier:
                case NodeDataType.Entity:
                case NodeDataType.Trigger:
                case NodeDataType.LocationFlag:
                case NodeDataType.List:
                    HandleStaticFieldAccess(node);
                    break;
                case NodeDataType.Float:
                case NodeDataType.Int:
                case NodeDataType.String:
                case NodeDataType.Short:
                case NodeDataType.Boolean:
                    HandleLiteral(node);
                    break;
                default:
                    // TODO: hack until everything is tracked down, populating string as value if exists
                    if (node.NodeString > 0 && strings[node.NodeString - 1] == 0)
                    {
                        //value = strings.ReadStringStarting(node.NodeString);
                    }
                    break;
            }
        }

        private void HandleMethodStart(ScenarioTag.ScriptSyntaxNode node)
        {
            stateData.Push(CreateEngineCall(GetScriptString(node)));
        }

        private void HandleMethodEnd()
        {
            var ie = stateData.Pop() as InvocationExpressionSyntax;

            Debug.Assert(ie != null, "Popped state data did not match expectations");

            var mutatedScope = scopes.Pop().AddStatements(ExpressionStatement(ie));
            scopes.Push(mutatedScope);
        }

        private LiteralExpressionSyntax HandleLiteral(ScenarioTag.ScriptSyntaxNode node)
        {
            var literal = SyntaxUtil.LiteralExpression(scenario, node);

            if (stateData.TryPop(out var currentData))
            {
                object mutatedStateData = currentData switch
                {
                    InvocationExpressionSyntax m => m.AddArgumentListArguments(Argument(literal)),
                };

                stateData.Push(mutatedStateData ?? currentData);
            }

            return literal;
        }

        private IdentifierNameSyntax HandleStaticFieldAccess(ScenarioTag.ScriptSyntaxNode node)
        {
            var id = IdentifierName(GetScriptString(node));

            if (stateData.TryPop(out var currentData))
            {
                object mutatedStateData = currentData switch
                {
                    InvocationExpressionSyntax m => m.AddArgumentListArguments(Argument(id)),
                };

                stateData.Push(mutatedStateData ?? currentData);
            }

            return id;
        }

        private void HandleNodeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.ExpressionScope:
                    break;
                case NodeType.Statement:
                    switch (node.DataType)
                    {
                        case NodeDataType.MethodOrOperator:
                            HandleMethodEnd();
                            break;
                        case NodeDataType.AI:
                        case NodeDataType.AIScript:
                        case NodeDataType.ReferenceGet:
                        case NodeDataType.Device:
                        case NodeDataType.EntityIdentifier:
                        case NodeDataType.Boolean:
                        case NodeDataType.Short:
                        case NodeDataType.String:
                        case NodeDataType.Entity:
                        case NodeDataType.StatementStart:
                        case NodeDataType.Float:
                        case NodeDataType.Int:
                        case NodeDataType.Trigger:
                        case NodeDataType.LocationFlag:
                        case NodeDataType.List:
                            break;
                    }

                    break;
                case NodeType.VariableAccess:
                    break;
            }
        }

        public string Generate()
        {
            var cls = classDecl
                .WithMembers(List<MemberDeclarationSyntax>(methods))
                // TODO: fields / properties
                ;

            var ns = nsDecl.WithMembers(List<MemberDeclarationSyntax>().Add(cls));

            var csharpTree = CSharpSyntaxTree.Create(ns.NormalizeWhitespace());

            //var compilation = CSharpCompilation.Create("OpenH2ScriptGen", new[] { csharpTree });
            //compilation.Emit()

            return csharpTree.ToString();
        }

        private InvocationExpressionSyntax CreateEngineCall(string method)
        {
            return InvocationExpression(IdentifierName(method))
                .WithArgumentList(ArgumentList());
        }

        private string GetScriptString(ScenarioTag.ScriptSyntaxNode node)
        {
            return ((Span<byte>)scenario.ScriptStrings).ReadStringStarting(node.NodeString);
        }

        private enum GenerationState
        {
            WritingScope,
            WritingMethodCall,
        }
    }
}
