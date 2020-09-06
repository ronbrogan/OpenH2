using OpenH2.Core.Architecture;
using OpenH2.Core.Configuration;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Components.Globals;
using OpenH2.Engine.Entities;
using OpenH2.Engine.EntityFactories;
using OpenH2.Engine.Systems;
using OpenH2.Foundation;
using OpenH2.Foundation.Engine;
using OpenH2.Physics.Core;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenToolkit.Graphics.ES30;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine
{
    public class Engine
    {
        IGraphicsHost graphicsHost;
        IGameLoopSource gameLoop;
        Func<GameWindow> gameWindowGetter;

        private World world;

        public Engine()
        {
            var host = new OpenGLHost();
            gameWindowGetter = host.GetWindow;

            graphicsHost = host;
            gameLoop = host;
        }

        public void Start(EngineStartParameters parameters)
        {
            graphicsHost.CreateWindow(new Vector2(1600, 900));

            var mapPath = parameters.LoadPathOverride ?? @"D:\H2vMaps\lockout.map";
            var configPath = Environment.GetEnvironmentVariable(ConfigurationConstants.ConfigPathOverrideEnvironmentVariable);

            if (configPath != null)
            {
                configPath = Path.GetFullPath(configPath);
            }
            else
            {
                configPath = Environment.CurrentDirectory + "/Configs";
            }

            var matFactory = new MaterialFactory(configPath);

            var factory = new MapFactory(Path.GetDirectoryName(mapPath), matFactory);

            matFactory.AddListener(() =>
            {
                LoadScene(factory, mapPath);
            });

            var rtWorld = new RealtimeWorld(this, gameWindowGetter());
            rtWorld.UseGraphicsAdapter(graphicsHost.GetAdapter());

            world = rtWorld;

            var scenario = LoadScene(factory, mapPath);
            rtWorld.UseSystem(new ScriptSystem(scenario, rtWorld));

            gameLoop.RegisterCallbacks(world.Update, world.Render);
            gameLoop.Start(60, 60);
        }

        private ScenarioTag LoadScene(MapFactory factory, string mapPath)
        {
            SpectatorCamera camera = new SpectatorCamera();

            if(world.Scene != null)
            {
                camera = world.Scene.Entities.FirstOrDefault((v) => v.Value.GetType() == typeof(SpectatorCamera)).Value as SpectatorCamera;
            }

            var scene = new Scene();

            scene.AddEntity(camera);
            //scene.AddEntity(new Player(true));

            var watch = new Stopwatch();
            watch.Start();
            var scenario = LoadMap(scene, factory, mapPath);
            watch.Stop();
            Console.WriteLine($"Loading map took {watch.ElapsedMilliseconds / 1000f} seconds");

            world.LoadScene(scene);

            return scenario;
        }

        public ScenarioTag LoadMap(Scene destination, MapFactory factory, string mapPath)
        {
            var fs = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8096);
            var map = factory.FromFile(fs);

            LoadGlobals(destination, map);
            var scenario = LoadScenario(destination, map);

            //PositioningEntities.AddLocators(map, destination);

            //PlaceLights(destination);

            return scenario;
        }

        private static ScenarioTag LoadScenario(Scene destination, H2vMap map)
        {
            map.TryGetTag(map.IndexHeader.Scenario, out var scenario);
            var terrains = scenario.Terrains;

            foreach (var terrain in terrains)
            {
                map.TryGetTag(terrain.Bsp, out var bsp);

                destination.AddEntity(TerrainFactory.FromBspData(map, bsp));

                foreach (var instance in bsp.InstancedGeometryInstances)
                {
                    destination.AddEntity(SceneryFactory.FromInstancedGeometry(map, bsp, instance));
                }
            }

            foreach (var sky in scenario.SkyboxInstances)
            {
                if (sky.Skybox == uint.MaxValue)
                    continue;

                destination.AddEntity(SkyboxFactory.FromTag(map, scenario, sky));
            }

            foreach (var scen in scenario.SceneryInstances)
            {
                if (scen.SceneryDefinitionIndex == ushort.MaxValue)
                    continue;

                destination.AddEntity(SceneryFactory.FromTag(map, scenario, scen));
            }

            foreach (var bloc in scenario.BlocInstances)
            {
                destination.AddEntity(BlocFactory.FromTag(map, scenario, bloc));
            }

            foreach (var mach in scenario.MachineryInstances)
            {
                destination.AddEntity(MachineryFactory.FromTag(map, scenario, mach));
            }

            foreach (var item in scenario.ItemCollectionPlacements)
            {
                destination.AddEntity(ItemFactory.FromTag(map, scenario, item));
            }

            foreach (var item in scenario.VehicleInstances)
            {
                // HACK: sometimes maxval, headlong
                if (item.Index == ushort.MaxValue)
                    continue;

                destination.AddEntity(ItemFactory.CreateFromVehicleInstance(map, scenario, item));
            }

            foreach(var item in scenario.TriggerVolumes)
            {
                destination.AddEntity(TriggerFactory.FromScenarioTriggerVolume(scenario, item));
            }

            return scenario;
        }

        private static void LoadGlobals(Scene destination, H2vMap map)
        {
            var gotGlobals = map.TryGetTag(map.IndexHeader.Globals, out var globals);
            Debug.Assert(gotGlobals);

            var globalEntity = new GlobalSettings();
            var globalMaterials = new MaterialListComponent(globalEntity);

            for (var i = 0; i < globals.MaterialDefinitions.Length; i++)
            {
                var def = globals.MaterialDefinitions[i];
                var mat = new PhysicsMaterial(i, def.Friction, def.Friction, def.Restitution);

                globalMaterials.AddPhysicsMaterial(mat);
            }

            globalEntity.SetComponents(new Component[] { globalMaterials });
            destination.AddEntity(globalEntity);
        }

        private void PlaceLights(Scene destination)
        {
            for(var i = 0; i < 9; i++)
            {
                var position = VectorExtensions.Random(3, 12);
                var color = VectorExtensions.RandomColor(200);

                var item = new Light();
                var model = new RenderModelComponent(item, ModelFactory.HalfTriangularThing(color));

                var xform = new TransformComponent(item, position);

                var light = new PointLightEmitterComponent(item)
                {
                    Light = new PointLight()
                    {
                        Color = new Vector3(color.X, color.Y, color.Z),
                        Position = Vector3.Zero,
                        Radius = 20f
                    }
                };

                item.SetComponents(new Component[]{
                    model,
                    xform,
                    light
                });

                destination.Entities.Add(Guid.NewGuid(), item);
            }
        }
    }
}
