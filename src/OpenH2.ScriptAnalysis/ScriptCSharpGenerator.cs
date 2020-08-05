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
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    

    public delegate void GenerationCallback(GenerationCallbackArgs args);
    public class GenerationCallbackArgs
    {
        public ScenarioTag.ScriptSyntaxNode Node { get; set; }
        public Scope Scope { get; set; }
    }


    /*
     * TODO:
     * - Create project with generated .cs files and/or compile and make warnings/errors available
     * - Sanitize/normalize property names, there are invalid identifiers, duplicates, and empties
     */
    public class ScriptCSharpGenerator
    { 
        private readonly ScenarioTag scenario;
        private readonly ClassDeclarationSyntax classDecl;
        private readonly NamespaceDeclarationSyntax nsDecl;
        private readonly List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        private readonly List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();
        private readonly List<PropertyDeclarationSyntax> properties = new List<PropertyDeclarationSyntax>();

        private Scope currentScope => scopes.Peek();
        private Stack<Scope> scopes;
        private ContinuationStack<int> childIndices;
        private List<(ScenarioTag.ScriptSyntaxNode, IGenerationContext)> pendingGenerators = new List<(ScenarioTag.ScriptSyntaxNode, IGenerationContext)>();

        public const string EngineImplementationClass = "OpenH2.Engine.Scripting.ScriptEngine";

        public event GenerationCallback OnNodeEnd;

        public ScriptCSharpGenerator(ScenarioTag scnr, string[] refrences = null, AttributeSyntax[] classAttributes = null)
        {
            this.scenario = scnr;
            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var ns = "OpenH2.Engine.Scripts.Generated" + string.Join('.', scenarioParts.Take(2));

            this.nsDecl = NamespaceDeclaration(ParseName(ns));

            var classModifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword))
                .Add(Token(SyntaxKind.PartialKeyword));            

            this.classDecl = ClassDeclaration("scnr_" + scenarioParts.Last())
                .WithModifiers(classModifiers);

            if(classAttributes != null && classAttributes.Any())
            {
                var classAttrs = List<AttributeListSyntax>()
                    .Add(AttributeList(SeparatedList(classAttributes)));

                this.classDecl = this.classDecl.WithAttributeLists(classAttrs);
            }
        }

        internal void AddPublicProperty(ScenarioTag.AiOrderDefinition order)
        {
            AddPublicProperty(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.AIOrders), order.Description);
        }

        internal void AddPublicProperty(ScenarioTag.AiReference ai)
        {
            AddPublicProperty(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.AI), ai.Description);
        }

        internal void AddPublicProperty(ScenarioTag.CameraPathTarget cam)
        {
            AddPublicProperty(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.CameraPathTarget), cam.Description);
        }

        internal void AddPublicProperty(ScenarioTag.WellKnownItem externalRef)
        {
            // TODO: type from external ref
            AddPublicProperty(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.Entity), externalRef.Description);
        }

        internal void AddPublicProperty(TypeSyntax type, string name)
        {
            properties.Add(PropertyDeclaration(type, name)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(SyntaxUtil.AutoPropertyAccessorList()));
        }

        public void AddGlobalVariable(ScenarioTag.ScriptVariableDefinition variable)
        {
            Debug.Assert(variable.Value_H16 < scenario.ScriptSyntaxNodes.Length, "Variable expression is not valid");

            var node = scenario.ScriptSyntaxNodes[variable.Value_H16];

            Debug.Assert(node.Checkval == variable.Value_L16, "Variable expression checkval did not match");


            var expressionContext = new SingleExpressionStatementContext(null, variable.DataType);
            var defScope = new Scope(variable.DataType, expressionContext, expressionContext);
            var retScope = EvaluateNodes(variable.Value_H16, defScope);

            Debug.Assert(retScope == defScope, "Returned scope was not the provided root");

            fields.Add(SyntaxUtil.CreateFieldDeclaration(variable, expressionContext.GetInnerExpression()));
        }

        public void AddMethod(ScenarioTag.ScriptMethodDefinition scriptMethod)
        {
            var modifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword));

            var method = MethodDeclaration(
                    SyntaxUtil.ScriptTypeSyntax(scriptMethod.ReturnType),
                    SyntaxUtil.SanitizeIdentifier(scriptMethod.Description))
                .WithModifiers(modifiers);

            // Push root method body as first scope
            var block = new StatementBlockContext();
            var rootScope = new Scope(scriptMethod.ReturnType, block, block);
            var retScope = EvaluateNodes(scriptMethod.SyntaxNodeIndex, rootScope);

            Debug.Assert(rootScope == retScope, "Last scope wasn't the provided root");

            methods.Add(method.WithBody(Block(block.GetInnerStatements())));
        }

        private Scope EvaluateNodes(int rootIndex, Scope rootScope)
        {
            scopes = new Stack<Scope>();
            scopes.Push(rootScope);

            childIndices = new ContinuationStack<int>();
            childIndices.PushFull(rootIndex);

            while (childIndices.TryPop(out var currentIndex, out var isContinuation))
            {
                var node = scenario.ScriptSyntaxNodes[currentIndex];

                if (isContinuation == false)
                {
                    var newContext = HandleNodeStart(node);
                    if(newContext.CreatesScope)
                    {
                        var newScope = currentScope.CreateChild(newContext);
                        scopes.Push(newScope);
                    }
                    else
                    {
                        newContext.GenerateInto(currentScope);
                    }
                }
                else
                {
                    HandleNodeEnd(node);
                }
            }

            var topScope = scopes.Pop();

            Debug.Assert(scopes.Count == 0, "Extra scopes on stack");

            return topScope;
        }

        private void PushNext(ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.NextIndex != ushort.MaxValue)
            {
                Debug.Assert(scenario.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                childIndices.PushFull(node.NextIndex);
            }
        }

        private IGenerationContext HandleNodeStart(ScenarioTag.ScriptSyntaxNode node)
        {
            return node.NodeType switch
            {
                NodeType.Scope => HandleScopeStart(node),
                NodeType.Expression => HandleExpression(node),
                NodeType.ScriptInvocation => HandleScriptInvocation(node),
                NodeType.VariableAccess => HandleVariableAccess(node),
                _ => throw new NotSupportedException($"Node type {node.NodeType} is not yet supported")
            }; 
        }

        // Scopes seems to use NodeData to specify what is inside the scope
        // and the Next value is used to specify the scope's next sibling instead
        // This is how the linear-ish node structure can expand into a more traditional AST
        private IGenerationContext HandleScopeStart(ScenarioTag.ScriptSyntaxNode node)
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

            // Create new scope, retaining current nearest StatementState
            //scopes.Push(new Scope(node.DataType, currentScope.StatementContext));

            //return NullGenerationContext.Instance;
            if(currentScope.IsInStatementContext)
            {
                return new ScopeGenerationContext.StatementContext(node, currentScope);
            }
            else
            {
                return new ScopeGenerationContext(node, currentScope);
            }
        }

        private IGenerationContext HandleExpression(ScenarioTag.ScriptSyntaxNode node)
        {
            PushNext(node);

            switch (node.DataType)
            {
                case ScriptDataType.Void:
                    // TODO: void expressions?
                    return NullGenerationContext.Instance;
                case ScriptDataType.MethodOrOperator:
                    return GetMethodState(node);
                case ScriptDataType.ReferenceGet:
                case ScriptDataType.Animation:
                case ScriptDataType.Weapon:
                case ScriptDataType.SpatialPoint:
                case ScriptDataType.WeaponReference:
                    return new ReferenceGetContext(scenario, node);
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
                    return new FieldGetContext(scenario, node);
                case ScriptDataType.Float:
                case ScriptDataType.Int:
                case ScriptDataType.String:
                case ScriptDataType.Short:
                case ScriptDataType.Boolean:
                    return new LiteralContext(scenario, node);
                default:
                    // TODO: hack until everything is tracked down, populating string as value if exists
                    return new UnknownContext(scenario, node);
            }
        }

        IGenerationContext HandleVariableAccess(ScenarioTag.ScriptSyntaxNode access)
        {
            PushNext(access);
            return new VariableAccessContext(scenario, access);
        }

        IGenerationContext HandleScriptInvocation(ScenarioTag.ScriptSyntaxNode invocation)
        {
            PushNext(invocation);
            return new ScriptInvocationContext(scenario, invocation);
        }

        private IGenerationContext GetMethodState(ScenarioTag.ScriptSyntaxNode node)
        {
            var methodName = GetScriptString(node);

            return GetMethodState(node, methodName);
        }

        private IGenerationContext GetMethodState(ScenarioTag.ScriptSyntaxNode node, string methodName)
        {
            ScriptDataType rt = currentScope.Type;

            return methodName switch
            {
                "begin" => new BeginCallContext(node, currentScope, rt),
                "-" =>  new BinaryOperatorContext(node, SyntaxKind.SubtractExpression),
                "+" =>  new BinaryOperatorContext(node, SyntaxKind.AddExpression),
                "*" =>  new BinaryOperatorContext(node, SyntaxKind.MultiplyExpression),
                "/" =>  new BinaryOperatorContext(node, SyntaxKind.DivideExpression),
                "%" =>  new BinaryOperatorContext(node, SyntaxKind.ModuloExpression),
                "=" =>  new BinaryOperatorContext(node, SyntaxKind.EqualsExpression),
                "<" =>  new BinaryOperatorContext(node, SyntaxKind.LessThanExpression),
                ">" =>  new BinaryOperatorContext(node, SyntaxKind.GreaterThanExpression),
                "<=" => new BinaryOperatorContext(node, SyntaxKind.LessThanOrEqualExpression),
                ">=" => new BinaryOperatorContext(node, SyntaxKind.GreaterThanOrEqualExpression),
                "or" => new BinaryOperatorContext(node, SyntaxKind.LogicalOrExpression),
                "and" => new BinaryOperatorContext(node, SyntaxKind.LogicalAndExpression),
                "not" => new UnaryOperatorContext(node, SyntaxKind.LogicalNotExpression),
                "if" => new IfStatementContext(node, currentScope),
                "set" => new FieldSetContext(node),
                _ => new MethodCallContext(node, methodName, rt),
            };
        }

        private void HandleNodeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
            this.OnNodeEnd?.Invoke(new GenerationCallbackArgs()
            {
                Node = node,
                Scope = currentScope
            });

            // Only generate into parent scope when the current scope ends
            if(node == currentScope.Context.OriginalNode)
            {
                var endingScope = scopes.Pop();

                endingScope.GenerateInto(currentScope);
            }
        }

        public string Generate()
        {
            var cls = classDecl;

            bool addedFieldRegionStart = false;
            bool addedFieldRegionEnd = false;

            foreach (var field in fields)
            {
                var f = field;
                if (addedFieldRegionStart == false)
                {
                    var regionToken = Trivia(
                                RegionDirectiveTrivia(false).WithEndOfDirectiveToken(
                                    Token(TriviaList(PreprocessingMessage("Fields")),
                                        SyntaxKind.EndOfDirectiveToken,
                                        TriviaList())));

                    f = f.InsertTriviaBefore(f.GetLeadingTrivia().First(), new[] { regionToken });
                    addedFieldRegionStart = true;
                }

                cls = cls.AddMembers(f);
            }

            foreach(var prop in properties)
            {
                cls = cls.AddMembers(prop);
            }
            
            foreach (var method in methods)
            {
                var m = method;
                if (addedFieldRegionStart && addedFieldRegionEnd == false)
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

            var csharpTree = CSharpSyntaxTree.Create(SyntaxUtil.Normalize(ns));

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
