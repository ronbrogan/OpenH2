using OpenH2.Core.Animation;
using System;
using System.IO;
using Xunit;

namespace OpenH2.Core.Tests.Animation
{
    public class JmadDataProcessorTests
    {
        [Fact, Trait("skip", "true")]
        public void ProcessAnimation1()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\fp_battle_rifle.first_person.ready.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(19, bones: 42, animationData);

            Assert.Equal(19, animation.Frames.GetLength(0));
        }

        [Fact, Trait("skip", "true")]
        public void ProcessAnimation2()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\fp_battle_rifle.first_person.fire_1.var1.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(8, bones: 42, animationData);

            Assert.Equal(8, animation.Frames.GetLength(0));
        }

        [Fact, Trait("skip", "true")]
        public void ProcessAnimation3()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\03_intro.camera.0.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(703, bones: 1, animationData);

            Assert.Equal(703, animation.Frames.GetLength(0));
        }

        [Fact, Trait("skip", "true")]
        public void ProcessAnimation4()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\animations\marine_tutorial.jmad.l01_0010_jon.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(110, bones: 53, animationData);

            Assert.Equal(110, animation.Frames.GetLength(0));
        }
    }
}
