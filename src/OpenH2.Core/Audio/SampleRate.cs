namespace OpenH2.Core.Audio
{
    public class SampleRate
    {
        public static readonly SampleRate _44k1 = new SampleRate(44100);
        public static readonly SampleRate _22k05 = new SampleRate(44100);
        public static readonly SampleRate _32k = new SampleRate(32000);
        public static readonly SampleRate _48k = new SampleRate(48000);

        public int Rate { get; }
        public SampleRate(int rate)
        {
            Rate = rate;
        }
    }
}
