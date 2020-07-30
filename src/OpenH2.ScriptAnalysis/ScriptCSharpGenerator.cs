using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    

    public delegate void GenerationCallback(GenerationCallbackArgs args);
    public class GenerationCallbackArgs
    {
        public ScenarioTag.ScriptSyntaxNode Node { get; set; }
        public IScriptGenerationState State { get; set; }
    }


    /*
     * TODO:
     * - Engine mock/stubs
     * - External reference lookups
     * - Create project with generated .cs files and/or compile and make warnings/errors available
     */
    public class ScriptCSharpGenerator
    { 
        private readonly ScenarioTag scenario;
        private readonly ClassDeclarationSyntax classDecl;
        private readonly NamespaceDeclarationSyntax nsDecl;
        private readonly List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        private readonly List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();

        private IScriptGenerationState currentState => stateData.Peek();
        private Stack<IScriptGenerationState> stateData;
        private ContinuationStack<int> childIndices;

        public const string EngineImplementationClass = "OpenH2.Engine.Scripting.ScriptEngine";

        public event GenerationCallback OnNodeEnd;

        public ScriptCSharpGenerator(ScenarioTag scnr, string[] refrences = null)
        {
            this.scenario = scnr;
            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var ns = "OpenH2.Engine.Scripts.Generated" + string.Join('.', scenarioParts.Take(2));

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

        public void AddGlobalVariable(ScenarioTag.ScriptVariableDefinition variable)
        {
            Debug.Assert(variable.Value_H16 < scenario.ScriptSyntaxNodes.Length, "Variable expression is not valid");

            var node = scenario.ScriptSyntaxNodes[variable.Value_H16];

            Debug.Assert(node.Checkval == variable.Value_L16, "Variable expression checkval did not match");

            var scope = EvaluateNodes(variable.Value_H16, new CastScopeData(variable.DataType)) as CastScopeData;

            Debug.Assert(scope != null, "Returned scope was not the correct type");

            fields.Add(SyntaxUtil.CreateFieldDeclaration(variable, scope.GenerateCastExpression()));
        }

        public void AddMethod(ScenarioTag.ScriptMethodDefinition scriptMethod)
        {
            var modifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword));

            var method = MethodDeclaration(
                    SyntaxUtil.ScriptTypeSyntax(scriptMethod.ReturnType),
                    SyntaxUtil.SanitizeIdentifier(scriptMethod.Description))
                .WithModifiers(modifiers);

            // Push root method body as first scope
            var scope = EvaluateNodes(scriptMethod.SyntaxNodeIndex, new ScopeBlockData()) as ScopeBlockData;

            Debug.Assert(scope != null, "Last scope wasn't a ScopeBlockData");

            methods.Add(method.WithBody(Block(List(scope.Statements))));

            Debug.Assert(stateData.Count == 0, "Extra scopes on stack");
        }

        private IScriptGenerationState EvaluateNodes(int rootIndex, IScriptGenerationState rootScope)
        {
            stateData = new Stack<IScriptGenerationState>();
            stateData.Push(rootScope);

            childIndices = new ContinuationStack<int>();
            childIndices.PushFull(rootIndex);

            while (childIndices.TryPop(out var currentIndex, out var isContinuation))
            {
                var node = scenario.ScriptSyntaxNodes[currentIndex];

                if (isContinuation == false)
                {
                    HandleNodeStart(node);
                }
                else
                {
                    HandleNodeEnd(node);
                }
            }

            return stateData.Pop();
        }

        private void PushNext(ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.NextIndex != ushort.MaxValue)
            {
                Debug.Assert(scenario.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                childIndices.PushFull(node.NextIndex);
            }
        }

        private void HandleNodeStart(ScenarioTag.ScriptSyntaxNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.ExpressionScope:
                    HandleExpressionScopeStart(node);
                    break;
                case NodeType.Statement:
                    HandleStatementStart(node);
                    break;
                case NodeType.VariableAccess:
                    HandleVariableAccess(node);
                    break;
                case NodeType.ScriptInvocation:
                    HandleScriptInvocation(node);
                    break;
                default:
                    throw new NotSupportedException($"Node type {node.NodeType} is not yet supported");
            }
        }

        // Expression scope seems to use NodeData to specify what is inside the scope
        // and the Next value is used to specify the scope's next sibling instead
        // This is how the linear-ish node structure can expand into a more traditional AST
        private void HandleExpressionScopeStart(ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.NextIndex != ushort.MaxValue)
            {
                // If we're not at top level, we need to push behind the current scope's continuation
                // This makes the 'next' into a 'sibling'
                bool repushOrig = childIndices.TryPop(out var orig, out var origCont);

                PushNext(node);

                if (repushOrig)
                {
                    Debug.Assert(origCont, "Popped scope wasn't a continuation");
                    childIndices.PushSeparate(orig, origCont);
                }
            }

            // These nodes should be inside of a new scope, so pushing here
            Debug.Assert(scenario.ScriptSyntaxNodes[node.NodeData_H16].Checkval == node.NodeData_L16, "Scope's next node checkval didn't match");
            childIndices.PushFull(node.NodeData_H16);

            if (node.DataType == ScriptDataType.Void)
            {
                stateData.Push(new ScopeBlockData());
            }
            else
            {
                // If it's not a StatementStart, it's effectively a data cast for the inside expression
                stateData.Push(new CastScopeData(node.DataType));
            }
        }

        private void HandleStatementStart(ScenarioTag.ScriptSyntaxNode node)
        {
            PushNext(node);

            switch (node.DataType)
            {
                case ScriptDataType.Void:
                    // TODO: void expressions?
                    break;
                case ScriptDataType.MethodOrOperator:
                    HandleMethodStart(node);
                    break;
                case ScriptDataType.ReferenceGet:
                case ScriptDataType.Animation:
                case ScriptDataType.Weapon:
                case ScriptDataType.SpatialPoint:
                case ScriptDataType.WeaponReference:
                    // TODO: implement GetReference or rework entirely
                    HandleMethodStart("GetReference");
                    HandleLiteral(node);
                    HandleMethodEnd();
                    break;
                case ScriptDataType.AI:
                case ScriptDataType.AIScript:
                case ScriptDataType.Device:
                case ScriptDataType.EntityIdentifier:
                case ScriptDataType.Entity:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.List:
                case ScriptDataType.StringId:
                case ScriptDataType.ScriptReference:
                case ScriptDataType.DeviceGroup:
                case ScriptDataType.AIOrders:
                case ScriptDataType.Bsp:
                case ScriptDataType.Effect:
                case ScriptDataType.LoopingSound:
                case ScriptDataType.GameDifficulty:
                case ScriptDataType.Unit:
                case ScriptDataType.Scenery:
                case ScriptDataType.VehicleSeat:
                case ScriptDataType.Equipment:
                case ScriptDataType.NavigationPoint:
                case ScriptDataType.Model:
                case ScriptDataType.Team:
                case ScriptDataType.Vehicle:
                case ScriptDataType.CameraPathTarget:
                case ScriptDataType.CinematicTitle:
                case ScriptDataType.AIBehavior:
                case ScriptDataType.Damage:
                case ScriptDataType.DamageState:
                    // TODO: create fields or some other access for these
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
                    HandleUnknown(node);
                    break;
            }
        }

        private void HandleVariableAccess(ScenarioTag.ScriptSyntaxNode node)
        {
            PushNext(node);

            var access = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression(),
                IdentifierName(
                    SyntaxUtil.SanitizeIdentifier(GetScriptString(node))));

            currentState.AddExpression(access);
            currentState.AddMetadata(node);
        }

        private void HandleScriptInvocation(ScenarioTag.ScriptSyntaxNode node)
        {
            PushNext(node);

            var invocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(
                        SyntaxUtil.SanitizeIdentifier(scenario.ScriptMethods[node.ScriptIndex].Description))));

            currentState.AddExpression(invocation);
            currentState.AddMetadata(node);
        }

        private void HandleUnknown(ScenarioTag.ScriptSyntaxNode node)
        {
            string unknownDescription = "";

            if (node.NodeString > 0 && node.NodeString < scenario.ScriptStrings.Length
                && scenario.ScriptStrings[node.NodeString - 1] == 0)
            {
                unknownDescription = GetScriptString(node);
            }

            unknownDescription += $" -{node.NodeType}<{node.DataType}>";
            currentState.AddExpression(InvocationExpression(IdentifierName("UNKNOWN"),
                ArgumentList(SingletonSeparatedList(Argument(SyntaxUtil.LiteralExpression(unknownDescription))))));
            currentState.AddMetadata(node);
        }

        private void HandleMethodStart(ScenarioTag.ScriptSyntaxNode node)
        {
            var methodName = GetScriptString(node);

            HandleMethodStart(methodName);
        }

        private void HandleMethodStart(string methodName)
        {
            ScriptDataType rt = (ScriptDataType)ushort.MaxValue;

            if(currentState is CastScopeData cast)
            {
                rt = cast.DestinationType;
            }
            else if (currentState is ScopeBlockData scope)
            {
                rt = ScriptDataType.Void;
            }

            IScriptGenerationState newState = methodName switch
            {
                "begin" => new BeginCallData(rt),
                "-" =>  new BinaryOperatorData(SyntaxKind.SubtractExpression),
                "+" =>  new BinaryOperatorData(SyntaxKind.AddExpression),
                "*" =>  new BinaryOperatorData(SyntaxKind.MultiplyExpression),
                "/" =>  new BinaryOperatorData(SyntaxKind.DivideExpression),
                "%" =>  new BinaryOperatorData(SyntaxKind.ModuloExpression),
                "=" =>  new BinaryOperatorData(SyntaxKind.EqualsExpression),
                "<" =>  new BinaryOperatorData(SyntaxKind.LessThanExpression),
                ">" =>  new BinaryOperatorData(SyntaxKind.GreaterThanExpression),
                "<=" => new BinaryOperatorData(SyntaxKind.LessThanOrEqualExpression),
                ">=" => new BinaryOperatorData(SyntaxKind.GreaterThanOrEqualExpression),
                "or" => new BinaryOperatorData(SyntaxKind.LogicalOrExpression),
                "and" => new BinaryOperatorData(SyntaxKind.LogicalAndExpression),
                "not" => new UnaryOperatorData(SyntaxKind.LogicalNotExpression),
                "if" => new IfStatementData(),
                "set" => new FieldSetData(),
                _ => new MethodCallData(methodName, rt),
            };

            stateData.Push(newState);
        }

        private void HandleMethodEnd()
        {
            _ = stateData.Pop() switch
            {
                BeginCallData b => b.Generate(currentState),
                BinaryOperatorData o => currentState.AddExpression(o.GenerateOperatorExpression()),
                UnaryOperatorData u => currentState.AddExpression(u.GenerateOperatorExpression()),
                MethodCallData m => currentState.AddExpression(m.GenerateInvocationExpression()),
                IfStatementData i => HoistOrInsertIf(i),
                FieldSetData s => HoistOrInsertSet(s)
            };
        }

        private IScriptGenerationState HoistOrInsertIf(IfStatementData statementData)
        {
            var hoisted = HoistIntoContainingScope(statementData, out var resultVariable);

            if(hoisted)
            {
                currentState.AddExpression(resultVariable);
            }

            return statementData;
        }

        private IScriptGenerationState HoistOrInsertSet(FieldSetData setData)
        {
            var hoisted = HoistIntoContainingScope(setData, out var resultVariable);

            if (hoisted)
            {
                currentState.AddExpression(resultVariable);
            }

            return setData;
        }

        private bool HoistIntoContainingScope(IHoistableGenerationState hoistable, out ExpressionSyntax hoistedAccessor)
        {
            bool didHoist = false;
            var poppedScopes = new Stack<IScriptGenerationState>();

            while (typeof(IScopedScriptGenerationState).IsAssignableFrom(currentState.GetType()) == false)
            {
                poppedScopes.Push(stateData.Pop());
            }

            var currentBlock = currentState as IScopedScriptGenerationState;
            Debug.Assert(currentBlock != null, $"Current scope is not a {nameof(IScopedScriptGenerationState)}");

            StatementSyntax[] statements;

            if (poppedScopes.Count == 0)
            {
                statements = hoistable.GenerateNonHoistedStatements();
                hoistedAccessor = default;
            }
            else
            {
                statements = hoistable.GenerateHoistedStatements(out hoistedAccessor);
                didHoist = true;
            }

            foreach (var statement in statements)
            {
                currentBlock.AddStatement(statement);
            }

            while (poppedScopes.TryPop(out var popped))
            {
                stateData.Push(popped);
            }

            return didHoist;
        }

        private void HandleLiteral(ScenarioTag.ScriptSyntaxNode node)
        {
            var literal = SyntaxUtil.LiteralExpression(scenario, node);

            currentState.AddExpression(literal);
            currentState.AddMetadata(node);
        }

        private void HandleStaticFieldAccess(ScenarioTag.ScriptSyntaxNode node)
        {
            if(node.NodeString == 0)
            {
                currentState.AddExpression(
                    DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType)));
            }
            else
            {
                currentState.AddExpression(
                    IdentifierName(SyntaxUtil.SanitizeIdentifier(GetScriptString(node))));
            }

            currentState.AddMetadata(node);
        }

        #region NodeEnd

        private void HandleNodeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            this.OnNodeEnd(new GenerationCallbackArgs()
            {
                Node = node,
                State = currentState
            });

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
                case NodeType.ScriptInvocation:

                    break;
            }
        }

        private void HandleExpressionScopeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.DataType == ScriptDataType.Void)
            {
                var endingScope = stateData.Pop() as ScopeBlockData;
                Debug.Assert(endingScope != null, "The ending void scope wasn't a ScopeBlockData");

                if (endingScope.Statements.Count == 0)
                    return;

                if (currentState is IScopedScriptGenerationState currentBlock)
                {
                    foreach (var statement in endingScope.Statements)
                    {
                        currentBlock.AddStatement(statement);
                    }
                }
                else if (endingScope.Statements.Count == 1
                    && endingScope.Statements[0].ChildNodes().Count() == 1
                    && endingScope.Statements[0].ChildNodes().First() is ExpressionSyntax ex)
                {
                    currentState.AddExpression(ex);
                }
                else
                {
                    currentState.AddExpression(InvocationExpression(
                        ParenthesizedLambdaExpression(
                            Block(List(endingScope.Statements)))));
                }

                
            }
            else
            {
                var endingScope = stateData.Pop() as CastScopeData;
                Debug.Assert(endingScope != null, "The ending scope wasn't a CastScopeData");

                currentState.AddExpression(endingScope.GenerateCastExpression());
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
                case ScriptDataType.Void:
                case ScriptDataType.Float:
                case ScriptDataType.Int:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.List:
                    break;
            }
        }

        #endregion

        public string Generate()
        {
            var cls = classDecl;

            bool addedFieldRegion = false;
            foreach (var field in fields)
            {
                var f = field;
                if (addedFieldRegion == false)
                {
                    var regionToken = Trivia(
                                RegionDirectiveTrivia(false).WithEndOfDirectiveToken(
                                    Token(TriviaList(PreprocessingMessage("Fields")),
                                        SyntaxKind.EndOfDirectiveToken,
                                        TriviaList())));

                    f = f.InsertTriviaBefore(f.GetLeadingTrivia().First(), new[] { regionToken });
                    addedFieldRegion = true;
                }

                cls = cls.AddMembers(f);
            }

            bool addedFieldRegionEnd = false;
            foreach (var method in methods)
            {
                var m = method;
                if (addedFieldRegion && addedFieldRegionEnd == false)
                {
                    m = m.InsertTriviaBefore(m.GetLeadingTrivia().First(), new[]{ 
                        Trivia(
                            EndRegionDirectiveTrivia(false))});

                    addedFieldRegionEnd = true;
                }

                cls = cls.AddMembers(m);
            }

            var ns = nsDecl
                .AddMembers(cls)
                .AddUsings(
                    UsingDirective(ParseName("System")),
                    UsingDirective(ParseName("OpenH2.Core.Scripting")),
                    UsingDirective(Token(SyntaxKind.StaticKeyword), null, ParseName(EngineImplementationClass))
                );

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
