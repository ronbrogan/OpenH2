using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;

namespace OpenH2.Engine.Systems
{
    public class SilkInputSystem : WorldSystem
    {
        private readonly IInputContext inputCtx;

        public SilkInputSystem(World world, IInputContext inputCtx) : base(world)
        {
            this.inputCtx = inputCtx;
        }

        public override void Update(double timestep)
        {
            var inputs = this.world.GetGlobalResource<InputStore>();

            var state = this.inputCtx.CaptureState();
            inputs.SetMouse(state.Mice[0]);
            inputs.SetKeys(state.Keyboards[0]);
        }
    }
}
