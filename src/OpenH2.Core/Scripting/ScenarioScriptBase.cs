using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Scripting
{
    public abstract class ScenarioScriptBase
    {
        public ScenarioTag Scenario { get; protected set; }
        protected IScriptEngine Engine;
        public ITeam player;
        public ITeam human;
        public ITeam prophet;
        public ITeam covenant;
        public ITeam sentinel;
        public ITeam heretic;
        public short cinematic_letterbox_style;
        public IAiActorDefinition ai_current_actor;
        public IAiActorDefinition ai_current_squad;
        public IAIBehavior guard;
        public short ai_combat_status_active;
        public short ai_combat_status_alert;
        public short ai_combat_status_idle;
        public short ai_combat_status_definite;
        public short ai_combat_status_certain;
        public short ai_combat_status_visible;
        public short ai_combat_status_clear_los;
        public short ai_combat_status_uninspected;
        public short ai_combat_status_dangerous;
        public short ai_movement_combat;
        public short ai_movement_patrol;
        public short ai_movement_flee;
        public IDamageState destroyed;
        public INavigationPoint _default;
        public INavigationPoint default_red;

        public abstract void InitializeData(ScenarioTag scenario, Scene scene);
    }
}
