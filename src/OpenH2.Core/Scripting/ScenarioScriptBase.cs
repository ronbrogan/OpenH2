using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Scripting
{
    public abstract class ScenarioScriptBase
    {
        protected IScriptEngine Engine;
        public Team player;
        public Team human;
        public Team prophet;
        public Team covenant;
        public Team sentinel;
        public Team heretic;
        public short cinematic_letterbox_style;
        public AI ai_current_actor;
        public AI ai_current_squad;
        public AIBehavior guard;
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
        public DamageState destroyed;
        public NavigationPoint _default;
        public NavigationPoint default_red;

        public abstract void InitializeData(ScenarioTag scenario);
    }
}
