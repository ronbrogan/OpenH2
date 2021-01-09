using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Metrics
{
    public interface IMetricSource
    {
        void Enable(IMetricSink destination);
    }
}
