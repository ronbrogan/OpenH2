using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenH2.Rendering.Pipelines
{
    public static class RenderPasses
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSkybox(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.IsSkybox) == ModelFlags.IsSkybox;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsShadowInteractable(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.CastsShadows) == ModelFlags.CastsShadows
                || (model.Flags & ModelFlags.ReceivesShadows) == ModelFlags.ReceivesShadows;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiffuse(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.Diffuse) == ModelFlags.Diffuse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWireframe(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.Wireframe) == ModelFlags.Wireframe;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDebugviz(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.DebugViz) == ModelFlags.DebugViz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTransparent(Model<BitmapTag> model)
        {
            Debug.Assert(model != null);

            return (model.Flags & ModelFlags.IsTransparent) == ModelFlags.IsTransparent;
        }
    }
}
