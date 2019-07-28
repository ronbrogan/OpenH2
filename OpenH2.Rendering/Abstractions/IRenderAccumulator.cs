using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Accumulates resources for each frame and orders/groups/batches calls to the <see cref="IGraphicsAdapter"/>
    /// </summary>
    public interface IRenderAccumulator<TMaterialMap>
    {
        void AddRigidBody(Mesh model, IMaterial<TMaterialMap> mat);
        void AddTerrain(Scenario.Terrain terrain);
        void AddSkybox(object skybox);

        //void AddUI();

        void DrawAndFlush();
    }
}
