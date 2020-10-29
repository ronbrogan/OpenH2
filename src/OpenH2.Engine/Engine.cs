using OpenH2.Core.Architecture;
using OpenH2.Core.Configuration;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using OpenH2.Foundation.Engine;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

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
                LoadMap(factory, mapPath);
            });

            var rtWorld = new RealtimeWorld(this, gameWindowGetter());
            rtWorld.UseGraphicsAdapter(graphicsHost.GetAdapter());

            world = rtWorld;

            LoadMap(factory, mapPath);

            gameLoop.RegisterCallbacks(world.Update, world.Render);
            gameLoop.Start(60, 60);
        }

        private void LoadMap(MapFactory factory, string mapPath)
        {
            SpectatorCamera camera = new SpectatorCamera();

            if(world.Scene != null)
            {
                camera = world.Scene.Entities.FirstOrDefault((v) => v.Value.GetType() == typeof(SpectatorCamera)).Value as SpectatorCamera;
            }

            var watch = new Stopwatch();
            watch.Start();

            using var fs = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8096);
            var map = factory.FromFile(fs);
            var scene = new Scene(map, new EntityCreator(map));
            scene.Load();

            watch.Stop();
            Console.WriteLine($"Loading map took {watch.ElapsedMilliseconds / 1000f} seconds");

            //scene.AddEntity(camera);

            var player = new Player(true);
            player.Transform.Position = map.Scenario.PlayerSpawnMarkers[0].Position;
            player.Transform.UpdateDerivedData();
            scene.AddEntity(player);

            world.LoadScene(scene);
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
