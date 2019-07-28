using OpenH2.Core.Representations;

namespace OpenH2.Core.Offsets
{
    public class SecondaryOffset : IOffset
    {
        private H2vMap scene;
        private int offset;

        public SecondaryOffset(H2vMap scene, int offsetValue)
        {
            this.offset = offsetValue;
            this.scene = scene;
        }
        
        // TODO: remove reliance the reference here to late bind the secondary magic
        public int Value => this.offset - this.scene.SecondaryMagic;

        public int OriginalValue => this.offset;
    }
}