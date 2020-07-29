using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IHoistableGenerationState
    {
        StatementSyntax[] GenerateHoistedStatements(out ExpressionSyntax hoistedAccessor);
        StatementSyntax[] GenerateNonHoistedStatements();
    }
}
