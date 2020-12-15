using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenH2.Core.Scripting.Execution
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct InterpreterResult
    {
        [FieldOffset(0)]
        public bool Boolean;

        [FieldOffset(0)]
        public int Int;

        [FieldOffset(0)]
        public short Short;

        [FieldOffset(0)]
        public float Float;

        [FieldOffset(4)]
        public ScriptDataType DataType;

        [FieldOffset(8)]
        public object Object;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat()
        {
            if (DataType == ScriptDataType.Float)
                return Float;
            else
                return (float)Int;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt()
        {
            if (DataType == ScriptDataType.Float)
                return (int)Float;
            else
                return Int;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetShort()
        {
            if (DataType == ScriptDataType.Float)
                return (short)Float;
            else
                return Short;
        }

        public static InterpreterResult From(bool v, ScriptDataType t = ScriptDataType.Boolean) => new InterpreterResult() { Boolean = v, DataType = t };
        public static InterpreterResult From(int v, ScriptDataType t = ScriptDataType.Int) => new InterpreterResult() { Int = v, DataType = t };
        public static InterpreterResult From(uint v, ScriptDataType t = ScriptDataType.Int) => new InterpreterResult() { Int = (int)v, DataType = t };
        public static InterpreterResult From(short v, ScriptDataType t = ScriptDataType.Short) => new InterpreterResult() { Short = v, DataType = t };
        public static InterpreterResult From(ushort v, ScriptDataType t = ScriptDataType.Short) => new InterpreterResult() { Short = (short)v, DataType = t };
        public static InterpreterResult From(object v, ScriptDataType t = ScriptDataType.Entity) => new InterpreterResult() { Object = v, DataType = t };

        public static implicit operator float(InterpreterResult r)
        {
            return r.Float;
        }

        public static implicit operator int(InterpreterResult r)
        {
            return r.Int;
        }

        public static implicit operator short(InterpreterResult r)
        {
            return r.Short;
        }
    }
}
