using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;

namespace OpenH2.Core.Scripting.GenerationState
{
    public abstract class BaseGenerationContext 
    {
        public virtual ScenarioTag.ScriptSyntaxNode OriginalNode { get; }
        public virtual bool CreatesScope => false;
        public virtual bool SuppressHoisting => false;
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
