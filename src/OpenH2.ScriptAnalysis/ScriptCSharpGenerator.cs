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
        private readonly List<StatementSyntax> constructorStatements = new List<StatementSyntax>();
        private readonly List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
        private readonly List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();
        private readonly List<PropertyDeclarationSyntax> properties = new List<PropertyDeclarationSyntax>();
        private readonly List<ClassDeclarationSyntax> nestedDataClasses = new List<ClassDeclarationSyntax>();

        private readonly MemberNameRepository nameRepo;

        private Scope currentScope => scopes.Peek();
        private Stack<Scope> scopes;
        private ContinuationStack<int> childIndices;

        public const string EngineImplementationClass = "OpenH2.Engine.Scripting.ScriptEngine";

        public ScriptCSharpGenerator(ScenarioTag scnr, MemberNameRepository nameRepo, string[] refrences = null, AttributeSyntax[] classAttributes = null)
        {
            this.scenario = scnr;
            this.nameRepo = nameRepo;
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

        internal void AddPublicProperty(ScenarioTag.AiOrderDefinition order, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.AIOrders, order.Description, itemIndex);
        }

        // Make squads into nested static classes
        internal void AddSquadData(ScenarioTag tag)
        {
            for (var i = 0; i < tag.AiSquadDefinitions.Length; i++)
            {
                var squad = tag.AiSquadDefinitions[i];

                var squadPropName = nameRepo.RegisterName(squad.Description, ScriptDataType.AI.ToString(), i);

                var dataClassProps = new List<PropertyDeclarationSyntax>();

                var nestedRepo = new MemberNameRepository();

                var m = 0;
                foreach (var ai in squad.StartingLocations)
                {
                    var propName = nestedRepo.RegisterName(ai.Description, ScriptDataType.AI.ToString(), m++);

                    dataClassProps.Add(SyntaxUtil.CreateProperty(ScriptDataType.AI, propName));
                }

                // This is so the script can reference the squad itself, need special init handling
                dataClassProps.Add(SyntaxUtil.CreateProperty(ScriptDataType.AI, "Squad"));

                var squadTypeName = "Squad_" + squadPropName;

                var cls = ClassDeclaration(squadTypeName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithMembers(new SyntaxList<MemberDeclarationSyntax>(dataClassProps))
                    .AddMembers(
                        ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), SyntaxUtil.ScriptTypeSyntax(ScriptDataType.AI))
                            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                            .WithOperatorKeyword(Token(SyntaxKind.OperatorKeyword))
                            .AddParameterListParameters(Parameter(Identifier("s")).WithType(ParseTypeName(squadTypeName)))
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("s"),
                                        IdentifierName("Squad"))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

                nestedDataClasses.Add(cls);
                nameRepo.NestedRepos.Add(squadPropName, nestedRepo);

                properties.Add(PropertyDeclaration(ParseTypeName(squadTypeName), squadPropName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(SyntaxUtil.AutoPropertyAccessorList()));
            }            
        }

        internal void AddPublicProperty(ScenarioTag.AiSquadGroupDefinition ai, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.AI, ai.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.CameraPathTarget cam, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.CameraPathTarget, cam.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.LocationFlagDefinition flag, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.LocationFlag, flag.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.CinematicTitleDefinition title, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.CinematicTitle, title.Title, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.TriggerVolume tv, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.Trigger, tv.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.WellKnownItem externalRef, int itemIndex)
        {
            var varType = externalRef.ItemType switch
            {
                ScenarioTag.WellKnownVarType.Unit => ScriptDataType.Unit,
                ScenarioTag.WellKnownVarType.Vehicle => ScriptDataType.Vehicle,
                ScenarioTag.WellKnownVarType.Weapon => ScriptDataType.Weapon,
                ScenarioTag.WellKnownVarType.Scenery => ScriptDataType.Scenery,
                ScenarioTag.WellKnownVarType.Machinery => ScriptDataType.Device,
                ScenarioTag.WellKnownVarType.Controller => ScriptDataType.Device,
                _ => ScriptDataType.Entity
            };

            AddPublicProperty(varType, externalRef.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.StartingProfileDefinition profile, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.Equipment, profile.Description, itemIndex);
        }

        internal void AddPublicProperty(ScenarioTag.DeviceGroupDefinition group, int itemIndex)
        {
            AddPublicProperty(ScriptDataType.DeviceGroup, group.Description, itemIndex);
        }

        internal void AddPublicProperty(ScriptDataType type, string name, int itemIndex)
        {
            var propName = nameRepo.RegisterName(name, type.ToString(), itemIndex);

            properties.Add(SyntaxUtil.CreateProperty(type, propName));
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

            var field = SyntaxUtil.CreateField(variable);
            fields.Add(field);

            var assignment = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(field.Declaration.Variables.First().Identifier.ToString())), 
                expressionContext.GetInnerExpression());

            constructorStatements.Add(ExpressionStatement(assignment));
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

            methods.Add(method.WithBody(Block(block.GetInnerStatements()))
                .AddAttributeLists(AttributeList(SeparatedList(new[] {
                    Attribute(IdentifierName(nameof(ScriptMethodAttribute).Replace("Attribute", "")))
                        .AddArgumentListArguments(AttributeArgument(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(nameof(Lifecycle)),
                            IdentifierName(scriptMethod.Lifecycle.ToString()))))
                }))));
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
                case ScriptDataType.Bsp:
                case ScriptDataType.Model:
                case ScriptDataType.LoopingSound:
                case ScriptDataType.Effect:
                case ScriptDataType.Damage:
                    return new ReferenceGetContext(scenario, node);
                case ScriptDataType.AI:
                    return new AiGetContext(scenario, node, nameRepo);
                case ScriptDataType.AIScript:
                case ScriptDataType.Device:
                case ScriptDataType.EntityIdentifier:
                case ScriptDataType.Entity:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.List:
                case ScriptDataType.ScriptReference:
                case ScriptDataType.DeviceGroup:
                case ScriptDataType.AIOrders:
                case ScriptDataType.Unit:
                case ScriptDataType.Scenery:
                case ScriptDataType.Equipment:
                case ScriptDataType.Team:
                case ScriptDataType.Vehicle:
                case ScriptDataType.CameraPathTarget:
                case ScriptDataType.CinematicTitle:
                case ScriptDataType.AIBehavior:
                case ScriptDataType.DamageState:
                    return new FieldGetContext(scenario, node, nameRepo);
                case ScriptDataType.NavigationPoint:
                    return new NavigationPointContext(scenario, node);
                case ScriptDataType.Float:
                case ScriptDataType.Int:
                case ScriptDataType.String:
                case ScriptDataType.Short:
                case ScriptDataType.Boolean:
                case ScriptDataType.StringId:
                case ScriptDataType.GameDifficulty:
                case ScriptDataType.VehicleSeat:
                    return new LiteralContext(scenario, node, currentScope);
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
                "begin_random" => new BeginRandomContext(node, rt),
                "sleep_until" => new SleepUntilContext(node, rt),
                "-" =>  new BinaryOperatorContext(node, SyntaxKind.SubtractExpression, rt),
                "+" =>  new BinaryOperatorContext(node, SyntaxKind.AddExpression, rt),
                "*" =>  new BinaryOperatorContext(node, SyntaxKind.MultiplyExpression, rt),
                "/" =>  new BinaryOperatorContext(node, SyntaxKind.DivideExpression, rt),
                "%" =>  new BinaryOperatorContext(node, SyntaxKind.ModuloExpression, rt),
                "=" =>  new BinaryOperatorContext(node, SyntaxKind.EqualsExpression, rt),
                "<" =>  new BinaryOperatorContext(node, SyntaxKind.LessThanExpression, rt),
                ">" =>  new BinaryOperatorContext(node, SyntaxKind.GreaterThanExpression, rt),
                "<=" => new BinaryOperatorContext(node, SyntaxKind.LessThanOrEqualExpression, rt),
                ">=" => new BinaryOperatorContext(node, SyntaxKind.GreaterThanOrEqualExpression, rt),
                "or" => new BinaryOperatorContext(node, SyntaxKind.LogicalOrExpression, rt),
                "and" => new BinaryOperatorContext(node, SyntaxKind.LogicalAndExpression, rt),
                "not" => new UnaryOperatorContext(node, SyntaxKind.LogicalNotExpression),
                "if" => new IfStatementContext(node, currentScope),
                "set" => new FieldSetContext(node),
                _ => new MethodCallContext(node, methodName, rt),
            };
        }

        private void HandleNodeEnd(ScenarioTag.ScriptSyntaxNode node)
        {
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

            if (constructorStatements.Any())
            {
                cls = cls.AddMembers(ConstructorDeclaration(cls.Identifier)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(constructorStatements)));
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


            foreach (var nested in nestedDataClasses)
            {
                cls = cls.AddMembers(nested);
            }

            cls = cls.AddBaseListTypes(SimpleBaseType(ParseTypeName("ScenarioScriptBase")));

            var ns = nsDecl
                .AddMembers(cls)
                .AddUsings(
                    UsingDirective(ParseName("System")),
                    UsingDirective(ParseName("OpenH2.Core.Scripting")),
                    UsingDirective(ParseName("OpenH2.Engine.Scripting")),
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
