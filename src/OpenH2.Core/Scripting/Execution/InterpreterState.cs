using OpenH2.Core.Exceptions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Scripting.Execution
{
    public struct StackFrame
    {
        public Queue<InterpreterResult> Locals;
        public ScenarioTag.ScriptSyntaxNode OriginatingNode;
        public ScenarioTag.ScriptSyntaxNode Current;
        public ushort Next;
    }

    public struct VariableReference
    {
        public int Index;

        public VariableReference(int i)
        {
            this.Index = i;
        }
    }

    public struct InterpreterState
    {
        private int TopOfStack;
        private StackFrame[] CallStack;
        public bool Yield;
        public int OriginalNodeIndex;
        public InterpreterResult Result;

        public static InterpreterState Create(int nodeIndex)
        {
            var s = new InterpreterState();
            s.OriginalNodeIndex = nodeIndex;
            s.TopOfStack = -1;
            s.CallStack = new StackFrame[32];
            s.Result = InterpreterResult.From();
            return s;
        }

        public ref StackFrame TopFrame => ref this.CallStack[this.TopOfStack];

        public void Reset()
        {
            this.TopOfStack = -1;
            this.Yield = false;
            this.Result = InterpreterResult.From();
        }

        public void Push(StackFrame frame)
        {
            try
            {
                this.CallStack[++this.TopOfStack] = frame;
            }
            catch (IndexOutOfRangeException)
            {
                Throw.InterpreterException("Interpreter CallStack has exceeded the allowed depth", this);
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

        public string SerializeCallStack()
        {
            var sb = new StringBuilder();

            for(var i = 0; i < FrameCount; i++)
            {
                var frame = CallStack[i];
                sb.AppendLine($"  At Op:{frame.OriginatingNode.OperationId} consuming Op:{frame.Current.OperationId} and next of {frame.Next}");
            }

            return sb.ToString();
        }
    }
}
