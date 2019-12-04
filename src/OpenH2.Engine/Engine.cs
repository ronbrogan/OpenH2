using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.EntityFactories;
using OpenH2.Engine.Stores;
using OpenH2.Foundation;
using OpenH2.Foundation.Engine;
using OpenH2.Rendering;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Pipelines;
using OpenH2.Rendering.Shaders;
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
        IGraphicsAdapter graphicsAdapter;
        IGameLoopSource gameLoop;
        public IRenderingPipeline<BitmapTag> RenderingPipeline;

        private World world;

        public Engine()
        {
            var host = new OpenGLHost();

            graphicsHost = host;
            gameLoop = host;
            graphicsAdapter = host.GetAdapter();

            RenderingPipeline = new ForwardRenderingPipeline(graphicsAdapter);
        }

        public void Start(EngineStartParameters parameters)
        {
            graphicsHost.CreateWindow(new Vector2(1600, 900));

            world = new RealtimeWorld(this);

            var scene = new Scene();
            
            scene.AddEntity(new SpectatorCamera());

            var watch = new Stopwatch();
            watch.Start();
            LoadMap(scene);
            watch.Stop();
            Console.WriteLine($"Loading map took {watch.ElapsedMilliseconds / 1000f} seconds");
            world.LoadScene(scene);

            gameLoop.RegisterCallbacks(Update, Render);
            gameLoop.Start(60, 60);
        }

        private void Update(double timestep)
        {
            world.Update(timestep);
        }

        private void Render(double timestep)
        {
            var renderList = world.GetGlobalResource<RenderListStore>();

            foreach(var (model, mx) in renderList.Models)
            {
                RenderingPipeline.AddStaticModel(model, mx);
            }

            foreach (var light in renderList.Lights)
            {
                RenderingPipeline.AddPointLight(light);
            }

            var cameras = world.Components<CameraComponent>();
            var cam = cameras.First();

            var pos = cam.PositionOffset;

            if(cam.TryGetSibling<TransformComponent>(out var xform))
            {
                pos += xform.Position;
            }

            var matrices = new GlobalUniform
            {
                ViewMatrix = cam.ViewMatrix,
                ProjectionMatrix = cam.ProjectionMatrix,
                ViewPosition = pos
            };

            graphicsAdapter.BeginFrame(matrices);

            RenderingPipeline.DrawAndFlush();

            graphicsAdapter.EndFrame();
        }

        public void LoadMap(Scene destination)
        {
            var mapPath = @"D:\H2vMaps\zanzibar.map";

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));

            var fs = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024);
            var map = factory.FromFile(fs);

            map.TryGetTag(map.IndexHeader.Scenario, out var scenario);

            var terrains = scenario.Terrains;

            foreach (var terrain in terrains)
            {
                map.TryGetTag(terrain.Bsp, out var bsp);

                destination.AddEntity(TerrainFactory.FromBspData(map, bsp));

                foreach(var instance in bsp.InstancedGeometryInstances)
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

            PositioningEntities.AddLocators(map, destination);

            PlaceLights(destination);
        }

        private void PlaceLights(Scene destination)
        {
            for(var i = 0; i < 9; i++)
            {
                var position = VectorExtensions.Random(3, 12);
                var color = VectorExtensions.RandomColor(200);

                var item = new Light();
                var model = new RenderModelComponent(item)
                {
                    RenderModel = ModelFactory.HalfTriangularThing(color)
                };

                var xform = new TransformComponent(item)
                {
                    Position = position
                };

                var light = new PointLightEmitterComponent(item)
                {
                    Light = new PointLight()
                    {
                        Color = color,
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
