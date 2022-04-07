namespace OpenH2.Core.Audio.Abstractions
{
    public interface IAudioAdapter
    {
        ISoundEmitter CreateEmitter();
        void DestroyEmitter(ISoundEmitter emitter);

        ISoundListener CreateListener();
        void DestroyListener(ISoundListener listener);
    }
}
