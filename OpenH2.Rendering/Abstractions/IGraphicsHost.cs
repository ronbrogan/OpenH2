using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Rendering.Abstractions
{
    public interface IGraphicsHost
    {
        void CreateWindow(Vector2 size, bool hidden = false);
        IGraphicsAdapter GetAdapter();
        void EnableConsoleDebug();
    }
}
