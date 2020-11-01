namespace OpenH2.Core.GameObjects
{
    public interface IAiActor : IUnit
    {

    }

    public interface IAiActorDefinition
    {
        public IAiActor Actor { get; }
    }
}
