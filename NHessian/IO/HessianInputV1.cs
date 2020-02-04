using NHessian.IO.Deserialization;
using NHessian.IO.Utils;
using System;
using System.Collections.Generic;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="HessianInput"/> implementation for hessian V1.
    /// http://hessian.caucho.com/doc/hessian-1.0-spec.xtp
    /// </summary>
    public sealed class HessianInputV1 : HessianInput
    {
        /// <summary>
        /// Instance used to deserialize complex values like lists and maps.
        /// </summary>
        private readonly Deserializer _deserializer;

        private readonly List<object> _refs = new List<object>();

        /// <summary>
        /// Initializes a new instance of <see cref="HessianInputV1"/>.
        /// </summary>
        /// <param name="streamReader">
        /// The instance used to read from the stream.
        /// </param>
        /// <param name="typeBindings">
        /// Custom bindings remoted type strings.
        /// </param>
        public HessianInputV1(HessianStreamReader streamReader, TypeBindings typeBindings = null)
            : base(streamReader)
        {
            _deserializer = new Deserializer(this, typeBindings);
        }

        /// <inheritdoc/>
        public override bool ReadBool()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // The byte 'F' represents false and the byte 'T' represents true.
                case 'T': return true;
                case 'F': return false;

                default:
                    throw new UnsupportedTagException("bool", tag);
            }
        }

        /// <inheritdoc/>
        public override DateTime ReadDate()
        {
            var tag = _streamReader.Read();
            if (tag == 'd')
            {
                // Date represented by a 64-bits long of milliseconds since the epoch.
                var unixTimeMillis = _streamReader.ReadLong();
                return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMillis).DateTime.ToLocalTime();
            }

            throw new UnsupportedTagException("date", tag);
        }

        /// <inheritdoc/>
        public override double ReadDouble()
        {
            var tag = _streamReader.Read();
            if (tag == 'D')
                return _streamReader.ReadDouble();

            throw new UnsupportedTagException("double", tag);
        }

        /// <inheritdoc/>
        public override int ReadInt()
        {
            var tag = _streamReader.Read();
            if (tag == 'I')
                return _streamReader.ReadInt();

            throw new UnsupportedTagException("int", tag);
        }

        /// <inheritdoc/>
        public override long ReadLong()
        {
            var tag = _streamReader.Read();
            if (tag == 'L')
                return _streamReader.ReadLong();

            throw new UnsupportedTagException("long", tag);
        }

        /// <inheritdoc/>
        public override object ReadObject(Type expectedType = null)
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // The byte 'N' represents the null pointer.
                // null values are allowed in place of any string, xml, binary, list, map, or remote.
                case 'N':
                    return null;

                case 'V':
                    return _deserializer.ReadList(expectedType);

                case 'M':
                    return _deserializer.ReadMap(expectedType);

                case 'R':
                    return ReadRawRef();
                // The byte 'F' represents false and the byte 'T' represents true.
                case 'T':
                    return BooleanBoxes.TrueBox;

                case 'F':
                    return BooleanBoxes.FalseBox;
                // A 32-bit signed integer.
                case 'I':
                    return _streamReader.ReadInt();
                //  A 64-bit signed integer.
                case 'L':
                    return _streamReader.ReadLong();
                // A 64-bit IEEE floating pointer number.
                case 'D':
                    return _streamReader.ReadDouble();
                // Date represented by a 64-bits long of milliseconds since the epoch.
                case 'd':
                    var unixTimeMillis = _streamReader.ReadLong();
                    return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMillis).DateTime.ToLocalTime();

                // Strings are encoded in chunks. 'S' represents the final chunk and 's' represents any initial chunk.
                // An XML document encoded as a 16-bit unicode character string encoded in UTF-8. XML data is encoded in chunks.
                case 'S':
                case 'X':
                    return _streamReader.ReadString(_streamReader.ReadShort());

                case 's':
                case 'x':
                    return _streamReader.ReadString(_streamReader.ReadShort()) + ReadObject();
                // Binary data is encoded in chunks. 'B' represents the final chunk and 'b' represents any initial chunk.
                case 'B':
                    return ReadRawBytes();

                case 'b':
                    // possibly optimizations
                    //  - use Array.resize
                    //  - use ArrayPool
                    var arr1 = ReadRawBytes();
                    var rest = (byte[])ReadObject();
                    var combined = new byte[arr1.Length + rest.Length];
                    Buffer.BlockCopy(arr1, 0, combined, 0, arr1.Length);
                    Buffer.BlockCopy(rest, 0, combined, arr1.Length, rest.Length);
                    return combined;

                case 'r':
                    // A reference to a remote object. The remote has a type and a utf-8 string representing the object's URL.
                    throw new NotImplementedException("Remotes have not been implemented");
                default:
                    throw new NotSupportedException($"Unknown code {tag}");
            }
        }

        /// <inheritdoc/>
        public override string ReadString()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                case 'S':
                case 'X':
                    return _streamReader.ReadString(_streamReader.ReadShort());

                case 'N':
                    return null;

                case 's':
                case 'x':
                    // possible optimizations
                    // - use StrginBuilder in loop
                    return _streamReader.ReadString(_streamReader.ReadShort()) + ReadString();

                default:
                    throw new UnsupportedTagException("string", tag);
            }
        }

        internal override void AddRef(object obj) => _refs.Add(obj);

        internal override bool IsEnd() => _streamReader.Peek() == 'z';

        internal override void ReadEnd()
        {
            var tag = _streamReader.Read();
            if (tag == 'z')
                return;

            throw new UnsupportedTagException("end", tag);
        }

        internal override HessianRemoteException ReadFault()
        {
            var tag = _streamReader.Read();
            if (tag == 'f')
            {
                var dict = new Dictionary<object, object>();
                while (!IsEnd())
                    dict.Add(ReadObject(), ReadObject());

                return HessianRemoteException.FromRawMap(dict);
            }

            throw new UnsupportedTagException("fault", tag);
        }

        internal override bool ReadListStart(out string typeName, out int length)
        {
            typeName = _streamReader.Peek() == 't' ? ReadType() : null;
            length = IsLength() ? ReadLength() : -1;
            return true;
        }

        internal override bool ReadMapStart(out string typeName, out ClassDefinition typeDef)
        {
            typeName = IsType() ? ReadType() : null;
            typeDef = null;
            return true;
        }

        internal override void ReadReplyEnd()
        {
            var tag = _streamReader.Read();
            if (tag == 'z')
                return;

            throw new UnsupportedTagException("reply end", tag);
        }

        internal override bool ReadReplyStart()
        {
            var tag = _streamReader.Read();
            if (tag != 'r')
                new UnsupportedTagException("reply", tag);

            // version
            var major = _streamReader.Read();
            var minor = _streamReader.Read();
            if (major != 1 || minor != 0)
                throw new NotSupportedException($"Only version 1.0 of hessian is currently supported. was: {major}.{minor:D1}");

            // is it a fault?
            return _streamReader.Peek() == 'f';
        }

        internal override string ReadType()
        {
            var tag = _streamReader.Read();
            if (tag == 't')
                return _streamReader.ReadString(_streamReader.ReadShort());

            throw new UnsupportedTagException("type", tag);
        }

        private bool IsLength() => _streamReader.Peek() == 'l';

        private bool IsType() => _streamReader.Peek() == 't';

        private int ReadLength()
        {
            var tag = _streamReader.Read();
            if (tag == 'l')
                return _streamReader.ReadInt();

            throw new UnsupportedTagException("length", tag);
        }

        private byte[] ReadRawBytes()
        {
            var len = _streamReader.ReadShort();
            var buf = new byte[len];
            _streamReader.Read(buf);
            return buf;
        }

        private object ReadRawRef()
        {
            /*
             * An integer referring to a previous list or map instance.
             * As each list or map is read from the input stream, it is assigned the integer position in the stream,
             * i.e. the first list or map is '0', the next is '1', etc. A later ref can then use the previous object.
             */
            var idx = _streamReader.ReadInt();
            return _refs[idx];
        }
    }
}