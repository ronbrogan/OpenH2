using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.OpenGL
{
    public static class OpenGLHelpers
    {
        public static void BufferShaderStorage<T>(ref int handle, ref T @struct, int? size = null) where T: struct
        {
            var s = size ?? Marshal.SizeOf(@struct);

            BufferObject(ref handle, ref @struct, BufferTarget.ShaderStorageBuffer, s);
        }

        public static void BufferShaderStorage<T>(ref int handle, ref T @struct) where T : struct, ISized
        {
            var s = @struct.SizeOf();

            BufferObject(ref handle, ref @struct, BufferTarget.ShaderStorageBuffer, s);
        }

        public static void BufferUniform<T>(ref int handle, ref T @struct, int? size = null) where T : struct
        {
            var s = size ?? Marshal.SizeOf(@struct);

            BufferObject(ref handle, ref @struct, BufferTarget.UniformBuffer, s);
        }

        private static void BufferObject<T>(ref int handle, ref T @struct, BufferTarget target, int size) where T : struct
        {
            if (handle == default)
            {
                GL.GenBuffers(1, out handle);
                GL.BindBuffer(target, handle);
                GL.BufferData(target, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(target, handle);
            }

            GL.BufferSubData(target, IntPtr.Zero, size, ref @struct);
            GL.BindBufferBase((BufferRangeTarget)(int)target, 2, handle);
            GL.BindBuffer(target, 0);
        }
    }
}
