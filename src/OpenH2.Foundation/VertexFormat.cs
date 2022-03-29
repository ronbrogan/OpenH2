using OpenH2.Foundation.Extensions;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Foundation
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexFormat
    {
        public static readonly int PositionOffset;
        public static readonly int TexCoordsOffset;
        public static readonly int NormalOffset;
        public static readonly int TangentOffset;
        public static readonly int BitangentOffset;


        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        static VertexFormat()
        {
            var o = default(VertexFormat);
            
            PositionOffset = UnsafeExtensions.Offset(ref o, ref o.Position);
            TexCoordsOffset = UnsafeExtensions.Offset(ref o, ref o.TexCoords);
            NormalOffset = UnsafeExtensions.Offset(ref o, ref o.Normal);
            TangentOffset = UnsafeExtensions.Offset(ref o, ref o.Tangent);
            BitangentOffset = UnsafeExtensions.Offset(ref o, ref o.Bitangent);
        }

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
