using System.Numerics;
using OpenH2.Audio.Abstractions;
using Silk.NET.OpenAL;

namespace OpenH2.OpenAL.Audio
{
    public static class AlSoft
    {
        public static int HRTF = 0x1992;
        public static int Disable = 0;
        public static int Enable = 1;
        public static int Auto = 2;
    }

    public unsafe class OpenALHost : IAudioHost
    {
        public readonly AL al;
        private readonly ALContext alc;
        private readonly Device* device;
        private readonly Context* context;
        internal readonly Vector3 forward;
        internal readonly Vector3 up;

        private OpenALHost(AL al, ALContext alc, Device* device, Context* context, Vector3 forward, Vector3 up)
        {
            this.al = al;
            this.alc = alc;
            this.device = device;
            this.context = context;
            this.forward = forward;
            this.up = up;
        }

        public unsafe static OpenALHost Open(Vector3 forward, Vector3 up)
        {
            var alc = ALContext.GetApi(soft: false);

            var al = AL.GetApi(soft: false);

            //var devices = alc.GetStringList(GetEnumerationStringList.DeviceSpecifier);

            // Get the default device, then go though all devices and select the AL soft device if it exists.
            string deviceName = alc.GetContextProperty(null, GetContextString.DeviceSpecifier);
            //foreach (var d in devices)
            //{
            //    if (d.Contains("OpenAL Soft"))
            //    {
            //        deviceName = d;
            //    }
            //}

            var device = alc.OpenDevice(deviceName);

            int* attrs = stackalloc int[] {
                AlSoft.HRTF,
                AlSoft.Enable
            };

            var context = alc.CreateContext(device, attrs);
            alc.MakeContextCurrent(context);

            return new OpenALHost(al, alc, device, context, forward, up);
        }

        public void MakeCurrent()
        {
            alc.MakeContextCurrent(this.context);
        }

        public IAudioAdapter GetAudioAdapter()
        {
            return new OpenALAudioAdapter(this);
        }

        public void Shutdown()
        {
            alc.MakeContextCurrent(null);
            alc.CloseDevice(this.device);
            alc.DestroyContext(this.context);
        }
    }
}
