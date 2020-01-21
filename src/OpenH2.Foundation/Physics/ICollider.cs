using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface ICollider
    {
        ISweepableBounds Bounds { get; set; }

        bool Intersects(ICollider other);
        IList<Contact> GenerateContacts(ICollider other);
    }
}
