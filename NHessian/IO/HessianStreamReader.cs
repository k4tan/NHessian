using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace NHessian.IO
{
    /// <summary>
    /// Reads hessian primitives from a stream.
    /// </summary>
    /// <remarks>
    /// The purpose of this class is to
    /// <list type="bullet">
    ///   <item>Provide re-usable primitive read methods that can be used by <see cref="HessianInput"/></item>
    ///   <item>Provide a single point of optimization for stream reads</item>
    /// </list>
    /// <para>
    /// This implementation is using a string cache that stores strings under
    /// a certain length (specified in constructor) and de-duplifies future occurances.
    /// If a string can be retrevied from the cache, no allocations are performed
    /// as the string is read into a temporary char buffer first and only turned into
    /// a string if it doesn't exist in the cache or is too long.
    /// </para>
    /// <para>
    /// This class uses unsafe code for string deserialization and comparison
    /// (Equality, HashCode).
    /// </para>
    /// </remarks>
    public sealed class HessianStreamReader : IDisposable
    {
        private readonly int _cacheMaxStringLength;
        private readonly bool _leaveOpen;
        private readonly Stream _stream;
        private readonly Dictionary<CharBuffer, string> _stringCache;
        private byte[] _buffer;
        private int _bufferCur;
        private int _bufferLen;
        private CharBuffer _charBuf;

        /// <summary>
        /// Initialize a new instance of <see cref="HessianStreamReader"/>.
        /// </summary>
        /// <param name="sourceStream">
        /// The stream to read from.
        /// </param>
        /// <param name="leaveOpen">
        /// true to leave the stream open after the <see cref="HessianStreamReader"/> object is disposed;
        /// </param>
        /// <param name="cacheMaxStringLength">
        /// Specifies the max length of a string that will be cached.
        /// A length of zero or less effectivly disables string caching.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="sourceStream"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="sourceStream"/> is not readable.
        /// </exception>
        public HessianStreamReader(Stream sourceStream, bool leaveOpen = false, int cacheMaxStringLength = 150)
        {
            _stream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
            _leaveOpen = leaveOpen;

            if (!sourceStream.CanRead)
                throw new ArgumentException("Can't read from source stream.", nameof(sourceStream));

            _buffer = ArrayPool<byte>.Shared.Rent(8 * 1024);

            _charBuf.charBuffer = ArrayPool<char>.Shared.Rent(128);
            _cacheMaxStringLength = cacheMaxStringLength;
            _stringCache = new Dictionary<CharBuffer, string>(new CharBufferEqualityComparer());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            ArrayPool<char>.Shared.Return(_charBuf.charBuffer);
            _buffer = null;
            _charBuf.charBuffer = null;
            _charBuf.length = 0;

            if (!_leaveOpen)
                _stream.Dispose();
        }

        /// <summary>
        /// Reads the next byte on the stream without advancing the cursor.
        /// </summary>
        /// <returns>
        /// Reuturns the next byte on the stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Peek()
        {
            if (_bufferCur < _bufferLen)
                return _buffer[_bufferCur];

            FetchNext();

            return _buffer[_bufferCur];
        }

        /// <summary>
        /// Reads the next byte on the stream.
        /// </summary>
        /// <returns>
        /// Reuturns the next byte on the stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read()
        {
            if (_bufferCur < _bufferLen)
                return _buffer[_bufferCur++];

            FetchNext();

            return _buffer[_bufferCur++];
        }

        /// <summary>
        /// Fills the <paramref name="buffer"/> with bytes from the stream.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that will be filled with bytes from the stream.
        /// Existing content will be overwritten.
        /// </param>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public void Read(byte[] buffer) => FillBuffer(buffer);

        /// <summary>
        /// Reads a 64bit double from the stream.
        /// </summary>
        /// <returns>
        /// Returns the read 64bit double value.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public double ReadDouble()
        {
            // A 64-bit IEEE floating pointer number.
            var asLong = ReadLong();
            var longBytes = BitConverter.GetBytes(asLong);
            return BitConverter.ToDouble(longBytes, 0);
        }

        /// <summary>
        /// Reads a 32bit integer from the stream.
        /// </summary>
        /// <returns>
        /// Returns the read 32bit integer.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public int ReadInt()
        {
            // int32 ::= b32 b24 b16 b8
            var b4 = Read();
            var b3 = Read();
            var b2 = Read();
            var b1 = Read();

            return (b4 << 24) + (b3 << 16) + (b2 << 8) + b1;
        }

        /// <summary>
        /// Reads the last read byte again.
        /// This allows it to basically go one byte back in the stream.
        /// </summary>
        /// <returns>
        /// Returns the last read byte again or -1 if no bytes have been read yet.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadLast()
        {
            // note: when FetchNext is called, we keep the last byte
            //       in the buffer so we can access it here
            if (_bufferCur > 0)
                return _buffer[_bufferCur - 1];

            // this should only happen if we haven't read any byte yet at all
            return -1;
        }

        /// <summary>
        /// Reads a 64bit long from the stream.
        /// </summary>
        /// <returns>
        /// Returns the read 64bit long.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public long ReadLong()
        {
            // int64 ::= b64 b56 b48 b40 b32 b24 b16 b8
            var b8 = (long)Read();
            var b7 = (long)Read();
            var b6 = (long)Read();
            var b5 = (long)Read();

            var b4 = (long)Read();
            var b3 = (long)Read();
            var b2 = (long)Read();
            var b1 = (long)Read();

            return (b8 << 56) + (b7 << 48) + (b6 << 40) + (b5 << 32) + (b4 << 24) + (b3 << 16) + (b2 << 8) + b1;
        }

        /// <summary>
        /// Reads a 16bit short from the stream.
        /// </summary>
        /// <returns>
        /// Returns the read 16bit short cast to integer.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public int ReadShort()
        {
            // int16 ::= b16 b8
            var b2 = Read();
            var b1 = Read();

            return (b2 << 8) + b1;
        }

        /// <summary>
        /// Reads a string from the stream of the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the string to be read.
        /// </param>
        /// <returns>
        /// The read string of the specifier <paramref name="length"/>.
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public string ReadString(int length)
        {
            // read string into char buffer
            if (_charBuf.charBuffer.Length < length)
            {
                // grow if to small
                ArrayPool<char>.Shared.Return(_charBuf.charBuffer);
                _charBuf.charBuffer = ArrayPool<char>.Shared.Rent(length);
            }

            ReadStringUnsafe(_charBuf.charBuffer, length);
            _charBuf.length = length;

            // caching
            if (_stringCache != null && length <= _cacheMaxStringLength)
            {
                if (!_stringCache.TryGetValue(_charBuf, out var s))
                {
                    // add string to cache
                    s = new string(_charBuf.charBuffer, 0, _charBuf.length);
                    var keyBuf = new CharBuffer
                    {
                        charBuffer = new char[_charBuf.length],
                        length = _charBuf.length
                    };
                    Array.Copy(_charBuf.charBuffer, 0, keyBuf.charBuffer, 0, _charBuf.length);
                    _stringCache.Add(keyBuf, s);
                }
                return s;
            }

            return new string(_charBuf.charBuffer, 0, _charBuf.length);
        }

        private void FetchNext()
        {
            /*
             * - copy latest byte to the begining of the new buffer
             * - fill rest of buffer (len - 1) with bytes from the stream
             * - store how many bytes we actually read
             * - reset cursor to 1 (0 is last read byte for ReadLast())
             */
            if (_bufferLen > 0)
                _buffer[0] = _buffer[_bufferLen - 1];

            _bufferLen = _stream.Read(_buffer, 1, _buffer.Length - 1) + 1;

            if (_bufferLen == 0)
                throw new EndOfStreamException();

            _bufferCur = 1;
        }

        private void FillBuffer(byte[] buffer, int offset = 0)
        {
            /*
             * Copies bytes from the stream buffer to target buffer
             * until it is full.
             * Because we might not have enough bytes in th stream buffer,
             * we might need to read it in chunks.
             * In that case:
             * - copy all available bytes
             * - buffer more from stream
             * - copy rest (calling the same method again)
             */
            var count = buffer.Length - offset;
            if (count > _bufferLen - _bufferCur)
            {
                // partial
                var available = _bufferLen - _bufferCur;

                Buffer.BlockCopy(_buffer, _bufferCur, buffer, offset, available);
                _bufferCur += count;

                FetchNext();
                FillBuffer(buffer, offset + available);
                return;
            }

            Buffer.BlockCopy(_buffer, _bufferCur, buffer, offset, count);
            _bufferCur += count;
        }

        private unsafe void ReadStringUnsafe(char[] targetBuffer, int readCount)
        {
            if (targetBuffer.Length < readCount)
            {
                throw new InvalidOperationException("Not enough space in the buffer to read UTF 8 string.");
            }

            fixed (char* ta = &targetBuffer[0])
            {
                var pTa = ta;
                var pTaLast = pTa + readCount;

                while (pTa < pTaLast)
                {
                    var b1 = Read();
                    if (b1 < 0x80)
                        *pTa++ = (char)b1;
                    else if ((b1 & 0xe0) == 0xc0)
                        *pTa++ = (char)(((b1 & 0x1f) << 6) + (Read() & 0x3f));
                    else if ((b1 & 0xf0) == 0xe0)
                        *pTa++ = (char)(((b1 & 0x0f) << 12) + ((Read() & 0x3f) << 6) + (Read() & 0x3f));
                    else if ((b1 & 0xf8) == 0xf0)
                        *pTa++ = (char)(((b1 & 0x07) << 18) + ((Read() & 0x3f) << 12) + ((Read() & 0x3f) << 6) + (Read() & 0x3f));
                    else
                        throw new Exception("Invalid unicode char");
                }
            }
        }

        #region CharBuffer

        /// <summary>
        /// Reusable buffer that always starts at 0 and has
        /// a custom length. <see cref="length"/> describes
        /// the 'usable' length of <see cref="charBuffer"/>
        /// independent of its actual size in memory.
        /// </summary>
        private struct CharBuffer
        {
            public char[] charBuffer;
            public int length;
        }

        #endregion CharBuffer

        #region CharBufferEqualityComparer

        private class CharBufferEqualityComparer : IEqualityComparer<CharBuffer>
        {
            public bool Equals(CharBuffer x, CharBuffer y)
            {
                if (x.length != y.length)
                    return false;

                int length = x.length;

                if (length == 0)
                    return true;

                unsafe
                {
                    fixed (char* ap = &x.charBuffer[0])
                    fixed (char* bp = &y.charBuffer[0])
                    {
                        char* a = ap;
                        char* b = bp;

                        // optimize by comparing longs instead of individual bytes
                        while (length >= 12)
                        {
                            if (*(long*)a != *(long*)b) return false;
                            if (*(long*)(a + 4) != *(long*)(b + 4)) return false;
                            if (*(long*)(a + 8) != *(long*)(b + 8)) return false;
                            a += 12;
                            b += 12;
                            length -= 12;
                        }

                        // compare byte by byte once we are < 12 length
                        while (length > 0)
                        {
                            if (*a != *b) break;
                            a++;
                            b++;
                            length--;
                        }

                        return length <= 0;
                    }
                }
            }

            public int GetHashCode(CharBuffer obj)
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                if (obj.length > 0)
                {
                    unsafe
                    {
                        fixed (char* src = &obj.charBuffer[0])
                        {
                            int* pint = (int*)src;
                            int len = obj.length;

                            while (len > 3)
                            {
                                hash1 = (hash1 << 5) + hash1 + (hash1 >> 27) ^ pint[0];
                                hash2 = (hash2 << 5) + hash2 + (hash2 >> 27) ^ pint[1];
                                pint += 2;
                                len -= 4;
                            }

                            if (len > 1)
                            {
                                hash1 = (hash1 << 5) + hash1 + (hash1 >> 27) ^ pint[0];
                            }
                        }
                    }
                }

                return hash1 + hash2 * 1566083941;
            }
        }

        #endregion CharBufferEqualityComparer
    }
}