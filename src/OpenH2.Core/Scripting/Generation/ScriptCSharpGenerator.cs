using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Scripting.GenerationState;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Scripting.Generation
{
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
        private static Dictionary<string, PropertyInfo> ScenarioTagProperties = typeof(ScenarioTag).GetProperties()
            .ToDictionary(p => p.Name);

        private Scope currentScope => scopes.Peek();
        private Stack<Scope> scopes;
        private ContinuationStack<int> childIndices;

        public ScriptCSharpGenerator(ScenarioTag scnr, MemberNameRepository nameRepo, string[] refrences = null, AttributeSyntax[] classAttributes = null)
        {
            this.scenario = scnr;
            this.nameRepo = nameRepo;

            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            var ns = "OpenH2.Scripts.Generated" + string.Join(".", scenarioParts.Take(2));

            this.nsDecl = NamespaceDeclaration(ParseName(ns));

            var classModifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword))
                .Add(Token(SyntaxKind.PartialKeyword));

            this.classDecl = ClassDeclaration("scnr_" + scenarioParts.Last())
                .WithModifiers(classModifiers);

            if (classAttributes != null && classAttributes.Any())
            {
                var classAttrs = List<AttributeListSyntax>()
                    .Add(AttributeList(SeparatedList(classAttributes)));

                this.classDecl = this.classDecl.WithAttributeLists(classAttrs);
            }
        }

        public void AddProperties(ScenarioTag scnr)
        {
            for (int i = 0; i < scnr.WellKnownItems.Length; i++)
            {
                var externalRef = scnr.WellKnownItems[i];

                if (externalRef.ItemType == ScenarioTag.WellKnownVarType.Undef)
                    continue;

                var varType = SyntaxUtil.WellKnownTypeSyntax(externalRef.ItemType);

                AddWellKnownPublicProperty(varType, externalRef.Identifier, i);
            }

            for (int i = 0; i < scnr.CameraPathTargets.Length; i++)
            {
                var cam = scnr.CameraPathTargets[i];
                AddPublicProperty(ScriptDataType.CameraPathTarget, cam.Description, i);
            }

            CreateNestedSquadClasses(scnr);

            for (int i = 0; i < scnr.AiSquadGroupDefinitions.Length; i++)
            {
                var ai = scnr.AiSquadGroupDefinitions[i];
                AddPublicProperty(ScriptDataType.AI, ai.Description, i);
            }

            for (int i = 0; i < scnr.AiOrderDefinitions.Length; i++)
            {
                var order = scnr.AiOrderDefinitions[i];
                AddPublicProperty(ScriptDataType.AIOrders, order.Description, i);
            }

            for (int i = 0; i < scnr.LocationFlagDefinitions.Length; i++)
            {
                var flag = scnr.LocationFlagDefinitions[i];
                AddPublicProperty(ScriptDataType.LocationFlag, flag.Description, i);
            }

            for (int i = 0; i < scnr.CinematicTitleDefinitions.Length; i++)
            {
                var title = scnr.CinematicTitleDefinitions[i];
                AddPublicProperty(ScriptDataType.CinematicTitle, title.Title, i);
            }

            for (int i = 0; i < scnr.TriggerVolumes.Length; i++)
            {
                var tv = scnr.TriggerVolumes[i];
                AddPublicProperty(ScriptDataType.Trigger, tv.Description, i);
            }

            for (int i = 0; i < scnr.StartingProfileDefinitions.Length; i++)
            {
                var profile = scnr.StartingProfileDefinitions[i];
                AddPublicProperty(ScriptDataType.StartingProfile, profile.Description, i);
            }

            for (int i = 0; i < scnr.DeviceGroupDefinitions.Length; i++)
            {
                var group = scnr.DeviceGroupDefinitions[i];
                AddPublicProperty(ScriptDataType.DeviceGroup, group.Description, i);
            }
        }

        // Make squads into nested classes
        public void CreateNestedSquadClasses(ScenarioTag tag)
        {
            for (var i = 0; i < tag.AiSquadDefinitions.Length; i++)
            {
                var squad = tag.AiSquadDefinitions[i];

                var squadPropName = nameRepo.RegisterName(squad.Description, ScriptDataType.AI.ToString(), i);

                var dataClassProps = new List<PropertyDeclarationSyntax>();
                dataClassProps.Add(SyntaxUtil.CreateProperty(ParseTypeName(nameof(ScenarioTag)), nameof(ScenarioTag)));

                var nestedRepo = new MemberNameRepository();

                var m = 0;
                foreach (var ai in squad.StartingLocations)
                {
                    var propName = nestedRepo.RegisterName(ai.Description, ScriptDataType.AI.ToString(), m++);

                    ExpressionSyntax startingLocationAccess = ElementAccessExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            ElementAccessExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(nameof(ScenarioTag)),
                                    IdentifierName(nameof(tag.AiSquadDefinitions))))
                            .AddArgumentListArguments(Argument(SyntaxUtil.LiteralExpression(i))),
                            IdentifierName(nameof(squad.StartingLocations))))
                        .AddArgumentListArguments(Argument(SyntaxUtil.LiteralExpression(m)));

                    dataClassProps.Add(PropertyDeclaration(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.AI), propName)
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithExpressionBody(ArrowExpressionClause(startingLocationAccess))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                }

                ExpressionSyntax squadAccess = ElementAccessExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(nameof(ScenarioTag)),
                        IdentifierName(nameof(tag.AiSquadDefinitions))))
                    .AddArgumentListArguments(Argument(SyntaxUtil.LiteralExpression(i)));

                // This is so the script can reference the squad itself, need special init handling
                dataClassProps.Add(PropertyDeclaration(SyntaxUtil.ScriptTypeSyntax(ScriptDataType.AI), "Squad")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(squadAccess))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

                var squadTypeName = "Squad_" + squadPropName;

                var cls = ClassDeclaration(squadTypeName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithMembers(new SyntaxList<MemberDeclarationSyntax>(dataClassProps))
                    .AddMembers(ConstructorDeclaration(squadTypeName)
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            Parameter(Identifier(nameof(ScenarioTag)))
                                .WithType(ParseTypeName(nameof(ScenarioTag))))
                        .WithBody(Block(new List<StatementSyntax>()
                        {
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(nameof(ScenarioTag))),
                                IdentifierName(nameof(ScenarioTag))))
                        })));

                nestedDataClasses.Add(cls);
                nameRepo.NestedRepos.Add(squadPropName, nestedRepo);

                properties.Add(PropertyDeclaration(ParseTypeName(squadTypeName), squadPropName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithAccessorList(SyntaxUtil.AutoPropertyAccessorList()));
            }
        }

        public void CreateDataInitializer(ScenarioTag scnr)
        {
            var scenarioParam = Identifier("scenarioTag");
            var sceneParam = Identifier("scene");

            var statements = new List<StatementSyntax>();

            statements.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName(nameof(ScenarioScriptBase.Scenario))),
                IdentifierName(scenarioParam))));

            for (int i = 0; i < scnr.WellKnownItems.Length; i++)
            {
                var externalRef = scnr.WellKnownItems[i];

                if (externalRef.ItemType != ScenarioTag.WellKnownVarType.Undef && externalRef.Index != ushort.MaxValue)
                {
                    var varType = SyntaxUtil.WellKnownTypeSyntax(externalRef.ItemType);
                    statements.Add(CreateWellKnownAssignment(varType, externalRef.Identifier, i));
                }
            }

            AddSquadInitialization(statements, scnr);

            this.methods.Add(
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    nameof(ScenarioScriptBase.InitializeData))
                .AddParameterListParameters(
                    Parameter(scenarioParam).WithType(ParseTypeName(nameof(ScenarioTag))),
                    Parameter(sceneParam).WithType(ParseTypeName(nameof(Scene))))
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
                .WithBody(Block(statements)));
        }

        private void AddSquadInitialization(List<StatementSyntax> statements, ScenarioTag tag)
        {
            for (var i = 0; i < tag.AiSquadDefinitions.Length; i++)
            {
                var squad = tag.AiSquadDefinitions[i];

                if (nameRepo.TryGetName(squad.Description, ScriptDataType.AI.ToString(), i, out var squadPropName) == false)
                {
                    return;
                }

                var squadTypeName = "Squad_" + squadPropName;

                var squadItem = ObjectCreationExpression(ParseTypeName(squadTypeName))
                    .AddArgumentListArguments(Argument(IdentifierName("scenarioTag")));
                var squadItemAssignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(squadPropName),
                    squadItem);
                statements.Add(ExpressionStatement(squadItemAssignment));
            }
        }

        private void AddWellKnownPublicProperty(TypeSyntax type, string name, int itemIndex)
        {
            var propName = nameRepo.RegisterName(name, nameof(ScenarioTag.EntityReference), itemIndex);

            var propType = GenericName(nameof(ScenarioEntity<object>))
                .AddTypeArgumentListArguments(type);

            var prop = PropertyDeclaration(propType, propName)
                .WithAccessorList(SyntaxUtil.AutoPropertyAccessorList());

            properties.Add(prop.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))));
        }

        private void AddPublicProperty(ScriptDataType type, string name, int itemIndex)
        {
            var propName = nameRepo.RegisterName(name, type.ToString(), itemIndex);

            var scnrPropName = type switch
            {
                ScriptDataType.CameraPathTarget => nameof(ScenarioTag.CameraPathTargets),
                ScriptDataType.LocationFlag => nameof(ScenarioTag.LocationFlagDefinitions),
                ScriptDataType.CinematicTitle => nameof(ScenarioTag.CinematicTitleDefinitions),
                ScriptDataType.Trigger => nameof(ScenarioTag.TriggerVolumes),
                ScriptDataType.StartingProfile => nameof(ScenarioTag.StartingProfileDefinitions),
                ScriptDataType.DeviceGroup => nameof(ScenarioTag.DeviceGroupDefinitions),
                ScriptDataType.AI => nameof(ScenarioTag.AiSquadGroupDefinitions),
                ScriptDataType.AIOrders => nameof(ScenarioTag.AiOrderDefinitions),
                _ => throw new Exception("Not support!")
            };

            ExpressionSyntax access = ElementAccessExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(nameof(ScenarioScriptBase.Scenario)),
                        IdentifierName(scnrPropName)))
                    .AddArgumentListArguments(Argument(SyntaxUtil.LiteralExpression(itemIndex)));

            var scnrProp = ScenarioTagProperties[scnrPropName];
            if (scnrProp.PropertyType.HasElementType &&
                scnrProp.PropertyType.GetElementType().GetInterfaces().Any(i => 
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameObjectDefinition<>)))
            {
                access = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    access,
                    IdentifierName(nameof(IGameObjectDefinition<object>.GameObject)));
            }

            var prop = PropertyDeclaration(SyntaxUtil.ScriptTypeSyntax(type), propName)
                .WithExpressionBody(ArrowExpressionClause(access))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            

            properties.Add(prop.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))));
        }

        public StatementSyntax CreateWellKnownAssignment(TypeSyntax type,
            string desiredName,
            int itemIndex)
        {
            if (nameRepo.TryGetName(desiredName, nameof(ScenarioTag.EntityReference), itemIndex, out var name) == false)
            {
                throw new Exception("Unable to find property name");
            }

            ExpressionSyntax access = ElementAccessExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("scenarioTag"),
                            IdentifierName(nameof(ScenarioTag.WellKnownItems))))
                        .AddArgumentListArguments(Argument(SyntaxUtil.LiteralExpression(itemIndex)));

            access = ObjectCreationExpression(
                GenericName(nameof(ScenarioEntity<object>))
                    .AddTypeArgumentListArguments(type))
                .AddArgumentListArguments(
                    Argument(SyntaxUtil.LiteralExpression(itemIndex)),
                    Argument(access));

            var exp = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(name),
                access);

            return ExpressionStatement(exp);
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
            var modifiers = SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword))
                .Add(Token(SyntaxKind.AsyncKeyword));

            TypeSyntax returnType = ParseTypeName("Task");

            if (scriptMethod.ReturnType != ScriptDataType.Void)
            {
                returnType = GenericName("Task").AddTypeArgumentListArguments(SyntaxUtil.ScriptTypeSyntax(scriptMethod.ReturnType));
            }

            var method = MethodDeclaration(
                    returnType,
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
                    if (newContext.CreatesScope)
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

            if (currentScope.IsInStatementContext)
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
                case ScriptDataType.Sound:
                case ScriptDataType.Animation:
                case ScriptDataType.TagReference:
                case ScriptDataType.Model:
                case ScriptDataType.LoopingSound:
                case ScriptDataType.Effect:
                case ScriptDataType.DamageEffect:
                    return new TagGetContext(scenario, node);
                case ScriptDataType.Bsp:
                case ScriptDataType.SpatialPoint:
                case ScriptDataType.WeaponReference:
                    return new ReferenceGetContext(scenario, node);
                case ScriptDataType.AI:
                    return new AiGetContext(scenario, node, nameRepo);
                case ScriptDataType.EntityIdentifier:
                case ScriptDataType.Entity:
                case ScriptDataType.Unit:
                case ScriptDataType.Scenery:
                case ScriptDataType.Device:
                case ScriptDataType.Vehicle:
                case ScriptDataType.List:
                    return new EntityGetContext(scenario, node, nameRepo);
                case ScriptDataType.AIScript:
                case ScriptDataType.ScriptReference:
                case ScriptDataType.Trigger:
                case ScriptDataType.LocationFlag:
                case ScriptDataType.DeviceGroup:
                case ScriptDataType.AIOrders:
                case ScriptDataType.StartingProfile:
                case ScriptDataType.Team:
                case ScriptDataType.CameraPathTarget:
                case ScriptDataType.CinematicTitle:
                case ScriptDataType.AIBehavior:
                case ScriptDataType.DamageState:
                case ScriptDataType.NavigationPoint:
                    return new FieldGetContext(scenario, node, nameRepo);
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
                "-" => new BinaryOperatorContext(node, SyntaxKind.SubtractExpression, rt),
                "+" => new BinaryOperatorContext(node, SyntaxKind.AddExpression, rt),
                "*" => new BinaryOperatorContext(node, SyntaxKind.MultiplyExpression, rt),
                "/" => new BinaryOperatorContext(node, SyntaxKind.DivideExpression, rt),
                "%" => new BinaryOperatorContext(node, SyntaxKind.ModuloExpression, rt),
                "=" => new BinaryOperatorContext(node, SyntaxKind.EqualsExpression, rt),
                "<" => new BinaryOperatorContext(node, SyntaxKind.LessThanExpression, rt),
                ">" => new BinaryOperatorContext(node, SyntaxKind.GreaterThanExpression, rt),
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
            if (node == currentScope.Context.OriginalNode)
            {
                var endingScope = scopes.Pop();

                endingScope.GenerateInto(currentScope);
            }
        }

        public NamespaceDeclarationSyntax Generate()
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

            foreach (var prop in properties)
            {
                cls = cls.AddMembers(prop);
            }

            if (constructorStatements.Any())
            {
                constructorStatements.Insert(0, ExpressionStatement(AssignmentExpression(
                   SyntaxKind.SimpleAssignmentExpression,
                   MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                       ThisExpression(),
                       IdentifierName("Engine")),
                   IdentifierName("scriptEngine")
               )));


                cls = cls.AddMembers(ConstructorDeclaration(cls.Identifier)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .AddParameterListParameters(Parameter(Identifier("scriptEngine")).WithType(ParseName("IScriptEngine")))
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
                    UsingDirective(ParseName("System.Threading.Tasks")),
                    UsingDirective(ParseName("OpenH2.Core.Architecture")),
                    UsingDirective(ParseName("OpenH2.Core.Tags")),
                    UsingDirective(ParseName("OpenH2.Core.Tags.Scenario")),
                    UsingDirective(ParseName("OpenH2.Core.Scripting")),
                    UsingDirective(ParseName("OpenH2.Core.GameObjects"))
                );

            return (NamespaceDeclarationSyntax)SyntaxUtil.Normalize(ns);
        }

        private string GetScriptString(ScenarioTag.ScriptSyntaxNode node)
        {
            return ((Span<byte>)scenario.ScriptStrings).ReadStringStarting(node.NodeString);
        }
    }
}
