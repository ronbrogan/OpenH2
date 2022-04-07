using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenH2.Core.Extensions
{
    internal class Utf8Extensions
    {
    }
    public unsafe class PinnedUtf8
    {
        private static ConditionalWeakTable<string, byte[]> pins = new ConditionalWeakTable<string, byte[]>();
        public readonly byte* Address;

        public PinnedUtf8(string value)
        {
            this.Address = Get(value);
        }

        public static byte* Get(string value)
        {
            // Maybe shouldn't use framework interning?
            var interned = string.Intern(value);

            if (pins.TryGetValue(interned, out var arr))
            {
                return (byte*)Unsafe.AsPointer(ref arr[0]);
            }

            lock (pins)
            {
                if (pins.TryGetValue(interned, out arr))
                {
                    return (byte*)Unsafe.AsPointer(ref arr[0]);
                }

                arr = GC.AllocateUninitializedArray<byte>(Encoding.UTF8.GetByteCount(interned) + 1, true);
                Encoding.UTF8.GetBytes(interned, arr);
                arr[arr.Length - 1] = 0; // Null terminate
                pins.Add(interned, arr);
            }

            return (byte*)Unsafe.AsPointer(ref arr[0]); // Just grab pointer since it's already pinned
        }

        public static implicit operator byte*(PinnedUtf8 obj)
        {
            return obj.Address;
        }
    }
}
