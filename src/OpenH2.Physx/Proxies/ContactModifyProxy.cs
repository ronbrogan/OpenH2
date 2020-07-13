using OpenH2.Physics.Core;
using OpenH2.Physics.Proxying;
using PhysX;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physx.Proxies
{
    public class ContactModifyProxy : ContactModifyCallback, IContactCreateProvider
    {
        private Dictionary<object, (ContactCallbackData, Action<ContactInfo>)> characterControllers
            = new Dictionary<object, (ContactCallbackData, Action<ContactInfo>)>();

        private Dictionary<object, (ContactCallbackData, Action<ContactInfo>)> registeredCallbacks 
            = new Dictionary<object, (ContactCallbackData, Action<ContactInfo>)>();

        private Vector3 Up = new Vector3(0, 0, 1);

        public void RegisterContactCallback(object body, ContactCallbackData relevantDataFilter, Action<ContactInfo> callback)
        {
            registeredCallbacks[body] = (relevantDataFilter, callback);
        }

        // TODO: needed?
        public void RegisterCharacterController(object body, Action<object, ContactInfo> callback)
        {

        }

        public override void OnContactModify(ContactModifyPair[] pairs)
        {
            ContactCallbackData filter;
            bool needsA;
            bool needsB;
            ContactInfo relevantData;

            foreach (var pair in pairs)
            {
                // Handle character controller behavior
                var isCharacterController = IsCharacterController(pair, out var ctrl, out var other);

                // Handle callbacks
                filter = ContactCallbackData.None;

                needsA = registeredCallbacks.TryGetValue(pair.ActorA, out var registeredA);
                needsB = registeredCallbacks.TryGetValue(pair.ActorB, out var registeredB);

                if (needsA) filter |= registeredA.Item1;
                if (needsB) filter |= registeredB.Item1;

                // Early exit if we don't need data
                if (filter == ContactCallbackData.None) return;

                for (var i = 0; i < pair.Contacts.Size; i++)
                {
                    relevantData = new ContactInfo();

                    if(isCharacterController)
                    {
                        var normal = pair.Contacts.GetNormal(i);
                        var upness = Vector3.Dot(normal, Up);

                        if (upness <= 0.5f)
                        {
                            pair.Contacts.SetDynamicFriction(i, 0f);
                            pair.Contacts.SetStaticFriction(i, 0f);
                        }
                        else
                        {
                            // handle foot contact
                            // TODO : is this needed?
                            relevantData.IsGroundContact = true;
                        }
                    }

                    if (Needs(ContactCallbackData.TargetVelocity))
                        relevantData.TargetVelocity = pair.Contacts.GetTargetVelocity(i);

                    if (Needs(ContactCallbackData.DynamicFriction))
                        relevantData.DynamicFriction = pair.Contacts.GetDynamicFriction(i);

                    if (Needs(ContactCallbackData.Normal))
                        relevantData.Normal = pair.Contacts.GetNormal(i);

                    if (Needs(ContactCallbackData.Point))
                        relevantData.Point = pair.Contacts.GetPoint(i);

                    if (Needs(ContactCallbackData.Faces))
                        relevantData.Faces = (pair.Contacts.GetInternalFaceIndex0(i), pair.Contacts.GetInternalFaceIndex1(i));



                    // Execute callbacks, receiver can get more data than they asked for here due to the pair-wise nature of contacts
                    if (needsA) 
                    {
                        if (Needs(ContactCallbackData.Material))
                            relevantData.Material = pair.ShapeB.GetMaterialFromInternalFaceIndex(pair.Contacts.GetInternalFaceIndex1(i));

                        registeredA.Item2(relevantData); 
                    }

                    if (needsB)
                    {
                        if (Needs(ContactCallbackData.Material))
                            relevantData.Material = pair.ShapeA.GetMaterialFromInternalFaceIndex(pair.Contacts.GetInternalFaceIndex0(i));

                        registeredB.Item2(relevantData);
                    }
                }                
            }

            bool Needs(ContactCallbackData d) => (filter & d) == d;
            
            // TODO: depends on engine-scoped data (flag of 1 meaning character controller)
            bool IsCharacterController(ContactModifyPair p, out RigidBody controller, out RigidActor other)
            {
                if((p.ShapeA.SimulationFilterData.Word0 & 1) == 1)
                {
                    controller = (RigidBody)p.ActorA;
                    other = p.ActorB;
                    return true;
                }
                
                if ((p.ShapeB.SimulationFilterData.Word0 & 1) == 1)
                {
                    controller = (RigidBody)p.ActorA;
                    other = p.ActorB;
                    return true;
                }

                controller = default;
                other = default;
                return false;
            }
        }
    }
}
