using OpenH2.Core.Maps;

namespace OpenH2.Core.Offsets
{
    public class PrimaryOffset : IOffset
    {
        private IH2Map scene;
        private int offset;

        public PrimaryOffset(IH2Map scene, int offsetValue)
        {
            this.scene = scene;
            this.offset = offsetValue;
        }

        public int Value => this.scene.PrimaryMagic + this.offset;

        public int OriginalValue => this.offset;
    }
}