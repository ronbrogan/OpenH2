using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Audio.Abstractions
{
    public interface IAudioHost
    {
        void MakeCurrent();
        IAudioAdapter GetAudioAdapter();
        void Shutdown();
    }
}
