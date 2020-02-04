using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

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
        public void WriteByte(byte value) => _stream.WriteByte(value);

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
        public void WriteBytes(byte[] buffer) => WriteBytes(buffer, 0, buffer.Length);

        /// <summary>
        /// Writes a char to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value) => WriteByte((byte)value);

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
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
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
            WriteByte((byte)(value >> 56));
            WriteByte((byte)(value >> 48));
            WriteByte((byte)(value >> 40));
            WriteByte((byte)(value >> 32));
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
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
        public void WriteRawString(string value) => WriteBytes(Encoding.UTF8.GetBytes(value));

        /// <summary>
        /// Writes a short (Int16) to the stream.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <exception cref="ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public void WriteShort(int value)
        {
            WriteByte((byte)(value >> 8));
            WriteByte((byte)value);
        }
    }
}