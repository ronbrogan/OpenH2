using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Maps.MCC
{
    public class H2mccMap : H2BaseMap<H2mccMapHeader>
    {
        public ScenarioTag Scenario { get; internal set; }
        public SoundMappingTag LocalSounds { get; internal set; }
        public GlobalsTag Globals { get; internal set; }

        public H2mccMap()
        {
        }

        public override void LoadWellKnownTags()
        {
            if (this.TryGetTag(this.IndexHeader.Scenario, out var scnr))
            {
                this.Scenario = scnr;
            }

            if (this.TryGetTag(this.Header.LocalSounds, out var ugh))
            {
                this.LocalSounds =ugh;
            }

            if (this.TryGetTag(this.IndexHeader.Globals, out var globals))
            {
                this.Globals = globals;
            }
        }
    }
}
