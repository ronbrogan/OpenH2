using OpenH2.Foundation;
using System.Numerics;

namespace OpenH2.Rendering
{
    public struct DrawGroupSingle
    {
        public ModelFlags Flags;
        public Matrix4x4 Transform;
        public DrawCommand DrawCommand;
    }

    public struct DrawGroup
    {
        public ModelFlags Flags;
        public Matrix4x4 Transform;
        public DrawCommand[] DrawCommands;
    }
}
