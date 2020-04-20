using System.Numerics;

namespace OpenH2.Foundation
{
    public class Face
    {
        public Vector3 Normal { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public Vector3 Position { get; private set; }

        public Face(Vector3[] verts)
        {
            this.Vertices = verts;
            var cross = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]);
            this.Normal = Vector3.Normalize(cross);

            var acc = Vector3.Zero;
            foreach(var vert in verts)
            {
                acc += vert;
            }

            this.Position = acc / verts.Length;
        }
    }
}
