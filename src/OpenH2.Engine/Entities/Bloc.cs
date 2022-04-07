using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;

namespace OpenH2.Engine.Entities
{
    public class Bloc : GameObjectEntity, IBloc
    {
        public Bloc()
        {
            this.Components = new Component[0];
        }
    }
}
