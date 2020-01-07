using System;
using System.IO;

namespace NHessian.IO
{
    /// <summary>
    /// Represents a stream of hessian encoded data that can be read from.
    /// </summary>
    public abstract class HessianInput
    {
        /// <summary>
        /// Instance used to read from the stream.
        /// </summary>
        protected readonly HessianStreamReader _streamReader;

        /// <summary>
        /// Initializes a new instance of <see cref="HessianInput"/>.
        /// </summary>
        /// <param name="streamReader">
        /// The instance used to read from the stream.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="streamReader"/> is null.
        /// </exception>
        protected HessianInput(HessianStreamReader streamReader)
        {
            _streamReader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
        }

        /// <summary>
        /// Read a boolean from the stream.
        /// </summary>
        /// <remarks>
        /// This methods main purpose is to avoid boxing behavior
        /// of <see cref="ReadObject(Type)"/> when we already know what
        /// value to expect.
        /// </remarks>
        /// <returns>
        /// Returns the read boolean.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract bool ReadBool();

        /// <summary>
        /// Read a date from the stream.
        /// </summary>
        /// <remarks>
        /// This methods main purpose is to avoid boxing behavior
        /// of <see cref="ReadObject(Type)"/> when we already know what
        /// value to expect.
        /// </remarks>
        /// <returns>
        /// Returns the read date.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract DateTime ReadDate();

        /// <summary>
        /// Read a double from the stream.
        /// </summary>
        /// <remarks>
        /// This methods main purpose is to avoid boxing behavior
        /// of <see cref="ReadObject(Type)"/> when we already know what
        /// value to expect.
        /// </remarks>
        /// <returns>
        /// Returns the read double.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract double ReadDouble();

        /// <summary>
        /// Read an int from the stream.
        /// </summary>
        /// <remarks>
        /// This methods main purpose is to avoid boxing behavior
        /// of <see cref="ReadObject(Type)"/> when we already know what
        /// value to expect.
        /// </remarks>
        /// <returns>
        /// Returns the read int.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract int ReadInt();

        /// <summary>
        /// Read a long from the stream.
        /// </summary>
        /// <remarks>
        /// This methods main purpose is to avoid boxing behavior
        /// of <see cref="ReadObject(Type)"/> when we already know what
        /// value to expect.
        /// </remarks>
        /// <returns>
        /// Returns the read long.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract long ReadLong();

        /// <summary>
        /// Read an object from the stream. Optionally, the expected
        /// type can be specified. The method will try its best
        /// to conform to the expectation.
        /// </summary>
        /// <returns>
        /// Returns the read object.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract object ReadObject(Type expectedType = null);

        /// <summary>
        /// Reads an rpc reply from the stream.
        /// This returns either the reply value or <see cref="HessianRemoteException"/>
        /// if the server responded with an error.
        /// </summary>
        /// <param name="expectedType">
        /// The expected reponse value type.
        /// </param>
        /// <returns>
        /// The reply value if successful, otherwise an instance of <see cref="HessianRemoteException"/>.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public object ReadReply(Type expectedType = null)
        {
            var isFault = ReadReplyStart();

            if (isFault)
            {
                var fault = ReadFault();
                ReadReplyEnd();
                return fault;
            }

            var reply = ReadObject(expectedType);
            ReadReplyEnd();
            return reply;
        }

        /// <summary>
        /// Read a string from the stream.
        /// </summary>
        /// <returns>
        /// Returns the read string.
        /// </returns>
        /// <exception cref="UnsupportedTagException">
        /// If the tag encountered on the stream is not valid.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// If the end of the stream is reached.
        /// </exception>
        public abstract string ReadString();

        internal abstract void AddRef(object obj);

        internal abstract bool IsEnd();

        internal abstract void ReadEnd();

        internal abstract HessianRemoteException ReadFault();

        internal abstract bool ReadListStart(out string typeName, out int length);

        internal abstract bool ReadMapStart(out string typeName, out ClassDefinition typeDef);

        internal abstract void ReadReplyEnd();

        internal abstract bool ReadReplyStart();

        internal abstract string ReadType();
    }
}