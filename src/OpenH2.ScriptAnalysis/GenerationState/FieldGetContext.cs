using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class FieldGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly ExpressionSyntax accessor;

        public override ScriptDataType? OwnDataType { get; }

        public FieldGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node, MemberNameRepository nameRepo) : base(node)
        {
            this.OwnDataType = node.DataType;

            if (node.NodeString == 0)
            {
                accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
            }
            else
            {
                var stringVal = SyntaxUtil.GetScriptString(scenario, node);

                if(stringVal == "none")
                {
                    accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
                }
                else
                {
                    var scope = string.Empty;
                    var name = stringVal;

                    if(stringVal.Equals("lz_pelican_02", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Whoa");
                    }

                    if(stringVal.Contains('.'))
                    {
                        var sepIndex = stringVal.LastIndexOf('.');
                        scope = stringVal.Substring(0, sepIndex);
                        name = stringVal.Substring(sepIndex + 1);
                    }

                    if(nameRepo.TryGetName(scope, name, node.DataType.ToString(), node.NodeData_H16, out var finalName))
                    {
                        accessor = SyntaxFactory.IdentifierName(finalName);
                    }
                    else
                    {
                        accessor = SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(name));
                    }
                }
            }
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(accessor);
        }
    }
}
