using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Extensions
{
    public static class SceneExtensions
    {
        public static PrimaryOffset PrimaryOffset(this Scene scene, int value)
        {
            return new PrimaryOffset(scene, value);
        }
    }
}