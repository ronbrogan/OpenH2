using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis
{
    public class ContinuationStack<T> 
    {
        private Stack<(T, bool)> store = new Stack<(T, bool)>();

        public void PushFull(T value)
        {
            store.Push((value, true));
            store.Push((value, false));
        }

        public void PushSeparate(T value, bool isContinuation)
        {
            store.Push((value, isContinuation));
        }

        public bool TryPop(out T value, out bool isContinuation)
        {
            if(store.TryPop(out var values))
            {
                value = values.Item1;
                isContinuation = values.Item2;
                return true;
            }

            value = default;
            isContinuation = default;
            return false;
        }
    }
}
