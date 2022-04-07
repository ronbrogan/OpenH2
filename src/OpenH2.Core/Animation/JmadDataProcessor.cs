using System;

namespace OpenH2.Core.Animation
{
    public class JmadDataProcessor : IAnimationProcessor
    {
        private JmadDataProcessor() { }
        public static JmadDataProcessor GetProcessor()
        {
            var processor = new JmadDataProcessor();

            return processor;
        }

        public Animation GetAnimation(int frames, int bones, Span<byte> allData)
        {
            var header = JmadDataContainer.Create(allData);

            // Most animation data starts with a flat section, ignoring until I figure out why
            if(header.Type == JmadDataType.Flat)
            {
                allData = allData.Slice(header.TotalLength());
                header = JmadDataContainer.Create(allData);
            }

            var frameData = new AnimationNodeTransform[frames, bones];

            for (int f = 0; f < frames; f++)
            {
                for (int b = 0; b < bones; b++)
                {
                    frameData[f, b] = new AnimationNodeTransform
                    {
                        Orientation = header.ReadOrientation(allData, f, b),
                        Translation = header.ReadTranslation(allData, f, b)
                    };
                }
            }

            return new Animation() 
            { 
                Frames = frameData 
            };
        }
    }
}
