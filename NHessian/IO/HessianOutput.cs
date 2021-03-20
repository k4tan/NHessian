using System;
using System.Collections.Generic;

namespace NHessian.IO
{
    /// <summary>
    /// Represents a hessian output stream that can be written to.
    /// </summary>
    public abstract class HessianOutput
    {
        /// <summary>
        /// Instance used to write to the stream.
        /// </summary>
        protected readonly HessianStreamWriter _streamWriter;

        /// <summary>
        /// Initializes a new instance of <see cref="HessianOutput"/>.
        /// </summary>
        /// <param name="streamWriter">
        /// The instance used to write to the stream.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="streamWriter"/> is null.
        /// </exception>
        protected HessianOutput(HessianStreamWriter streamWriter)
        {
            _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        }

        /// <summary>
        /// Writes a <see cref="bool"/> value to the stream..
        /// </summary>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteBool(bool value);

        /// <summary>
        /// Writes a <see cref="byte"/> array to the stream..
        /// </summary>
        /// <param name="value">
        /// The array to write.
        /// </param>
        public abstract void WriteByteArray(byte[] value);

        /// <summary>
        /// Writes an rpc call to the stream.
        /// </summary>
        /// <param name="methodName">
        /// The name of the service method that is invoked.
        /// </param>
        /// <param name="args">
        /// The method arguments.
        /// </param>
        /// <param name="headers">
        /// Optional headers.
        /// </param>
        public void WriteCall(
            string methodName,
            object[] args,
            Tuple<string, object>[] headers = null)
        {
            WriteCallStart(methodName, args, headers);

            foreach (var arg in args)
                WriteObject(arg);

            WriteCallEnd();
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value to the stream..
        /// </summary>
        /// <remarks>
        /// <see cref="DateTimeKind.Unspecified"/> is assumed to be <see cref="DateTimeKind.Local"/>.
        /// </remarks>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteDate(DateTime value);

        /// <summary>
        /// Writes a <see cref="double"/> value to the stream..
        /// </summary>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteDouble(double value);

        /// <summary>
        /// Writes a <see cref="int"/> value to the stream..
        /// </summary>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteInt(int value);

        /// <summary>
        /// Writes a <see cref="long"/> value to the stream..
        /// </summary>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteLong(long value);

        /// <summary>
        /// Writes null to the stream.
        /// </summary>
        public abstract void WriteNull();

        /// <summary>
        /// Writes an object to the stream.
        /// </summary>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        public abstract void WriteObject(object obj);

        /// <summary>
        /// Writes a <see cref="string"/> value to the stream..
        /// </summary>
        /// <param name="value">
        /// The value to write.
        /// </param>
        public abstract void WriteString(string value);

        internal abstract void WriteCallEnd();

        internal abstract void WriteCallStart(
            string methodName,
            object[] args,
            Tuple<string, object>[] headers);

        internal abstract void WriteListEnd();

        /// <returns>Returns true if the list has an end; otherwise false</returns>
        internal abstract bool WriteListStart(int length, string typeName);

        internal abstract int WriteMapDefinition(string typeName, IReadOnlyCollection<string> fieldNames);

        internal abstract void WriteMapEnd();

        internal abstract void WriteMapStart(string typeName);

        internal abstract void WriteMapStart(int definitionIdx);
    }
}