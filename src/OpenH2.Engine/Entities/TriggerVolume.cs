using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Engine.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace OpenH2.Engine.Entities
{
    public class TriggerVolume : GameObjectEntity, ITriggerVolume
    {
        public void SetComponents(
            TransformComponent transform,
            TriggerGeometryComponent volume,
            params Component[] rest)
        {
            var allComps = new List<Component>();
            allComps.Add(transform);
            allComps.Add(volume);
            allComps.AddRange(rest);

            base.SetComponents(allComps);
        }

        public bool Contains(IGameObject entity)
        {
            return false;
        }

        public IGameObject[] GetObjects(TypeFlags f = TypeFlags.All)
        {
            return Array.Empty<IGameObject>();
        }

        public void KillOnEnter(bool enable)
        {
        }
    }
}
