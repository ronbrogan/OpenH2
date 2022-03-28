using OpenH2.Audio.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.OpenAL.Audio
{
    public class OpenALAudioAdapter : IAudioAdapter
    {
        private readonly OpenALHost host;

        internal OpenALAudioAdapter(OpenALHost host)
        {
            this.host = host;
        }

        public ISoundEmitter CreateEmitter()
        {
            return new ALSoundEmitter(this.host.al);
        }

        public ISoundListener CreateListener()
        {
            return new ALSoundListener(host.al, host.forward, host.up);
        }

        public void DestroyEmitter(ISoundEmitter emitter)
        {
            if (emitter is ALSoundEmitter alEmitter)
            {
                alEmitter.Dispose();
            }
            else
            {
                throw new NotSupportedException("Emitter from another adapter was provided");
            }
        }

        public void DestroyListener(ISoundListener listener)
        {
        }
    }
}
