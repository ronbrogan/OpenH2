using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering.Pipelines
{
    public class RenderPasses
    {
        public RenderPasses(ICollection<(Model<BitmapTag>, Matrix4x4)> renderables)
        {
            Diffuse = new List<(Model<BitmapTag>, Matrix4x4)>(renderables.Count);
            ShadowInteractables = new List<(Model<BitmapTag>, Matrix4x4)>(renderables.Count);

            foreach (var renderable in renderables)
            {
                // TODO figure out why this can be null
                if (renderable.Item1 == null)
                    continue;

                if (renderable.Item1.Flags.HasFlag(ModelFlags.IsSkybox))
                {
                    Skyboxes.Add(renderable);
                }

                if (renderable.Item1.Flags.HasFlag(ModelFlags.CastsShadows) || renderable.Item1.Flags.HasFlag(ModelFlags.ReceivesShadows))
                {
                    ShadowInteractables.Add(renderable);
                }

                if (renderable.Item1.Flags.HasFlag(ModelFlags.Diffuse))
                {
                    Diffuse.Add(renderable);
                }

                if (renderable.Item1.Flags.HasFlag(ModelFlags.IsTransparent))
                {
                    Transparent.Add(renderable);
                }
            }
        }

        // Assume small amount of Skybox and Transparent objects
        public List<(Model<BitmapTag>, Matrix4x4)> Skyboxes = new List<(Model<BitmapTag>, Matrix4x4)>();
        public List<(Model<BitmapTag>, Matrix4x4)> Transparent = new List<(Model<BitmapTag>, Matrix4x4)>();
        public List<(Model<BitmapTag>, Matrix4x4)> ShadowInteractables;
        public List<(Model<BitmapTag>, Matrix4x4)> Diffuse;
    }
}
