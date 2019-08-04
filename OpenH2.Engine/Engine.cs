using OpenH2.Core.Architecture;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.EntityFactories;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Engine;
using OpenH2.Rendering;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Shaders;
using System;
using System.IO;
using System.Linq;

namespace OpenH2.Engine
{
    public class Engine
    {
        IGraphicsHost graphicsHost;
        IGraphicsAdapter graphicsAdapter;
        IGameLoopSource gameLoop;
        public IRenderAccumulator<BitmapTag> RenderAccumulator;

        private World world;

        public Engine()
        {
            var host = new OpenGLHost();

            graphicsHost = host;
            gameLoop = host;
            graphicsAdapter = host.GetAdapter();

            RenderAccumulator = new RenderAccumulator(graphicsAdapter);
        }

        public void Start(EngineStartParameters parameters)
        {
            graphicsHost.CreateWindow();

            world = new RealtimeWorld(this);

            var scene = new Scene();
            
            scene.AddEntity(new SpectatorCamera());
            LoadMap(scene);
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

            foreach(var model in renderList.Models)
            {
                var modelMatrix = model.CreateTransformationMatrix();

                foreach(var mesh in model.Meshes)
                {
                    mesh.Note = model.Note;
                    RenderAccumulator.AddRigidBody(mesh, renderList.Materials[mesh.MaterialIdentifier], modelMatrix);
                }
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

            RenderAccumulator.DrawAndFlush();
        }

        public void LoadMap(Scene destination)
        {
            var mapPath = @"D:\H2vMaps\ascension.map";

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var map = factory.FromFile(File.OpenRead(mapPath));

            var scenario = map.GetLocalTagsOfType<ScenarioTag>().First();

            var bsps = map.GetLocalTagsOfType<BspTag>();

            foreach(var bsp in bsps)
            {
                destination.AddEntity(TerrainFactory.FromBspData(map, bsp));
            }

            foreach(var scen in scenario.SceneryInstances)
            {
                if (scen.SceneryDefinitionIndex == ushort.MaxValue)
                    continue;

                destination.AddEntity(SceneryFactory.FromTag(map, scenario, scen));
            }

            foreach (var bloc in scenario.BlocInstances)
            {
                destination.AddEntity(BlocFactory.FromTag(map, scenario, bloc));
            }

            PositioningEntities.AddLocators(map, destination);
        }
    }
}
