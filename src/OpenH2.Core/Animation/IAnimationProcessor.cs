using System;

namespace OpenH2.Core.Animation
{
    public interface IAnimationProcessor
    {
        Animation GetAnimation(int frames, int maxBones, Span<byte> data);
    }
}