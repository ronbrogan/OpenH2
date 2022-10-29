using System;

namespace OpenH2.Core.Scripting
{
    public sealed class OriginScenarioAttribute : Attribute
    {
        public string ScenarioId { get; set; }

        public OriginScenarioAttribute(string scenarioId)
        {
            this.ScenarioId = scenarioId;
        }
    }

    public sealed class SpawnCountsAttribute : Attribute
    {
        public int Normal { get; }
        public int Legendary { get; }

        public SpawnCountsAttribute(int normal, int legendary)
        {
            Normal = normal;
            Legendary = legendary;
        }
    }
}
