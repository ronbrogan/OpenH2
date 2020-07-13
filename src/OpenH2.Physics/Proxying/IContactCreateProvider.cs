using OpenH2.Physics.Core;
using System;

namespace OpenH2.Physics.Proxying
{
    public interface IContactCreateProvider
    {
        void RegisterContactCallback(object body, ContactCallbackData relevantDataFilter, Action<ContactInfo> callback);
        
        void RegisterCharacterController(object body, Action<object, ContactInfo> callback);
    }
}
