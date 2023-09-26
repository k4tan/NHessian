using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace NHessian.IO
{
    /// <summary>
    /// Writes hessian primitives to a stream.
    /// </summary>
    /// <remarks>
    /// The purpose of this class is to
    /// <list type="bullet">
    ///   <item>Provide re-usable primitive write methods that can be used by <see cref="HessianOutput"/></item>
    ///   <item>Provide a single point of optimization for stream writes</item>
    /// </list>
    /// </remarks>
    public sealed class HessianStreamWriter : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly Stream _stream;
        // Stream.WriteByte allocates an array in the default implementation. 
        // Use internal buffer to call Stream.Write that does not allocate.
        private byte[] _eightByteBuffer = new byte[8];

        /// <summary>
        /// Initialize a new instance of <see cref="HessianStreamWriter"/>.
        /// </summary>
        /// <param name="targetStream">
        /// The stream to be written to.
        /// </param>
        /// <param name="leaveOpen">
        /// true to leave the stream open after the <see cref="HessianStreamWriter"/> object is disposed;
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="targetStream"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="targetStream"/> is not writable.
        /// </exception>
        public HessianStreamWriter(Stream targetStream, bool leaveOpen = false)
        {
            _stream = targetStream ?? throw new ArgumentNullException(nameof(targetStream));
            _leaveOpen = leaveOpen;

            if (!targetStream.CanWrite)
                throw new ArgumentException("targetStream is not writable", nameof(targetStream));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_leaveOpen)
                _stream.Dispose();
        }

        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            _eightByteBuffer[0] = value;

            _stream.Write(_eightByteBuffer, 0, 1);
        }

        /// <summary>
        /// Writes a subarray of bytes to the stream.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/>
        /// to the current stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte <paramref name="offset"/> in buffer at which to begin copying
        /// bytes to the current stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the current stream.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="WriteBytes(byte[],int,int)"/> was called after the stream was closed.
        /// </exception>
        public void WriteBytes(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes the entire array to the stream.
        /// </summary>
        /// <param name="buffer">An array of bytes.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="WriteBytes(byte[],int,int)"/> was called after the stream was closed.
        /// </exception>
        public void WriteBytes(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a char to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value)
        {
            _eightByteBuffer[0] = (byte)value;

            _stream.Write(_eightByteBuffer, 0, 1);
        }

        /// <summary>
        /// Writes a double to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteDouble(double value) => WriteLong(BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Writes an int to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteInt(int value)
        {
            _eightByteBuffer[0] = (byte)(value >> 24);
            _eightByteBuffer[1] = (byte)(value >> 16);
            _eightByteBuffer[2] = (byte)(value >> 8);
            _eightByteBuffer[3] = (byte)value;

            _stream.Write(_eightByteBuffer, 0, 4);
        }

        /// <summary>
        /// Writes a long to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteLong(long value)
        {
            _eightByteBuffer[0] = (byte)(value >> 56);
            _eightByteBuffer[1] = (byte)(value >> 48);
            _eightByteBuffer[2] = (byte)(value >> 40);
            _eightByteBuffer[3] = (byte)(value >> 32);
            _eightByteBuffer[4] = (byte)(value >> 24);
            _eightByteBuffer[5] = (byte)(value >> 16);
            _eightByteBuffer[6] = (byte)(value >> 8);
            _eightByteBuffer[7] = (byte)value;

            _stream.Write(_eightByteBuffer, 0, 8);
        }

        /// <summary>
        /// Converts the string into the correct enociding and writes it to the stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteRawString(string value) 
        {
            /* NOTE to Surrogates
             * 
             * A 'char' in .Net represents a value 0-65535. This does not conver all Unicode characters!
             * A unicode character > 65535 has to be encoded as two chars (surrogate pair).
             * A surrogate is a value in the range 0xD00 - 0xDFF (reserved in unicode set).
             * So a surrogate is also a valid unicode character (although useless w/o with other half of the pair).
             * 
             * In this implementation, we do NOT turn surrogate pairs into 4-byte UTF-8.
             * Rather, surrogate pairs are sent as two 3-byte UTF-8.
             * Therefore, 4-byte UTF-8 is never used here.
             */
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c < 0x80)
                {
                    _eightByteBuffer[0] = (byte)c;
                    _stream.Write(_eightByteBuffer, 0, 1);
                }
                else if (c < 0x800)
                {
                    _eightByteBuffer[0] = (byte)(0xc0 + ((c >> 6) & 0x1f));
                    _eightByteBuffer[1] = (byte)(0x80 + (c & 0x3f));
                    _stream.Write(_eightByteBuffer, 0, 2);
                }
                else
                {
                    _eightByteBuffer[0] = (byte)(0xe0 + ((c >> 12) & 0xf));
                    _eightByteBuffer[1] = (byte)(0x80 + ((c >> 6) & 0x3f));
                    _eightByteBuffer[2] = (byte)(0x80 + (c & 0x3f));
                    _stream.Write(_eightByteBuffer, 0, 3);
                }
            }
        }

        /// <summary>
        /// Writes a short (Int16) to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteShort(int value)
        {
            _eightByteBuffer[0] = (byte)(value >> 8);
            _eightByteBuffer[1] = (byte)value;

            _stream.Write(_eightByteBuffer, 0, 2);
        }
    }
}