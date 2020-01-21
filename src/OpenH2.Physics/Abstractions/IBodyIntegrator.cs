using OpenH2.Foundation.Physics;

namespace OpenH2.Physics.Abstractions
{
    public interface IBodyIntegrator<TBody>
    {
        void Integrate(TBody body, float timestep);
    }
}
