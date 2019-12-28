using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering.Pipelines
{
    public static class RenderPasses
    {
        public static bool IsSkybox(Model<BitmapTag> model)
        {
            return model?.Flags.HasFlag(ModelFlags.IsSkybox) ?? false;
        }

        public static bool IsShadowInteractable(Model<BitmapTag> model)
        {
            if (model == null) return false;

            return model.Flags.HasFlag(ModelFlags.CastsShadows) || model.Flags.HasFlag(ModelFlags.ReceivesShadows);
        }

        public static bool IsDiffuse(Model<BitmapTag> model)
        {
            return model?.Flags.HasFlag(ModelFlags.Diffuse) ?? false;
        }

        public static bool IsTransparent(Model<BitmapTag> model)
        {
            return model?.Flags.HasFlag(ModelFlags.IsTransparent) ?? false;
        }
    }
}
