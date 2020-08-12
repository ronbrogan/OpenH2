using System;

namespace OpenH2.Core.Scripting
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
        Static = 3,
        CommandScript = 5
    }
}
