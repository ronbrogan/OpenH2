using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.snd)] // snd!
    public class SoundTag : BaseTag
    {
        public override string Name { get; set; }
        public SoundTag(uint id) : base(id)
        {
        }

        // Not quite sure what's going on here yet, so little data
    }
}
