using System;

namespace OpenH2.Foundation.Engine
{
    public interface IGameLoopSource
    {
        void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback);
        void Start(int updatesPerSecond, int framesPerSecond);
    }
}
