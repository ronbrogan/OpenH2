using OpenH2.Foundation;
using System.Runtime.CompilerServices;

namespace OpenH2.Rendering.Pipelines
{
    public static class RenderPasses
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSkybox(DrawGroup model)
        {
            return (model.Flags & ModelFlags.IsSkybox) == ModelFlags.IsSkybox;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsShadowInteractable(DrawGroup model)
        {
            return (model.Flags & ModelFlags.CastsShadows) == ModelFlags.CastsShadows
                || (model.Flags & ModelFlags.ReceivesShadows) == ModelFlags.ReceivesShadows;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiffuse(DrawGroup model)
        {
            return (model.Flags & ModelFlags.Diffuse) == ModelFlags.Diffuse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWireframe(DrawGroup model)
        {
            return (model.Flags & ModelFlags.Wireframe) == ModelFlags.Wireframe;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDebugviz(DrawGroup model)
        {
            return (model.Flags & ModelFlags.DebugViz) == ModelFlags.DebugViz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTransparent(DrawGroup model)
        {
            return (model.Flags & ModelFlags.IsTransparent) == ModelFlags.IsTransparent;
        }
    }
}
