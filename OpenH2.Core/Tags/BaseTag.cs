using OpenH2.Core.Parsing;

namespace OpenH2.Core.Tags
{
    public abstract class BaseTag
    {
        public abstract string Name { get; set; }

        public uint Id { get; private set; }

        public uint Offset { get; set; }
        
        public uint Length { get; set; }

#if DEBUG
        public int InternalSecondaryMagic { get; set; }
#endif

        public BaseTag(uint id)
        {
            this.Id = id;
        }

        public virtual void PopulateExternalData(H2vReader reader) { }
    }
}
