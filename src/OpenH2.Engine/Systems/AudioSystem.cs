using OpenH2.Core.Architecture;
using OpenH2.Core.Audio;
using OpenH2.Core.Audio.Abstractions;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using System;
using System.Collections.Generic;
using SampleRate = OpenH2.Core.Audio.SampleRate;

namespace OpenH2.Engine.Systems
{
    public class AudioSystem : WorldSystem
    {
        private Scene scene;
        private SoundMappingTag soundMapping;
        private ISoundListener listener;
        private ISoundEmitter globalEmitter;
        private readonly IAudioAdapter audioAdapter;
        private Random rng = new Random(42);

        public AudioSystem(World world, IAudioAdapter audioAdapter) : base(world)
        {
            this.listener = audioAdapter.CreateListener();
            this.globalEmitter = audioAdapter.CreateEmitter();
            this.audioAdapter = audioAdapter;
        }

        public override void Initialize(Scene scene)
        {
            this.scene = scene;

            this.soundMapping = scene.Map.GetTag(scene.Map.Globals.SoundInfos[0].SoundMap);
        }

        public override void Update(double timestep)
        {
            var listenerComponents = this.world.Components<SoundListenerComponent>();

            if (listenerComponents.Count == 0)
                return;

            var listenerComponent = listenerComponents[0];

            this.listener.SetPosition(listenerComponent.Transform.Position + listenerComponent.PositionOffset);
            this.listener.SetOrientation(listenerComponent.Transform.Orientation);
            this.globalEmitter.SetPosition(listenerComponent.Transform.Position);
            
            var comps = this.world.Components<SoundEmitterComponent>();

            foreach(var comp in comps)
            {
                if (comp.Emitter == null)
                {
                    // TODO: destroy emitter
                    comp.Emitter = this.audioAdapter.CreateEmitter();
                }

                comp.Emitter.SetPosition(comp.Transform.Position);
            }
        }

        public void Start(SoundTag sound, IGameObject target)
        {
            var clip = GetClip(sound);

            ISoundEmitter emitter;

            if(target is GameObjectEntity e)
            {
                if(e.SoundEmitter.Emitter == null)
                {
                    // TODO: destroy emitter
                    e.SoundEmitter.Emitter = this.audioAdapter.CreateEmitter();
                }

                emitter = e.SoundEmitter.Emitter;
            }
            else
            {
                emitter = globalEmitter;
            }

            emitter.PlayImmediate(clip.Encoding, clip.SampleRate, clip.Data);
            SoundTagEmitters[sound.Id] = emitter;
        }

        private Dictionary<uint, ISoundEmitter> SoundTagEmitters = new Dictionary<uint, ISoundEmitter>();
        public float SecondsRemaining(SoundTag snd)
        {
            if(SoundTagEmitters.TryGetValue(snd.Id, out var emitter))
            {
                return (float)emitter.RemainingTime().TotalSeconds;
            }

            return 0;
        }
        
        private ClipData GetClip(SoundTag snd)
        {
            var soundEntry = soundMapping.SoundEntries[snd.SoundEntryIndex];

            var clipIndex = rng.Next(0, soundEntry.NamedSoundClipCount);
            var clipInfo = soundMapping.NamedSoundClips[soundEntry.NamedSoundClipIndex + clipIndex];

            var result = new ClipData();

            result.Encoding = snd.Encoding switch
            {
                EncodingType.ImaAdpcmMono => AudioEncoding.MonoImaAdpcm,
                EncodingType.ImaAdpcmStereo => AudioEncoding.StereoImaAdpcm,
                _ => AudioEncoding.Mono16,
            };

            result.SampleRate = snd.SampleRate switch
            {
                Core.Tags.SampleRate.hz22k05 => SampleRate._22k05,
                Core.Tags.SampleRate.hz44k1 => SampleRate._44k1,
                _ => SampleRate._44k1
            };

            var clipSize = 0;

            for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
            {
                var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];
                clipSize += (int)(chunk.Length & 0x3FFFFFFF);
            }

            Span<byte> clipData = new byte[clipSize];
            var clipDataCurrent = 0;

            for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
            {
                var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];

                var len = (int)(chunk.Length & 0x3FFFFFFF);
                var chunkData = this.scene.Map.ReadData(chunk.Offset.Location, chunk.Offset, len);

                chunkData.Span.CopyTo(clipData.Slice(clipDataCurrent));
                clipDataCurrent += len;
            }

            result.Data = clipData;

            return result;
        }

        private ref struct ClipData
        {
            public AudioEncoding Encoding;
            public SampleRate SampleRate;
            public Span<byte> Data;
        }
    }
}
