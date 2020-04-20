using OpenH2.Foundation.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physics.Abstractions
{
    public interface IContactGenerator
    {
        Contact[] DetectCollisions(IList<IBody> candidatePairs);
    }
}
