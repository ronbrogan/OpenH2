using OpenH2.Core.Architecture;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Systems
{
    public class BspSystem : WorldSystem
    {
        private InputStore inputStore;

        private Dictionary<int, List<Entity>> bspEntities = new();

        private BitArray loadedBsps = new BitArray(0);
        private BitArray bspsToLoad = new BitArray(0);
        private BitArray bspsToUnload = new BitArray(0);


        public BspSystem(World world) : base(world)
        {
        }

        public void SwitchBsp(int desiredIndex, bool toggle)
        {
            if(desiredIndex >= loadedBsps.Length)
            {
                Logger.Log($"BSP[{desiredIndex}] does not exist", Logger.Color.Red);
                return;
            }

            if(toggle)
            {
                if (this.loadedBsps[desiredIndex])
                {
                    this.bspsToUnload[desiredIndex] = true;
                }
                else
                {
                    this.bspsToLoad[desiredIndex] = true;
                }
            }
            else
            {
                for (int i = 0; i < loadedBsps.Length; i++)
                {
                    if (i == desiredIndex) 
                        continue;

                    if(loadedBsps[i]) 
                        bspsToUnload[i] = true;
                }

                if (this.loadedBsps[desiredIndex] == false)
                {
                    bspsToLoad[desiredIndex] = true;
                }
            }
        }

        public override void Initialize(Scene scene)
        {
            bspEntities.Clear();
            this.inputStore = this.world.GetGlobalResource<InputStore>();

            var terrains = scene.Scenario.Terrains;

            this.loadedBsps = new BitArray(terrains.Length);
            this.bspsToLoad = new BitArray(terrains.Length);
            this.bspsToUnload = new BitArray(terrains.Length);

            for (int i = 0; i < terrains.Length; i++)
            {
                var terrain = terrains[i];
                var entities = new List<Entity>();

                var bsp = scene.Map.GetTag(terrain.Bsp);

                entities.Add(scene.EntityCreator.FromBsp(bsp));

                foreach (var instance in bsp.InstancedGeometryInstances)
                {
                    entities.Add(scene.EntityCreator.FromInstancedGeometry(bsp, instance));
                }

                // Find appropriate skybox
                if(terrain.SkyIndex >= 0 && terrain.SkyIndex < scene.Map.Scenario.SkyboxInstances.Length)
                {
                    var sky = scene.Map.Scenario.SkyboxInstances[terrain.SkyIndex];

                    if(sky.Skybox.IsInvalid == false)
                    {
                        entities.Add(scene.EntityCreator.FromSkyboxInstance(sky));
                    }
                }

                scene.GatherPlacedEntities(i, entities);

                this.bspEntities[i] = entities;
            }

            // Load BSP 0
            this.bspsToLoad[0] = true;
        }

        public override void Update(double timestep)
        {
            PopulateSwitchCommandFromKeys();

            for (int i = 0; i < this.bspsToUnload.Length; i++)
            {
                if(this.bspsToUnload[i])
                {
                    this.bspsToUnload[i] = false;

                    foreach (var e in bspEntities[i])
                        world.Scene.RemoveEntity(e);

                    this.loadedBsps[i] = false;
                }
            }

            for (int i = 0; i < this.bspsToLoad.Length; i++)
            {
                if (this.bspsToLoad[i])
                {
                    this.bspsToLoad[i] = false;

                    foreach (var e in bspEntities[i])
                        world.Scene.AddEntity(e);

                    this.loadedBsps[i] = true;
                }
            }
        }

        private void PopulateSwitchCommandFromKeys()
        {
            var bspIndex = -1;

            if (this.inputStore.WasPressed(Keys.KeyPad0))
            {
                bspIndex = 0;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad1))
            {
                bspIndex = 1;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad2))
            {
                bspIndex = 2;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad3))
            {
                bspIndex = 3;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad4))
            {
                bspIndex = 4;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad5))
            {
                bspIndex = 5;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad6))
            {
                bspIndex = 6;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad7))
            {
                bspIndex = 7;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad8))
            {
                bspIndex = 8;
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad9))
            {
                bspIndex = 9;
            }

            if(bspIndex >= 0)
            {
                var toggle = this.inputStore.IsDown(Keys.LeftControl)
                || this.inputStore.IsDown(Keys.RightControl);

                SwitchBsp(bspIndex, toggle);
            }
        }
    }
}
