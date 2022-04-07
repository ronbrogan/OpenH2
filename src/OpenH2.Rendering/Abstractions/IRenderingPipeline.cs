using System.Collections.Generic;
using OpenH2.Foundation;
using OpenH2.Rendering.Shaders;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Accumulates resources for each frame and orders/groups/batches calls to the <see cref="IGraphicsAdapter"/>
    /// </summary>
    public interface IRenderingPipeline<TMaterialMap>
    {
        //void SetModels(IList<(Model<TMaterialMap>, Matrix4x4)> models);
        void SetModels(List<DrawGroup> models);
        void AddPointLight(PointLight light);

        //void AddUI();

        void DrawAndFlush();
        void SetGlobals(GlobalUniform matrices);
    }
}
