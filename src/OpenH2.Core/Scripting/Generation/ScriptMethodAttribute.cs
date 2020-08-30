using System;

namespace OpenH2.Core.Scripting.Generation
{
    public class ScriptMethodAttribute : Attribute
    {
        public Lifecycle Type { get; }

        public ScriptMethodAttribute(Lifecycle type)
        {
            this.Type = type;
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
