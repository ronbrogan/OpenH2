using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Foundation
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexFormat
    {
        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public VertexFormat(Vector3 pos, Vector2 tex, Vector3 norm)
        {
            Position = pos;
            TexCoords = tex;
            Normal = norm;
            Tangent = Vector3.One;
            Bitangent = Vector3.One;
        }

        public VertexFormat(Vector3 pos, Vector2 tex, Vector3 norm, Vector3 tan, Vector3 bitan)
        {
            Position = pos;
            TexCoords = tex;
            Normal = norm;
            Tangent = tan;
            Bitangent = bitan;
        }

        public static readonly int Size = Marshal.SizeOf<VertexFormat>();
    }
}
