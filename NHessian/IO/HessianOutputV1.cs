using NHessian.IO.Serialization;
using NHessian.IO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="HessianOutput"/> implementation for hessian V1.
    /// http://hessian.caucho.com/doc/hessian-1.0-spec.xtp
    /// </summary>
    public sealed class HessianOutputV1 : HessianOutput
    {
        private readonly Serializer _serializer;

        private readonly Dictionary<object, int> _valueRefs
            = new Dictionary<object, int>(ReferenceEqualityComparer.Default);

        /// <summary>
        /// Initializes a new instance of <see cref="HessianOutputV1"/>.
        /// </summary>
        /// <param name="streamWriter">
        /// The instance used to write to the stream.
        /// </param>
        /// <param name="typeBindings">
        /// Custom type bindings.
        /// </param>
        public HessianOutputV1(HessianStreamWriter streamWriter, TypeBindings typeBindings = null)
            : base(streamWriter)
        {
            _serializer = new Serializer(this, typeBindings);
        }

        /// <inheritdoc/>
        public override void WriteBool(bool value)
        {
            // The byte 'F' represents false and the byte 'T' represents true.
            if (value) _streamWriter.WriteChar('T');
            else _streamWriter.WriteChar('F');
        }

        /// <inheritdoc/>
        public override void WriteByteArray(byte[] value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            const int maxChunkLength = 0xFFFF;
            var cur = 0;
            while (value.Length - cur > maxChunkLength)
            {
                // to large
                // we can only encode 16bit length
                // encode in chunks
                _streamWriter.WriteChar('b');
                _streamWriter.WriteShort(maxChunkLength);
                _streamWriter.WriteBytes(value, cur, maxChunkLength);
                cur += maxChunkLength;
            }

            // final chunk
            _streamWriter.WriteChar('B');
            var finalChunkLen = value.Length - cur;
            _streamWriter.WriteShort(finalChunkLen);
            _streamWriter.WriteBytes(value, cur, finalChunkLen);
        }

        /// <inheritdoc/>
        public override void WriteDate(DateTime value)
        {
            // Date represented by a 64-bits long of milliseconds since the epoch.
            var l = new DateTimeOffset(value).ToUnixTimeMilliseconds();
            _streamWriter.WriteChar('d');
            _streamWriter.WriteLong(l);
        }

        /// <inheritdoc/>
        public override void WriteDouble(double value)
        {
            // A 64-bit IEEE floating pointer number.
            _streamWriter.WriteChar('D');
            _streamWriter.WriteDouble(value);
        }

        /// <inheritdoc/>
        public override void WriteInt(int value)
        {
            // An integer is represented by the byte 'I' followed by the 4-bytes of the integer in big-endian order
            _streamWriter.WriteChar('I');
            _streamWriter.WriteInt(value);
        }

        /// <inheritdoc/>
        public override void WriteLong(long value)
        {
            // An long is represented by the byte 'L' followed by the 8-bytes of the integer in big-endian order
            _streamWriter.WriteChar('L');
            _streamWriter.WriteLong(value);
        }

        /// <inheritdoc/>
        public override void WriteNull()
        {
            // The byte 'N' represents the null pointer.
            // null values are allowed in place of any string, xml, binary, list, map, or remote.
            _streamWriter.WriteChar('N');
        }

        /// <inheritdoc/>
        public override void WriteObject(object obj)
        {
            if (obj == null)
            {
                WriteNull();
                return;
            }

            // NOTE 'remote' not implemented
            switch (obj)
            {
                case string s:
                    WriteString(s);
                    break;

                case bool b:
                    WriteBool(b);
                    break;

                case int i:
                    WriteInt(i);
                    break;

                case char c:
                    WriteInt(c);
                    break;

                case long l:
                    WriteLong(l);
                    break;

                case double d:
                    WriteDouble(d);
                    break;

                case float f:
                    WriteDouble(f);
                    break;

                case DateTime date:
                    WriteDate(date);
                    break;

                case byte[] byteArr:
                    WriteByteArray(byteArr);
                    break;
                // TODO is this still creating a jump table?
                case IEnumerable list when !(obj is IDictionary):
                    if (!TryWriteReference(list))
                    {
                        AddValueRef(obj);
                        _serializer.WriteList(list);
                    }
                    break;

                case object _:
                    if (!TryWriteReference(obj))
                    {
                        AddValueRef(obj);
                        _serializer.WriteMap(obj);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public override void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            const int maxChunkLength = 0xFFFF;
            var cur = value;
            while (cur.Length > maxChunkLength)
            {
                // to large
                // we can only encode 16bit length
                // encode in chunks
                _streamWriter.WriteChar('s');
                _streamWriter.WriteShort(maxChunkLength);
                _streamWriter.WriteRawString(cur.Substring(0, maxChunkLength));
                cur = cur.Substring(maxChunkLength);
            }

            // final chunk
            _streamWriter.WriteChar('S');
            _streamWriter.WriteShort(cur.Length);
            _streamWriter.WriteRawString(cur);
        }

        internal override void WriteCallEnd() => _streamWriter.WriteChar('z');

        internal override void WriteCallStart(
            string methodName,
            IReadOnlyList<object> args,
            IReadOnlyList<Tuple<string, object>> headers)
        {
            /* NOTE no support foroverloads */
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

            if (args is null)
                throw new ArgumentNullException(nameof(args));

            _streamWriter.WriteChar('c');
            _streamWriter.WriteByte(1); // major
            _streamWriter.WriteByte(0); // minor

            if (headers != null)
            {
                foreach (var header in headers.Where(it => it.Item1 != null))
                {
                    // header key
                    _streamWriter.WriteChar('H');
                    _streamWriter.WriteShort(header.Item1.Length);
                    _streamWriter.WriteRawString(header.Item1);
                    // header value
                    WriteObject(header.Item2);
                }
            }

            // method name
            _streamWriter.WriteChar('m');
            _streamWriter.WriteShort(methodName.Length);
            _streamWriter.WriteRawString(methodName);
        }

        internal override void WriteListEnd() => _streamWriter.WriteChar('z');

        internal override bool WriteListStart(int length, string typeName)
        {
            _streamWriter.WriteChar('V');

            if (!string.IsNullOrWhiteSpace(typeName))
                WriteType(typeName);

            if (length >= 0)
            {
                _streamWriter.WriteChar('l');
                _streamWriter.WriteInt(length);
            }

            return true;
        }

        internal override int WriteMapDefinition(string typeName, IReadOnlyCollection<string> fieldNames)
        {
            return -1;
        }

        internal override void WriteMapEnd() => _streamWriter.WriteChar('z');

        internal override void WriteMapStart(string typeName)
        {
            _streamWriter.WriteChar('M');

            if (!string.IsNullOrWhiteSpace(typeName))
                WriteType(typeName);
        }

        internal override void WriteMapStart(int definitionIdx)
        {
            throw new NotSupportedException("Hessian V1 does not support compact objects");
        }

        private void AddValueRef(object obj) => _valueRefs.Add(obj, _valueRefs.Count);

        private bool TryWriteReference(object obj)
        {
            if (_valueRefs.TryGetValue(obj, out var idx))
            {
                // have seen object before... write reference
                _streamWriter.WriteChar('R');
                _streamWriter.WriteInt(idx);

                return true;
            }
            return false;
        }

        private void WriteType(string type)
        {
            _streamWriter.WriteChar('t');
            _streamWriter.WriteShort(type.Length);
            _streamWriter.WriteRawString(type);
        }
    }
}