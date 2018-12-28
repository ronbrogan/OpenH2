using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Rendering.Abstractions
{
    public interface IGameLoopSource
    {
        void RegisterCallbacks(Action updateCallback, Action renderCallback);
        void Start(int updatesPerSecond, int framesPerSecond);
    }
}
