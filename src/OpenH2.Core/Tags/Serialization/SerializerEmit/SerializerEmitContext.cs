using System;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenH2.Core.Tags.Serialization.SerializerEmit
{
    internal class SerializerEmitContext
    {
        public Func<Type, SerializerEmitContext> GetNestedContext;
        public ILGenerator MethodIL;
        public Func<MethodInfo> GetMethodInfo;
    }
}
