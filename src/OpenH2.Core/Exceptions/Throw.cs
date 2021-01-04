using OpenH2.Core.Scripting.Execution;
using System;

namespace OpenH2.Core.Exceptions
{
    public static class Throw
    {
        public static void Exception(string description)
        {
            throw new Exception(description);
        }

        public static Exception InterpreterException(string description, InterpreterState state)
        {
            throw new InterpreterException(description + Environment.NewLine + state.SerializeCallStack());
        }

        public static void NotSupported(string description)
        {
            throw new NotSupportedException(description);
        }
    }
}
