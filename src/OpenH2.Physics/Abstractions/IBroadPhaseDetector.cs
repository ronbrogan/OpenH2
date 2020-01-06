using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physics.Abstractions
{
    public interface IBroadPhaseDetector
    {
        object DetectCandidateCollisions();
    }
}
