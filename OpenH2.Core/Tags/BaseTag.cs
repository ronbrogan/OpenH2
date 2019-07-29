using OpenH2.Core.Parsing;

namespace OpenH2.Core.Tags
{
    public abstract class BaseTag
    {
        public abstract string Name { get; set; }

        public readonly uint Id;

        public BaseTag()
        {
        }

        public BaseTag(uint id)
        {
            this.Id = id;
        }

        public virtual void PopulateExternalData(H2vReader reader) { }
    }
}
