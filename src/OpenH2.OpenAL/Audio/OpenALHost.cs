using OpenH2.Audio.Abstractions;
using OpenTK.Audio.OpenAL;
using System.Numerics;

namespace OpenH2.OpenAL.Audio
{
    public static class AlSoft
    {
        public static int HRTF = 0x1992;
        public static int Disable = 0;
        public static int Enable = 1;
        public static int Auto = 2;
    }

    public class OpenALHost : IAudioHost
    {
        private readonly ALDevice device;
        private readonly ALContext context;
        internal readonly Vector3 forward;
        internal readonly Vector3 up;

        private OpenALHost(ALDevice device, ALContext context, Vector3 forward, Vector3 up)
        {
            this.device = device;
            this.context = context;
            this.forward = forward;
            this.up = up;
        }

        public static OpenALHost Open(Vector3 forward, Vector3 up)
        {
            var devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);

            // Get the default device, then go though all devices and select the AL soft device if it exists.
            string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
            foreach (var d in devices)
            {
                if (d.Contains("OpenAL Soft"))
                {
                    deviceName = d;
                }
            }

            var device = ALC.OpenDevice(deviceName);
            var context = ALC.CreateContext(device, new int[] { 
                AlSoft.HRTF, AlSoft.Enable
            });
            ALC.MakeContextCurrent(context);

            return new OpenALHost(device, context, forward, up);
        }

        public void MakeCurrent()
        {
            ALC.MakeContextCurrent(this.context);
        }

        public IAudioAdapter GetAudioAdapter()
        {
            return new OpenALAudioAdapter(this);
        }

        public void Shutdown()
        {
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.CloseDevice(this.device);
            ALC.DestroyContext(this.context);
        }
    }
}
