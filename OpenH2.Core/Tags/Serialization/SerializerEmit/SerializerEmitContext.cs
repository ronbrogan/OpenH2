using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace OpenH2.Core.Tags.Serialization.SerializerEmit
{
    internal class SerializerEmitContext
    {
        public Func<Type, SerializerEmitContext> GetNestedContext;
        public ILGenerator MethodIL;
        public Func<MethodInfo> GetMethodInfo;
    }
}
