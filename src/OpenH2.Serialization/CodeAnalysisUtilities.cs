using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace OpenH2.Serialization
{
    internal static class CodeAnalysisUtilities
    {
        public static INamedTypeSymbol GetTypeSymbol<T>(this Compilation compilation)
        {
            return compilation.GetTypeSymbol(typeof(T));
        }

        public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, Type t)
        {
            return compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .Select(a => a.GetTypeByMetadataName(t.FullName))
                .Single(a => a != null);
        }
    }
}
