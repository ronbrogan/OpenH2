using OpenH2.Core.Architecture;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Systems
{
    public class BspSystem : WorldSystem
    {
        private InputStore inputStore;
        private int loadedIndex;
        private int? switchToIndex;

        private Dictionary<int, List<Entity>> bspEntities = new();

        public BspSystem(World world) : base(world)
        {
        }

        public void SwitchToBsp(int desiredIndex)
        {
            if(loadedIndex != desiredIndex)
            {
                switchToIndex = desiredIndex;
            }
        }

        public override void Initialize(Scene scene)
        {
            bspEntities.Clear();
            this.inputStore = this.world.GetGlobalResource<InputStore>();

            var terrains = scene.Scenario.Terrains;

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

                this.bspEntities[i] = entities;
            }

            // Load BSP 0
            this.loadedIndex = 0;
            foreach (var e in bspEntities[this.loadedIndex])
                this.world.Scene.AddEntity(e);
        }

        public override void Update(double timestep)
        {
            PopulateSwitchCommandFromKeys();

            if (switchToIndex.HasValue)
            {
                if(bspEntities.TryGetValue(this.switchToIndex.Value, out var addEntities))
                {
                    var desiredIndex = switchToIndex.Value;
                    switchToIndex = null;

                    foreach (var e in bspEntities[this.loadedIndex])
                        world.Scene.RemoveEntity(e);

                    foreach (var e in addEntities)
                        world.Scene.AddEntity(e);

                    this.loadedIndex = desiredIndex;
                }
                else
                {
                    Logger.Log($"BSP[{this.switchToIndex}] does not exist", Logger.Color.Red);
                    this.switchToIndex = null;
                    return;
                }
            }
        }

        private void PopulateSwitchCommandFromKeys()
        {
            if (this.inputStore.WasPressed(Keys.KeyPad0))
            {
                SwitchToBsp(0);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad1))
            {
                SwitchToBsp(1);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad2))
            {
                SwitchToBsp(2);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad3))
            {
                SwitchToBsp(3);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad4))
            {
                SwitchToBsp(4);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad5))
            {
                SwitchToBsp(5);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad6))
            {
                SwitchToBsp(6);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad7))
            {
                SwitchToBsp(7);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad8))
            {
                SwitchToBsp(8);
            }
            else if (this.inputStore.WasPressed(Keys.KeyPad9))
            {
                SwitchToBsp(9);
            }
        }
    }
}
