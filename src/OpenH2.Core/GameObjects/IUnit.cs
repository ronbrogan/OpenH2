namespace OpenH2.Core.GameObjects
{
    public interface IUnit : IGameObject
    {
        public IGameObject Driver { get; }
    }
}
