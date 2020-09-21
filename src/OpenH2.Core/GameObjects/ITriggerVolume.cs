namespace OpenH2.Core.GameObjects
{
    public interface ITriggerVolume : IGameObject
    {
        void KillOnEnter(bool enable);
        IGameObject[] GetObjects(TypeFlags f = TypeFlags.All);
        bool Contains(IGameObject entity);
    }
}
