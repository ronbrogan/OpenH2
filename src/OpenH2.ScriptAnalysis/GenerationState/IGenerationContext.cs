using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IGenerationContext
    {
        IReadOnlyList<object> Metadata { get; }
        bool CreatesScope { get; }
        ScriptDataType? OwnDataType { get; }
        ScenarioTag.ScriptSyntaxNode OriginalNode { get; }
        IGenerationContext AddExpression(ExpressionSyntax expression);
        void AddMetadata(object metadata);
        void GenerateInto(Scope scope);
    }

    public class NullGenerationContext : IGenerationContext
    {
        public static IGenerationContext Instance { get; } = new NullGenerationContext();

        private NullGenerationContext() { }

        public bool CreatesScope => false;
        public ScriptDataType? OwnDataType => null;

        public IReadOnlyList<object> Metadata => Array.Empty<object>();

        public ScenarioTag.ScriptSyntaxNode OriginalNode => null;

        public IGenerationContext AddExpression(ExpressionSyntax expression) => this;

        public void AddMetadata(object metadata) { }

        public void GenerateInto(Scope scope) { }
    }
}