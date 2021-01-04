using OpenH2.Core.GameObjects;
using System;
using Result = OpenH2.Core.Scripting.Execution.InterpreterResult;

namespace OpenH2.Core.Scripting
{
    public class GlobalVariableSet
    {
        public IAiActorDefinition? ai_current_actor;
        public IAiActorDefinition? ai_current_squad;
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
        public short cinematic_letterbox_style;

        public Result this[ushort i]
        {
            get { return Get(i); }
            set { Set(i, value); }
        }

        public Result Get(ushort id)
        {
            var result = id switch
            {
                33563 => Result.From(this.ai_current_squad, ScriptDataType.AI),
                33564 => Result.From(this.ai_current_actor, ScriptDataType.AI),
                33572 => Result.From(this.ai_combat_status_idle),
                33573 => Result.From(this.ai_combat_status_alert),
                33574 => Result.From(this.ai_combat_status_active),
                33575 => Result.From(this.ai_combat_status_uninspected),
                33576 => Result.From(this.ai_combat_status_definite),
                33577 => Result.From(this.ai_combat_status_certain),
                33578 => Result.From(this.ai_combat_status_visible),
                33579 => Result.From(this.ai_combat_status_clear_los),
                33580 => Result.From(this.ai_combat_status_dangerous),
                33581 => Result.From(this.ai_movement_patrol),
                33583 => Result.From(this.ai_movement_combat),
                33584 => Result.From(this.ai_movement_flee),
                33754 => Result.From(this.cinematic_letterbox_style),
                _ => throw new ArgumentException("Value is not a valid ID")
            };

            result.VariableIndex = id;
            return result;
        }

        public void Set(ushort id, Result value)
        {
            value.VariableIndex = id;

            switch (id)
            {
                case 33563: this.ai_current_squad = value.Object as IAiActorDefinition; break;
                case 33564: this.ai_current_actor = value.Object as IAiActorDefinition; break;
                case 33572: this.ai_combat_status_idle = value.Short; break;
                case 33573: this.ai_combat_status_alert = value.Short; break;
                case 33574: this.ai_combat_status_active = value.Short; break;
                case 33575: this.ai_combat_status_uninspected = value.Short; break;
                case 33576: this.ai_combat_status_definite = value.Short; break;
                case 33577: this.ai_combat_status_certain = value.Short; break;
                case 33578: this.ai_combat_status_visible = value.Short; break;
                case 33579: this.ai_combat_status_clear_los = value.Short; break;
                case 33580: this.ai_combat_status_dangerous = value.Short; break;
                case 33581: this.ai_movement_patrol = value.Short; break;
                case 33583: this.ai_movement_combat = value.Short; break;
                case 33584: this.ai_movement_flee = value.Short; break;
                case 33754: this.cinematic_letterbox_style = value.Short; break;
                default: throw new ArgumentException("Value is not a valid ID");
            }
        }
    }
}
