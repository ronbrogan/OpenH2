using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Scripting
{
    public abstract class SquadBase
    {
        public ScenarioTag ScenarioTag { get; }

        public ScenarioTag.AiSquadDefinition Squad { get; }

        public string? SquadGroup { get; }

        public SquadBase(ScenarioTag scenarioTag, int squadIndex, string? group = null)
        {
            ScenarioTag = scenarioTag;
            SquadGroup = group;
            Squad = scenarioTag.AiSquadDefinitions[squadIndex];
        }
    }
}
