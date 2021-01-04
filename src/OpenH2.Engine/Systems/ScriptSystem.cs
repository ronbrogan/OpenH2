using OpenH2.Core.Architecture;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Scripting;
using OpenH2.Engine.Stores;
using OpenH2.Foundation.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OpenH2.Engine.Systems
{
    public class ScriptSystem : WorldSystem
    {
        private readonly AudioSystem audioSystem;
        private readonly CameraSystem cameraSystem;
        private bool run = false;
        private InterpretingScriptExecutor executor;
        private ScriptEngine engine;
        private Stopwatch stopwatch;
        private InputStore inputStore;

        public ScriptSystem(World world, 
            AudioSystem audioSystem,
            CameraSystem cameraSystem) : base(world)
        {
            this.audioSystem = audioSystem;
            this.cameraSystem = cameraSystem;
        }

        public override void Initialize(Scene scene)
        {
            this.inputStore = this.world.GetGlobalResource<InputStore>();
            var scenario = scene.Map.Scenario;


            this.executor = new InterpretingScriptExecutor(scenario);
            this.engine = new ScriptEngine(scene, 
                this.executor, 
                this.audioSystem,
                this.cameraSystem);

            this.executor.Initialize(this.engine);
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            base.Initialize(scene);
        }

        public override void Update(double timestep)
        {
            if(this.inputStore.WasPressed(Keys.F11))
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
