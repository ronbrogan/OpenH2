using System;

namespace OpenH2.Core.Scripting
{
    public sealed class ScriptImplementationAttribute : Attribute
    {
        public int Id { get; }

        public ScriptImplementationAttribute(int id)
        {
            this.Id = id;
        }
    }
}
