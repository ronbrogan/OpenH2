using PhysX;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Physx.Proxies
{
    public class PhysxEventProxy : SimulationEventCallback
    {
        public override void OnAdvance(RigidBody[] rigidBodies, Matrix4x4[] poses)
        {
            base.OnAdvance(rigidBodies, poses);
        }

        public override void OnConstraintBreak(ConstraintInfo[] constraints)
        {
            base.OnConstraintBreak(constraints);
        }

        public override void OnContact(ContactPairHeader pairHeader, ContactPair[] pairs)
        {
            foreach(var pair in pairs)
            {
                var contacts = pair.ExtractContacts();

                foreach(var c in contacts)
                {
                    

                    pairHeader.Actor0.GetShape(0).GetMaterialFromInternalFaceIndex(c.InternalFaceIndex0);
                }
            }

            

            base.OnContact(pairHeader, pairs);
        }

        public override void OnSleep(Actor[] actors)
        {
            base.OnSleep(actors);
        }

        public override void OnTrigger(TriggerPair[] pairs)
        {
            base.OnTrigger(pairs);
        }

        public override void OnWake(Actor[] actors)
        {
            base.OnWake(actors);
        }
    }
}
