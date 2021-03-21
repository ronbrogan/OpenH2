using OpenH2.Audio.Abstractions;
using OpenH2.Core.Architecture;
using OpenH2.Core.Configuration;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Metrics;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.EntityFactories;
using OpenH2.Foundation;
using OpenH2.Foundation.Engine;
using OpenH2.OpenAL.Audio;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace OpenH2.Engine
{
    public class Engine
    {
        IGraphicsHost graphicsHost;
        IAudioHost audioHost;
        IGameLoopSource gameLoop;
        Func<GameWindow> gameWindowGetter;

        private World world;

        public Engine()
        {
            var host = new OpenGLHost();
            gameWindowGetter = host.GetWindow;

            graphicsHost = host;
            gameLoop = host;

            audioHost = OpenALHost.Open(EngineGlobals.Forward, EngineGlobals.Up);
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

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));

            matFactory.AddListener(() =>
            {
                LoadMap(factory, mapPath, matFactory);
            });

            world = new RealtimeWorld(gameWindowGetter(), 
                audioHost.GetAudioAdapter(), 
                graphicsHost);

            LoadMap(factory, mapPath, matFactory);

            gameLoop.RegisterCallbacks(world.Update, world.Render);
            gameLoop.Start(60, 60);
        }

        private void LoadMap(MapFactory factory, string mapPath, IMaterialFactory materialFactory)
        {
            var watch = new Stopwatch();
            watch.Start();

            var imap = factory.Load(mapPath);

            if(imap is not H2vMap map)
            {
                throw new Exception("Engine only supports Halo 2 Vista maps currently");
            }

            map.UseMaterialFactory(materialFactory);

            var scene = new Scene(map, new EntityCreator(map));
            scene.Load();

            watch.Stop();
            Console.WriteLine($"Loading map took {watch.ElapsedMilliseconds / 1000f} seconds");

            var player = new Player(true);
            player.FriendlyName = "player_0";
            player.Transform.Position = map.Scenario.PlayerSpawnMarkers[0].Position + new Vector3(0, 0, 0.3f);
            player.Transform.Orientation = Quaternion.CreateFromAxisAngle(EngineGlobals.Up, map.Scenario.PlayerSpawnMarkers[0].Heading);
            player.Transform.UpdateDerivedData();
            scene.AddEntity(player);


            foreach (var squad in map.Scenario.AiSquadDefinitions)
            {
                foreach (var start in squad.StartingLocations)
                {
                    var entity = ActorFactory.SpawnPointFromStartingLocation(map, start);

                    if(entity != null)
                        scene.AddEntity(entity);
                }
            }

            world.LoadScene(scene);

            var timestamp = DateTime.Now.ToString("yy-MM-ddTHH-mm");
            var sinkPath = Path.Combine(Environment.CurrentDirectory, "diagnostics", $"{timestamp}-metrics.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(sinkPath));
            var sink = new FlatFileMetricSink(sinkPath);
            scene.UseMetricSink(sink);
            sink.Start();
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
