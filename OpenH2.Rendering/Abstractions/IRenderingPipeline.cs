using System.Numerics;
using OpenH2.Core.Tags;
using OpenH2.Foundation;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Accumulates resources for each frame and orders/groups/batches calls to the <see cref="IGraphicsAdapter"/>
    /// </summary>
    public interface IRenderingPipeline<TMaterialMap>
    {
        void AddStaticModel(Model<TMaterialMap> model);
        void AddTerrain(ScenarioTag.Terrain terrain);
        void AddSkybox(object skybox);
        void AddPointLight(Vector3 position, Vector3 color, float intensity);

        //void AddUI();

        void DrawAndFlush();
    }
}
