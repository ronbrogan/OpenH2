using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Extensions
{
    public static class MapExtensions
    {
        public static PrimaryOffset PrimaryOffset(this H2vMap scene, int value)
        {
            return new PrimaryOffset(scene, value);
        }
    }
}