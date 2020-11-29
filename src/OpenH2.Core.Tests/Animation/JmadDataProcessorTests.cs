using OpenH2.Core.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var animation = processor.GetAnimation(19, maxBones: 42, animationData);

            Assert.Equal(23, animation.Frames.GetLength(0));
        }

        [Fact, Trait("skip", "true")]
        public void ProcessAnimation2()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\fp_battle_rifle.first_person.fire_1.var1.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(8, maxBones: 42, animationData);

            Assert.Equal(6, animation.Frames.GetLength(0));
        }

        [Fact, Trait("skip", "true")]
        public void ProcessAnimation3()
        {
            Span<byte> animationData = File.ReadAllBytes(@"D:\h2scratch\03_intro.camera.0.anim");

            var processor = JmadDataProcessor.GetProcessor();

            var animation = processor.GetAnimation(703, maxBones: 1, animationData);

            Assert.Equal(1, animation.Frames.GetLength(0));
        }
    }
}
