using System;

namespace OpenH2.Core.Exceptions
{
    public static class Throw
    {
        public static void Exception(string description)
        {
            throw new Exception(description);
        }

        public static void InterpreterException(string description)
        {
            throw new InterpreterException(description);
        }

        public static void NotSupported(string description)
        {
            throw new NotSupportedException(description);
        }
    }
}
