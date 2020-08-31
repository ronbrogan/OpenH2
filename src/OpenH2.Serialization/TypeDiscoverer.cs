using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace OpenH2.Serialization
{
    internal class TypeDiscoverer : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Types { get; } = new List<TypeDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDecl)
            {
                this.Types.Add(typeDecl);
            }
        }
    }
}
