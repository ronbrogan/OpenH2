namespace OpenH2.Core.Audio
{
    public class SampleRate
    {
        public static readonly SampleRate _44k1 = new SampleRate(44100);
        public static readonly SampleRate _22k05 = new SampleRate(44100);

        public int Rate { get; }
        public SampleRate(int rate)
        {
            Rate = rate;
        }
    }
}
