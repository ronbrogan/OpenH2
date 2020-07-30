using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IExpressionContext
    {
        IReadOnlyList<object> Metadata { get; }

        IExpressionContext AddExpression(ExpressionSyntax expression);

        void AddMetadata(object metadata);

        void GenerateInto(Scope scope);
    }

    public class NullExpressionContext : IExpressionContext
    {
        public static IExpressionContext Instance { get; } = new NullExpressionContext();

        private NullExpressionContext() { }

        public IReadOnlyList<object> Metadata => Array.Empty<object>();

        public IExpressionContext AddExpression(ExpressionSyntax expression) => this;

        public void AddMetadata(object metadata) { }

        public void GenerateInto(Scope scope) { }
    }
}