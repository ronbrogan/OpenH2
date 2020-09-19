using OpenH2.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.GameObjects
{
    public interface IGameObject
    {
        void TeleportTo(Vector3 position);
        void SetShield(float vitality);
        void Show();
        void Hide();
    }
}
