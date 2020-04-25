using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;

namespace OpenH2.Engine.Systems
{
    public class OpenTKInputSystem : WorldSystem
    {
        private readonly GameWindow window;

        public OpenTKInputSystem(World world, GameWindow window) : base(world)
        {
            this.window = window;
        }

        public override void Update(double timestep)
        {
            var inputs = this.world.GetGlobalResource<InputStore>();

            inputs.SetMouse(window.MouseState);
            inputs.SetKeys(window.KeyboardState);
        }
    }
}
