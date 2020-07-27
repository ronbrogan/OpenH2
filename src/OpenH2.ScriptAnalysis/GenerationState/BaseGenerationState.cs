using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public abstract class BaseGenerationState 
    {
        public IReadOnlyList<object> Metadata => _metadata;
        private List<object> _metadata = new List<object>();

        public virtual void AddMetadata(object metadata)
        {
            this._metadata.Add(metadata);
        }
    }
}
