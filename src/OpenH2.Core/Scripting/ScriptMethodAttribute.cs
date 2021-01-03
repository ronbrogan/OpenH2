using System;

namespace OpenH2.Core.Scripting
{
    public class ScriptMethodAttribute : Attribute
    {
        public Lifecycle Lifecycle { get; }
        public ushort Id { get; }

        public ScriptMethodAttribute(ushort id, Lifecycle lifecycle)
        {
            this.Lifecycle = lifecycle;
            this.Id = id;
        }
    }

    public enum Lifecycle : ushort
    {
        Startup = 0,
        Dormant = 1,
        Continuous = 2,
        Static = 3,
        Stub = 4,
        CommandScript = 5
    }
}
