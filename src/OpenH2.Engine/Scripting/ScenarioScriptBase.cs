using OpenH2.Core.Scripting;

namespace OpenH2.Engine.Scripting
{
    public class ScenarioScriptBase
    {
        public Team player;
        public short cinematic_letterbox_style;
        public AI ai_current_actor;
        public AI ai_current_squad;
        public short ai_combat_status_active;
        public short ai_combat_status_alert;
        public short ai_combat_status_idle;
        public short ai_combat_status_certain;
        public short ai_combat_status_visible;
        public short ai_combat_status_clear_los;
        public short ai_combat_status_uninspected;
        public short ai_combat_status_dangerous;
        public short ai_movement_combat;
        public short ai_movement_patrol;
        public short ai_movement_flee;
        
    }
}
