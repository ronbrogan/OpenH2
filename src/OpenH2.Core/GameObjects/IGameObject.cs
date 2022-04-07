using System.Numerics;

namespace OpenH2.Core.GameObjects
{
    public interface IGameObject
    {
        Vector3 Position { get; }
        Vector3 EyeOffset { get; }
        Quaternion Orientation { get; }

        float Shield { get; set; }
        float Health { get; set; }
        IGameObject Parent { get; }
        IAiActorDefinition? Ai { get; }
        bool IsAlive { get; }

        void TeleportTo(Vector3 position);
        void Scale(float scale);
        
        void Show();
        void Hide();
        void Attach(IGameObject entity);
        void Detach(IGameObject child);
    }
}
