namespace OpenH2.Core.Tags
{
    public abstract class BaseTag
    {
        public abstract string Name { get; set; }

        public readonly uint Id;

        public BaseTag(uint id)
        {
            this.Id = id;
        }
    }
}
