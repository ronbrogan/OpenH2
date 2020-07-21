using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.CompilerServices;
using OpenH2.Core.Extensions;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using OpenH2.ScriptAnalysis.GenerationState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    /*
     * TODO:
     * - Typed scopes aren't implemented yet, missing many arguments
     *    - Ideally try to find return value to remove redundant casts
     * - Engine mock/stubs
     * - External reference lookups
     * - Create project with generated .cs files and/or compile and make warnings/errors available
     * 
     */


    public class ScriptCSharpGenerator
    {
        private readonly ScenarioTag scenario;
        private readonly ClassDeclarationSyntax classDecl;
        private readonly NamespaceDeclarationSyntax nsDecl;
        private readonly List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        private readonly List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();

        private Stack<IScriptGenerationState> stateData = new Stack<IScriptGenerationState>();
        private IScriptGenerationState currentState => stateData.Peek();

        public const string EngineImplementationClass = "Engine";

        public ScriptCSharpGenerator(ScenarioTag scnr, string[] refrences = null)
        {
            this.scenario = scnr;
            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var ns = "OpenH2.Scripts." + string.Join('.', scenarioParts.Take(2));

            this.nsDecl = NamespaceDeclaration(ParseName(ns));

            var classModifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword));

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

        internal void AddGlobalVariable(ScenarioTag.ScriptVariableDefinition variable)
        {
            fields.Add(SyntaxUtil.CreateFieldDeclaration(scenario, variable));
        }

        public void AddMethod(ScenarioTag.ScriptMethodDefinition scriptMethod)
        {
            var modifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword));

            // TODO: Check return type property is correct. Decorate with additional info?
            var method = MethodDeclaration(SyntaxUtil.ScriptTypeSyntax((ScriptDataType)scriptMethod.Index1), scriptMethod.Description)
                .WithModifiers(modifiers);

            // Push root method body as first scope
            stateData.Push(new ScopeBlockData());

            var childIndices = new ContinuationStack<int>();
            childIndices.PushFull(scriptMethod.ValueA);

            while (childIndices.TryPop(out var currentIndex, out var isContinuation))
            {
                var node = scenario.ScriptSyntaxNodes[currentIndex];

                if (isContinuation == false)
                {
                    HandleNodeStart(node, childIndices);
                }
                else
                {
                    HandleNodeEnd(node);
                }
            }

            var scope = stateData.Pop() as ScopeBlockData;

            Debug.Assert(scope != null, "Last scope wasn't a ScopeBlockData");

            if (scope.Statements.Count == 1 && scope.Statements[0].ChildNodes().First() is InvocationExpressionSyntax invocation
                && invocation.ChildNodes().First() is ParenthesizedLambdaExpressionSyntax lambda
                && lambda.Body is BlockSyntax lambdaBlock)
            {
                method = method.WithBody(lambdaBlock);
            }
            else
            {
                method = method.WithBody(Block().WithStatements(List(scope.Statements)));
            }

            methods.Add(method);

            Debug.Assert(stateData.Count == 0, "Extra scopes on stack");
        }

        private void HandleNodeStart(ScenarioTag.ScriptSyntaxNode node, ContinuationStack<int> indices)
        {
            switch (node.NodeType)
            {
                case NodeType.ExpressionScope:
                    HandleExpressionScopeStart(node, indices);
                    break;
                case NodeType.Statement:
                    HandleStatementStart(node, indices);
                    break;
                case NodeType.VariableAccess:
                    HandleVariableAccess(node);
                    break;
            }
        }

        // Expression scope seems to use NodeData to specify what is inside the scope
        // and the Next value is used to specify the scope's next sibling instead
        // This is how the linear-ish node structure can expand into a more traditional AST
        private void HandleExpressionScopeStart(ScenarioTag.ScriptSyntaxNode node, ContinuationStack<int> indices)
        {
            if (node.DataType == ScriptDataType.StatementStart)
            {
                if (node.NextIndex != ushort.MaxValue)
                {
                    // If we're not at top level, we need to push behind the current scope's continuation
                    // This makes the 'next' into a 'sibling'
                    bool repushOrig = indices.TryPop(out var orig, out var origCont);

                    Debug.Assert(scenario.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                    indices.PushFull(node.NextIndex);

                    if (repushOrig)
                    {
                        Debug.Assert(origCont, "Popped scope wasn't a continuation");
                        indices.PushSeparate(orig, origCont);
                    }
                }

                // These nodes should be inside of a new scope, so pushing here
                Debug.Assert(scenario.ScriptSyntaxNodes[node.NodeData_H16].Checkval == node.NodeData_L16, "Scope's next node checkval didn't match");
                indices.PushFull(node.NodeData_H16);
                stateData.Push(new ScopeBlockData());
            }
            else
            {
                switch (node.DataType)
                {
                    case ScriptDataType.MethodOrOperator:
                        break;
                    case ScriptDataType.Boolean:
                        break;
                    case ScriptDataType.Float:
                        break;
                    case ScriptDataType.Short:
                        break;
                    case ScriptDataType.Int:
                        break;
                    case ScriptDataType.String:
                        break;
                    case ScriptDataType.Trigger:
                        break;
                    case ScriptDataType.LocationFlag:
                        break;
                    case ScriptDataType.AI:
                        break;
                    case ScriptDataType.AIScript:
                        break;
                    case ScriptDataType.List:
                        break;
                    case ScriptDataType.ReferenceGet:
                        break;
                    case ScriptDataType.Entity:
                        break;
                    case ScriptDataType.Device:
                        break;
                    case ScriptDataType.EntityIdentifier:
                        break;
                }
            }
        }

        private void HandleStatementStart(ScenarioTag.ScriptSyntaxNode node, ContinuationStack<int> indices)
        {
            if (node.NextIndex != ushort.MaxValue)
            {
                Debug.Assert(scenario.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                indices.PushFull(node.NextIndex);
            }

            Span<byte> strings = scenario.ScriptStrings;

            switch (node.DataType)
            {
                case ScriptDataType.StatementStart:
                    throw new Exception("StatementStart is not a valid statement type");
                case ScriptDataType.MethodOrOperator:
                    HandleMethodStart(node);
                    break;
                case ScriptDataType.ReferenceGet:
                    break;
                case ScriptDataType.AI:
                case ScriptDataType.AIScript:
                case ScriptDataType.Device:
                case ScriptDataType.EntityIdentifier:
                case ScriptDataType.Entity:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.List:
                    HandleStaticFieldAccess(node);
                    break;
                case ScriptDataType.Float:
                case ScriptDataType.Int:
                case ScriptDataType.String:
                case ScriptDataType.Short:
                case ScriptDataType.Boolean:
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
            var methodName = GetScriptString(node);

            IScriptGenerationState newState = methodName switch
            {
                "begin" => new BeginCallData(),
                _ => new MethodCallData(methodName),
            };

            stateData.Push(newState);
        }

        private void HandleMethodEnd()
        {
            var state = stateData.Pop() switch
            {
                BeginCallData b => currentState.AddExpression(b.GenerateInvocationStatement()),
                MethodCallData m => currentState.AddExpression(m.GenerateInvocationExpression()),
            };
        }

        private void HandleLiteral(ScenarioTag.ScriptSyntaxNode node)
        {
            var literal = SyntaxUtil.LiteralExpression(scenario, node);

            currentState.AddExpression(literal);
        }

        private void HandleVariableAccess(ScenarioTag.ScriptSyntaxNode node)
        {
            var access = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression(),
                IdentifierName(GetScriptString(node)));

            currentState.AddExpression(access);
        }

        private void HandleStaticFieldAccess(ScenarioTag.ScriptSyntaxNode node)
        {
            var id = IdentifierName(GetScriptString(node));

            currentState.AddExpression(id);
        }

        private void HandleNodeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.ExpressionScope:
                    HandleExpressionScopeEnd(node);
                    break;
                case NodeType.Statement:
                    HandleStatementEnd(node);
                    break;
                case NodeType.VariableAccess:
                    break;
            }
        }

        private void HandleExpressionScopeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.DataType == ScriptDataType.StatementStart)
            {
                var endingScope = stateData.Pop() as ScopeBlockData;

                Debug.Assert(endingScope != null, "The ending scope wasn't a ScopeBlockData");

                if (endingScope.Statements.Count == 0)
                    return;

                ExpressionSyntax scopeExpression;

                if (endingScope.Statements.Count == 1
                    && endingScope.Statements[0].ChildNodes().Count() == 1
                    && endingScope.Statements[0].ChildNodes().First() is ExpressionSyntax ex)
                {
                    scopeExpression = ex;
                }
                else
                {
                    scopeExpression = InvocationExpression(
                        ParenthesizedLambdaExpression(
                            Block(List(endingScope.Statements))));
                }

                currentState.AddExpression(scopeExpression);
            }
        }

        private void HandleStatementEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            switch (node.DataType)
            {
                case ScriptDataType.MethodOrOperator:
                    HandleMethodEnd();
                    break;
                case ScriptDataType.AI:
                case ScriptDataType.AIScript:
                case ScriptDataType.ReferenceGet:
                case ScriptDataType.Device:
                case ScriptDataType.EntityIdentifier:
                case ScriptDataType.Boolean:
                case ScriptDataType.Short:
                case ScriptDataType.String:
                case ScriptDataType.Entity:
                case ScriptDataType.StatementStart:
                case ScriptDataType.Float:
                case ScriptDataType.Int:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.List:
                    break;
            }
        }

        public string Generate()
        {
            var cls = classDecl;

            bool addedRegion = false;
            foreach (var field in fields)
            {
                var f = field;
                if (addedRegion == false)
                {
                    var regionToken = Trivia(
                                RegionDirectiveTrivia(false).WithEndOfDirectiveToken(
                                    Token(TriviaList(PreprocessingMessage("Fields")),
                                        SyntaxKind.EndOfDirectiveToken,
                                        TriviaList())));

                    f = f.InsertTriviaBefore(f.GetLeadingTrivia().First(), new[] { regionToken });
                    addedRegion = true;
                }

                cls = cls.AddMembers(f);
            }

            bool addedFieldRegionEnd = false;
            foreach (var method in methods)
            {
                var m = method;
                if (addedFieldRegionEnd == false)
                {
                    m = m.InsertTriviaBefore(m.GetLeadingTrivia().First(), new[]{ 
                        Trivia(
                            EndRegionDirectiveTrivia(false))});

                    addedFieldRegionEnd = true;
                }

                cls = cls.AddMembers(m);
            }

            var ns = nsDecl.WithMembers(List<MemberDeclarationSyntax>().Add(cls));

            var csharpTree = CSharpSyntaxTree.Create(ns.NormalizeWhitespace());

            //var compilation = CSharpCompilation.Create("OpenH2ScriptGen", new[] { csharpTree });
            //compilation.Emit()

            return csharpTree.ToString();
        }

        private string GetScriptString(ScenarioTag.ScriptSyntaxNode node)
        {
            return ((Span<byte>)scenario.ScriptStrings).ReadStringStarting(node.NodeString);
        }
    }
}
