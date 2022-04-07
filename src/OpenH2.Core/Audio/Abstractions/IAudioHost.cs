namespace OpenH2.Core.Audio.Abstractions
{
    public interface IAudioHost
    {
        void MakeCurrent();
        IAudioAdapter GetAudioAdapter();
        void Shutdown();
    }
}
