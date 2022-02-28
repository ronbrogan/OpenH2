using OpenH2.Core.Architecture;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Engine.Scripting;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Silk.NET.Input;
using System.Diagnostics;

namespace OpenH2.Engine.Systems
{
    public class ScriptSystem : WorldSystem
    {
        private readonly AudioSystem audioSystem;
        private readonly CameraSystem cameraSystem;
        private readonly ActorSystem actorSystem;
        private readonly AnimationSystem animationSystem;
        private bool run = false;
        private InterpretingScriptExecutor executor;
        private ScriptEngine engine;
        private Stopwatch stopwatch;
        private InputStore inputStore;

        public ScriptSystem(World world, 
            AudioSystem audioSystem,
            CameraSystem cameraSystem,
            ActorSystem actorSystem,
            AnimationSystem animationSystem) : base(world)
        {
            this.audioSystem = audioSystem;
            this.cameraSystem = cameraSystem;
            this.actorSystem = actorSystem;
            this.animationSystem = animationSystem;
        }

        public override void Initialize(Scene scene)
        {
            this.inputStore = this.world.GetGlobalResource<InputStore>();
            var scenario = scene.Map.Scenario;


            this.executor = new InterpretingScriptExecutor(scenario);
            this.engine = new ScriptEngine(scene, 
                this.executor, 
                this.audioSystem,
                this.cameraSystem,
                this.actorSystem,
                this.animationSystem);

            this.executor.Initialize(this.engine);
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            scene.RegisterMetricSource(this.executor);

            base.Initialize(scene);
        }

        public override void Update(double timestep)
        {
            if(this.inputStore.WasPressed(Key.F11))
            {
                this.run = !this.run;
                Logger.LogInfo($"Toggling script execution to [{(this.run ? "ON" : "OFF")}]");
            }

            if(run && this.stopwatch.ElapsedMilliseconds >= 33)
            {
                this.stopwatch.Restart();
                this.executor.Execute();
            }
        }
    }
}
