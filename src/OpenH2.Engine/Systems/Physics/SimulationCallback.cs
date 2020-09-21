using PhysX;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Systems.Physics
{
    public class SimulationCallback : SimulationEventCallback
    {
        public List<TriggerPair[]> TriggerEventSets { get; } = new List<TriggerPair[]>();

        public override void OnTrigger(TriggerPair[] pairs)
        {
            this.TriggerEventSets.Add(pairs);
            base.OnTrigger(pairs);
        }
    }
}
