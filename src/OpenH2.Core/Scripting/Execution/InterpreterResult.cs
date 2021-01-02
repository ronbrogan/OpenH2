using OpenH2.Core.Exceptions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenH2.Core.Scripting.Execution
{
    public struct StackFrame
    {
        public Queue<InterpreterResult> Locals;
        public ScenarioTag.ScriptSyntaxNode OriginatingNode;
        public ScenarioTag.ScriptSyntaxNode Current;
        public ushort Next;
    }

    public struct InterpreterState
    {
        private int TopOfStack;
        private StackFrame[] CallStack;
        public bool Yield;

        public InterpreterResult Result;

        public static InterpreterState Create()
        {
            var s = new InterpreterState();
            s.TopOfStack = -1;
            s.CallStack = new StackFrame[32];
            s.Result = InterpreterResult.From();
            return s;
        }

        public ref StackFrame TopFrame => ref this.CallStack[this.TopOfStack];

        public void Push(StackFrame frame)
        {
            try
            {
                this.CallStack[++this.TopOfStack] = frame;
            }
            catch (IndexOutOfRangeException)
            {
                Throw.InterpreterException("Interpreter CallStack has exceeded the allowed depth");
            }
        }

        public ref StackFrame Pop()
        {
            try
            {
                return ref this.CallStack[this.TopOfStack--];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InterpreterException("Interpreter CallStack is already empty");
            }
        }

        public int FrameCount => TopOfStack+1;
    }

    public struct VariableReference
    {
        public int Index;

        public VariableReference(int i)
        {
            this.Index = i;
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct InterpreterResult
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
        public object? Object;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat()
        {
            if (DataType == ScriptDataType.Float)
                return Float;
            else
                return Int;
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

        public void Add(InterpreterResult operand)
        {
            Debug.Assert(this.DataType == ScriptDataType.Float);

            this.Float += operand.GetFloat();
        }

        public void Subtract(InterpreterResult operand)
        {
            Debug.Assert(this.DataType == ScriptDataType.Float);

            this.Float -= operand.GetFloat();
        }

        public void Multiply(InterpreterResult operand)
        {
            Debug.Assert(this.DataType == ScriptDataType.Float);

            this.Float *= operand.GetFloat();
        }

        public void Divide(InterpreterResult operand)
        {
            Debug.Assert(this.DataType == ScriptDataType.Float);

            this.Float /= operand.GetFloat();
        }

        public static InterpreterResult Min(InterpreterResult left, InterpreterResult right)
        {
            Debug.Assert(left.DataType == ScriptDataType.Float);

            return Difference(left, right) < 0 ? left : right;
        }

        public static InterpreterResult Max(InterpreterResult left, InterpreterResult right)
        {
            Debug.Assert(left.DataType == ScriptDataType.Float);

            return Difference(left, right) < 0 ? right : left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Difference(InterpreterResult left, InterpreterResult right)
        {
            Debug.Assert(left.DataType == ScriptDataType.Float);

            return left.Float - right.GetFloat();
        }

        public static InterpreterResult From(bool v, ScriptDataType t = ScriptDataType.Boolean) => new InterpreterResult() { Boolean = v, DataType = t };
        public static InterpreterResult From(int v, ScriptDataType t = ScriptDataType.Int) => new InterpreterResult() { Int = v, DataType = t };
        public static InterpreterResult From(uint v, ScriptDataType t = ScriptDataType.Int) => new InterpreterResult() { Int = (int)v, DataType = t };
        public static InterpreterResult From(short v, ScriptDataType t = ScriptDataType.Short) => new InterpreterResult() { Short = v, DataType = t };
        public static InterpreterResult From(ushort v, ScriptDataType t = ScriptDataType.Short) => new InterpreterResult() { Short = (short)v, DataType = t };
        public static InterpreterResult From(float v, ScriptDataType t = ScriptDataType.Float) => new InterpreterResult() { Float = v, DataType = t };
        public static InterpreterResult From(object? v, ScriptDataType t = ScriptDataType.Entity) => new InterpreterResult() { Object = v, DataType = t };
        public static InterpreterResult From() => new InterpreterResult() { DataType = ScriptDataType.Void };

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
