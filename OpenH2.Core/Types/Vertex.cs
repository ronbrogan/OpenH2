using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Core.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;
    }
}
