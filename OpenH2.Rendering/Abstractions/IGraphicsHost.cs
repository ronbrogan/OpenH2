using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Rendering.Abstractions
{
    public interface IGraphicsHost
    {
        void CreateWindow();
        IGraphicsAdapter GetAdapter();
    }
}
