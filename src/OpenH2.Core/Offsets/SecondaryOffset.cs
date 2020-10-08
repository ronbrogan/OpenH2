using OpenH2.Core.Maps;

namespace OpenH2.Core.Offsets
{
    public class SecondaryOffset : IOffset
    {
        private H2BaseMap scene;
        private int offset;

        public SecondaryOffset(H2BaseMap scene, int offsetValue)
        {
            this.offset = offsetValue;
            this.scene = scene;
        }

        public SecondaryOffset(int magic, int offsetValue)
        {
            this.offset = offsetValue;
            this.precalculatedValue = this.offset - magic;
        }

        // TODO: remove reliance the reference here to late bind the secondary magic
        private int? precalculatedValue = null;
        public int Value => precalculatedValue ?? this.offset - this.scene.SecondaryMagic;

        public int OriginalValue => this.offset;
    }
}