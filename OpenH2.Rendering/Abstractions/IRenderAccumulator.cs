using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Tags;

namespace OpenH2.Rendering.Abstractions
{
    /// <summary>
    /// Accumulates resources for each frame and orders/groups/batches calls to the <see cref="IGraphicsAdapter"/>
    /// </summary>
    public interface IRenderAccumulator
    {
        void AddRigidBody(object model);
        void AddTerrain(Scenario.Terrain terrain);
        void AddSkybox(object skybox);

        void DrawAndFlush();
    }
}
