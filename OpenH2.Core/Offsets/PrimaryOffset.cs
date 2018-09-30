using OpenH2.Core.Representations;

namespace OpenH2.Core.Offsets
{
    public class PrimaryOffset : IOffset
    {
        private Scene scene;
        private int offset;

        public PrimaryOffset(Scene scene, int offsetValue)
        {
            this.scene = scene;
            this.offset = offsetValue;
        }

        public int Value => this.scene.PrimaryMagic + this.offset;

        public int OriginalValue => this.offset;
    }
}