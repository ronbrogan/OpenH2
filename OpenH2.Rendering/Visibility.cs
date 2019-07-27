using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Rendering.Abstractions;

namespace OpenH2.Rendering
{
    public class Visibility
    {
        private readonly IRenderAccumulator accumulator;

        public Visibility(IRenderAccumulator accumulator)
        {
            this.accumulator = accumulator;
        }

        // TODO create interface to expose items instead of Visibility using Scenario directly?
        public void AccumulateVisibleItems(Scene scene)
        {
            //foreach(var terrain in scene.Terrains)
            //{
            //    accumulator.AddTerrain(terrain);
            //}

            //foreach(var skybox in scenario.Skybox)
            //{
            //    accumulator.AddSkybox(skybox);
            //}
        }
    }
}
