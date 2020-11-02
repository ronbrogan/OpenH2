using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Audio.Abstractions
{
    public interface IAudioAdapter
    {
        ISoundEmitter CreateEmitter();
        void DestroyEmitter(ISoundEmitter emitter);

        ISoundListener CreateListener();
        void DestroyListener(ISoundListener listener);
    }
}
