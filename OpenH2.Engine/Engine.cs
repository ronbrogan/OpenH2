using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using OpenH2.Core.Architecture;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.EntityFactories;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems;
using OpenH2.Foundation.Engine;
using OpenH2.Rendering;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Shaders;
using OpenH2.Translation;
using OpenH2.Translation.TagData;
using OpenTK.Graphics.OpenGL;

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

            // process all non-render systems in correct order



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

            graphicsAdapter.UseMatricies(matrices);

            RenderAccumulator.DrawAndFlush();
        }

        public void LoadMap(Scene destination)
        {
            var mapPath = @"D:\Halo 2 Vista Original Maps\ascension.map";

            var factory = new MapFactory();
            var map = factory.FromFile(File.OpenRead(mapPath));

            var translator = new TagTranslator(map);
            var bsps = translator.GetAll<BspTagData>();

            destination.AddEntity(TerrainFactory.FromBspData(map, bsps.First()));
        }
    }
}
