using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Engine.Components;
using System.Collections.Generic;

namespace OpenH2.Engine.Entities
{
    public class Machine : GameObjectEntity, IMachine
    {
        public void SetComponents(TransformComponent xform, 
            params Component[] rest)
        {
            this.Transform = xform;

            var allComponents = new List<Component>();
            allComponents.Add(xform);
            allComponents.AddRange(rest);
            this.SetComponents(allComponents);
        }
    }
}
