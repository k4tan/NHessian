using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace NHessian.IO
{
    internal class StringInternPool : IDisposable
    {
        private readonly int _hashCodeRandomizer;
        private Entry[] _entries;

        // Mask is array size - 1. Mask should always be (2^n)-1.
        // Picking the right size can have performance implications.
        // The larger the array, the longer it takes to initialize it with null (ctor).
        // But if to small, entry chains might get deep and search takes longer.
        private const int _mask = 127;

        public StringInternPool()
        {
            _hashCodeRandomizer = Environment.TickCount;

            var arrLen = _mask + 1;
            _entries = ArrayPool<Entry>.Shared.Rent(arrLen);
            // ArrayPool array might contain items; initialize with nulls
            Array.Clear(_entries, 0, arrLen);
        }

        public string GetOrAdd(char[] charBuf, int length)
        {
            if (charBuf.Length < length)
                throw new ArgumentException("length exceeds end of buffer", nameof(length));

            var hashCode = TextGetHashCode(charBuf, length);

            var index = hashCode & _mask;
            var head = _entries[index];

            if (head == null)
            {
                var s = new string(charBuf, 0, length);
                _entries[index] = new Entry(s, hashCode);
                return s;
            }
            else
            {
                for (var current = head; ; current = current.Next)
                {
                    if (hashCode == current.HashCode && TextEquals(charBuf, length, current.Value))
                        return current.Value;

                    if (current.Next == null)
                    {
                        var s = new string(charBuf, 0, length);
                        current.Next = new Entry(s, hashCode);
                        return s;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TextEquals(char[] charBuf, int length, string s)
        {
            return MemoryExtensions.Equals(charBuf.AsSpan(0, length), s.AsSpan(), StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TextGetHashCode(char[] charBuf, int length)
        {
            // simple hash code calculator; probably good enough for our purpose
            int hash1 = length + _hashCodeRandomizer;
            int hash2 = hash1;
            
            switch (length)
            {
                case 0:
                    break;
                case 1:
                    hash1 = (hash1 << 5) + hash1 + (hash1 >> 27) ^ charBuf[0];
                    break;
                default:
                    hash1 = (hash1 << 5) + hash1 + (hash1 >> 27) ^ charBuf[0];
                    hash2 = (hash2 << 5) + hash2 + (hash2 >> 27) ^ charBuf[1];
                    break;
            }

            return hash1 + (hash2 * 1566083941);
        }

        public void Dispose()
        {
            ArrayPool<Entry>.Shared.Return(_entries);
            _entries = null;
        }

        #region Entry

        private sealed class Entry
        {
            public Entry(string value, int hasCode)
            {
                Value = value;
                HashCode = hasCode;
            }

            public string Value { get; }
            public int HashCode { get; }
            public Entry Next { get; set; }
        }

        #endregion Entry
    }
}