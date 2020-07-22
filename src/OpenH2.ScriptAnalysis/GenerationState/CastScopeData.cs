using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class CastScopeData : IScriptGenerationState
    {
        private readonly ScriptDataType destinationType;

        private ExpressionSyntax scopedExpression = null;

        public CastScopeData(ScriptDataType destinationType)
        {
            this.destinationType = destinationType;
        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
        {
            Debug.Assert(this.scopedExpression == null, "Scoped expression contains multiple expressions");

            this.scopedExpression = expression;

            return this;
        }
        
        public ExpressionSyntax GenerateCastExpression()
        {
            // Temporarily making this not a blind explicit cast
            // It adds a lot of noise to the scripts
            // Future work can detect inner expression type and conditionally cast
            // My preference at the moment is to allow functions to accept the various params 
            //  or add implicit casts to the param types
            //return SyntaxFactory.CastExpression(SyntaxUtil.ScriptTypeSyntax(this.destinationType),
            //    SyntaxFactory.ParenthesizedExpression(this.scopedExpression));
            return scopedExpression;
        }
    }
}
