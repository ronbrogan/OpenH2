using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders.Voxelize
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VoxelizeUniform
    {
        public long VoxelTextureHandle;

        public static int Size => OpenTK.BlittableValueType<VoxelizeUniform>.Stride;
    }
}
