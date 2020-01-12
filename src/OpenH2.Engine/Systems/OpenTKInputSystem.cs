using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using OpenTK.Input;

namespace OpenH2.Engine.Systems
{
    public class OpenTKInputSystem : WorldSystem
    {
        public OpenTKInputSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            var inputs = this.world.GetGlobalResource<InputStore>();

            inputs.SetMouse(Mouse.GetCursorState());
            inputs.SetKeys(Keyboard.GetState());
        }
    }
}
