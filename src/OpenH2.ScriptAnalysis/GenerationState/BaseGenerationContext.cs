using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public abstract class BaseGenerationContext 
    {
        public virtual ScenarioTag.ScriptSyntaxNode OriginalNode { get; }
        public virtual bool CreatesScope => false;
        public virtual ScriptDataType? OwnDataType => null;
        public IReadOnlyList<object> Metadata => _metadata;
        private List<object> _metadata = new List<object>();

        public BaseGenerationContext(ScenarioTag.ScriptSyntaxNode node)
        {
            this.OriginalNode = node;
        }

        public virtual void AddMetadata(object metadata)
        {
            this._metadata.Add(metadata);
        }
    }
}
