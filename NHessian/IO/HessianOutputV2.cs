using NHessian.IO.Serialization;
using NHessian.IO.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="HessianOutput"/> implementation for hessian V2.
    /// http://hessian.caucho.com/doc/hessian-serialization.html
    /// http://hessian.caucho.com/doc/hessian-ws.html
    /// </summary>
    public sealed class HessianOutputV2 : HessianOutput
    {
        private readonly Serializer _serializer;

        private readonly Dictionary<string, int> _typeDefinitionRefs = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _typeRefs = new Dictionary<string, int>();

        private readonly Dictionary<object, int> _valueRefs
                            = new Dictionary<object, int>(ReferenceEqualityComparer.Default);

        /// <summary>
        /// Initializes a new instance of <see cref="HessianOutputV2"/>.
        /// </summary>
        /// <param name="streamWriter">
        /// The instance used to write to the stream.
        /// </param>
        /// <param name="typeBindings">
        /// Custom type bindings.
        /// </param>
        public HessianOutputV2(HessianStreamWriter streamWriter, TypeBindings typeBindings = null)
            : base(streamWriter)
        {
            _serializer = new Serializer(this, typeBindings);
        }

        /// <inheritdoc/>
        public override void WriteBool(bool value)
        {
            // # boolean true/false
            // boolean    ::= 'T'
            //            ::= 'F'
            _streamWriter.WriteChar(value ? 'T' : 'F');
        }

        /// <inheritdoc/>
        public override void WriteByteArray(byte[] value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            WriteByteArray(value, 0, value.Length);
        }

        /// <inheritdoc/>
        public override void WriteDate(DateTime value)
        {
            // # time in UTC encoded as 64-bit long milliseconds since epoch
            // date   ::= x4a b7 b6 b5 b4 b3 b2 b1 b0
            //        ::= x4b b3 b2 b1 b0              # minutes since epoch

            var ms = new DateTimeOffset(value).ToUnixTimeMilliseconds();

            if (ms % 60000 == 0)
            {
                // short date
                // minute granularity (1000ms * 60s)
                // ::= x4b b3 b2 b1 b0
                _streamWriter.WriteByte(0x4b);
                _streamWriter.WriteInt((int)(ms / 60000));
                return;
            }
            // long date
            // ::= x4a b7 b6 b5 b4 b3 b2 b1 b0
            _streamWriter.WriteByte(0x4a);
            _streamWriter.WriteLong(ms);
        }

        /// <inheritdoc/>
        public override void WriteDouble(double value)
        {
            // # 64-bit IEEE double
            var asInt = (int)value;

            if (Math.Abs(asInt - value) < double.Epsilon)
            {
                if (asInt == 0)
                {
                    // ::= x5b                   # 0.0
                    _streamWriter.WriteByte(0x5b);
                    return;
                }
                if (asInt == 1)
                {
                    // ::= x5c                   # 1.0
                    _streamWriter.WriteByte(0x5c);
                    return;
                }
                if (asInt >= sbyte.MinValue && asInt <= sbyte.MaxValue)
                {
                    // ::= x5d b0                # byte cast to double (-128.0 to 127.0)
                    _streamWriter.WriteByte(0x5d);
                    _streamWriter.WriteByte((byte)asInt);
                    return;
                }
                if (asInt >= short.MinValue && asInt <= short.MaxValue)
                {
                    // ::= x5e b1 b0             # short cast to double
                    _streamWriter.WriteByte(0x5e);
                    _streamWriter.WriteShort(asInt);
                    return;
                }
            }

            // not sure why this check is works... (used to comply with java implementation)
            int mills = (int)(value * 1000);
            if (0.001 * mills == value)
            {
                // ::= x5f b3 b2 b1 b0       # 32-bit float cast to double
                _streamWriter.WriteByte(0x5f);
                _streamWriter.WriteInt(mills);
                //_streamWriter.WriteFloat((float)value);
                return;
            }
            // ::= 'D' b7 b6 b5 b4 b3 b2 b1 b0
            _streamWriter.WriteChar('D');
            _streamWriter.WriteDouble(value);
        }

        /// <inheritdoc/>
        public override void WriteInt(int value)
        {
            // # 32-bit signed integer
            if (value >= -0x10 && value <= 0x2f)
            {
                // ::= [x80-xbf]             # -x10 to x2f
                _streamWriter.WriteByte((byte)(value + 0x90));
            }
            else if (value >= -0x800 && value <= 0x7ff)
            {
                // ::= [xc0-xcf] b0          # -x800 to x7ff
                _streamWriter.WriteByte((byte)((value >> 8) + 0xc8));
                _streamWriter.WriteByte((byte)value);
            }
            else if (value >= -0x40000 && value <= 0x3ffff)
            {
                // ::= [xd0-xd7] b1 b0       # -x40000 to x3ffff
                _streamWriter.WriteByte((byte)((value >> 16) + 0xd4));
                _streamWriter.WriteByte((byte)(value >> 8));
                _streamWriter.WriteByte((byte)value);
            }
            else
            {
                // 'I' b3 b2 b1 b0
                _streamWriter.WriteChar('I');
                _streamWriter.WriteInt(value);
            }
        }

        /// <inheritdoc/>
        public override void WriteLong(long value)
        {
            // # 64-bit signed long integer
            if (value >= -0x08 && value <= 0x0f)
            {
                // ::= [xd8-xef]             # -x08 to x0f
                _streamWriter.WriteByte((byte)(value + 0xe0));
            }
            else if (value >= -0x800 && value <= 0x7ff)
            {
                // ::= [xf0-xff] b0          # -x800 to x7ff
                _streamWriter.WriteByte((byte)((value >> 8) + 0xf8));
                _streamWriter.WriteByte((byte)value);
            }
            else if (value >= -0x40000 && value <= 0x3ffff)
            {
                // ::= [x38-x3f] b1 b0       # -x40000 to x3ffff
                _streamWriter.WriteByte((byte)((value >> 16) + 0x3c));
                _streamWriter.WriteByte((byte)(value >> 8));
                _streamWriter.WriteByte((byte)value);
            }
            else if (value >= -0x80000000L && value <= 0x7fffffffL)
            {
                // ::= x59 b3 b2 b1 b0       # 32-bit integer cast to long
                // Longs which fit into 32-bits are encoded in five octets with the leading byte 0x59.
                _streamWriter.WriteByte(0x59);
                _streamWriter.WriteByte((byte)(value >> 24));
                _streamWriter.WriteByte((byte)(value >> 16));
                _streamWriter.WriteByte((byte)(value >> 8));
                _streamWriter.WriteByte((byte)value);
            }
            else
            {
                // ::= L b7 b6 b5 b4 b3 b2 b1 b0
                _streamWriter.WriteChar('L');
                _streamWriter.WriteLong(value);
            }
        }

        /// <inheritdoc/>
        public override void WriteNull()
        {
            // # null value
            // null       ::= 'N'
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

            switch (obj)
            {
                case string s:
                    WriteString(s);
                    break;

                case bool b:
                    WriteBool(b);
                    break;

                case DateTime date:
                    WriteDate(date);
                    break;

                case int i:
                    WriteInt(i);
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

                case byte[] byteArr:
                    WriteByteArray(byteArr);
                    break;

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

            // # UTF-8 encoded character string split into 64k chunks
            var len = value.Length;
            if (len <= 31)
            {
                // ::= [x00-x1f] <utf8-data>         # string of length 0-31
                _streamWriter.WriteByte((byte)len);
                _streamWriter.WriteRawString(value);
            }
            else if (len <= 1023)
            {
                // ::= [x30-x34] b0 <utf8-data>         # string of length 0-1023
                _streamWriter.WriteByte((byte)(0x30 + (len >> 8)));
                _streamWriter.WriteByte((byte)len);
                _streamWriter.WriteRawString(value);
            }
            else if (len <= 65535)
            {
                // ::= 'S' b1 b0 <utf8-data>         # string of length 0-65535
                _streamWriter.WriteChar('S');
                _streamWriter.WriteShort(len);
                _streamWriter.WriteRawString(value);
            }
            else
            {
                // ::= x52 b1 b0 <utf8-data> string  # non-final chunk
                var s = value.Substring(0, 65535);
                _streamWriter.WriteByte(0x52);
                _streamWriter.WriteShort(s.Length);
                _streamWriter.WriteRawString(s);
                // encode next chunk
                WriteString(value.Substring(65535));
            }
        }

        internal override void WriteCallEnd()
        {
        }

        internal override void WriteCallStart(
            string methodName,
            object[] args,
            Tuple<string, object>[] headers)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

            if (args is null)
                throw new ArgumentNullException(nameof(args));

            // version header
            _streamWriter.WriteChar('H');
            _streamWriter.WriteByte(2); // major version
            _streamWriter.WriteByte(0); // minor version

            // call
            _streamWriter.WriteChar('C');
            WriteString(methodName);
            WriteInt(args.Length);
        }

        internal override void WriteListEnd() => _streamWriter.WriteChar('Z');

        internal override bool WriteListStart(int length, string typeName)
        {
            var hasType = !string.IsNullOrWhiteSpace(typeName);

            if (length < 0)
            {
                if (hasType)
                {
                    _streamWriter.WriteByte(0x55);
                    WriteType(typeName);
                }
                else
                    _streamWriter.WriteByte(0x57);

                return true;
            }

            if (length <= 0x07)
            {
                if (hasType)
                {
                    _streamWriter.WriteByte((byte)(0x70 + (byte)length));
                    WriteType(typeName);
                }
                else
                    _streamWriter.WriteByte((byte)(0x78 + (byte)length));
            }
            else
            {
                if (hasType)
                {
                    _streamWriter.WriteChar('V');
                    WriteType(typeName);
                }
                else
                    _streamWriter.WriteByte(0x58);

                WriteInt(length);
            }

            return false;
        }

        internal override int WriteMapDefinition(string typeName, IReadOnlyCollection<string> fieldNames)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return -1;

            if (!_typeDefinitionRefs.TryGetValue(typeName, out var idx))
            {
                idx = _typeDefinitionRefs.Count;
                _typeDefinitionRefs.Add(typeName, idx);

                _streamWriter.WriteChar('C');
                WriteString(typeName);
                WriteInt(fieldNames.Count);

                foreach (var fieldName in fieldNames)
                    WriteString(fieldName);
            }

            return idx;
        }

        internal override void WriteMapEnd() => _streamWriter.WriteChar('Z');

        internal override void WriteMapStart(string typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                _streamWriter.WriteChar('M');
                WriteType(typeName);
            }
            else
                _streamWriter.WriteChar('H');
        }

        internal override void WriteMapStart(int definitionIdx)
        {
            if (definitionIdx < 0)
                throw new ArgumentOutOfRangeException(nameof(definitionIdx));

            if (definitionIdx < 0x0f)
                _streamWriter.WriteByte((byte)(0x60 + definitionIdx));
            else
            {
                _streamWriter.WriteChar('O');
                WriteInt(definitionIdx);
            }
        }

        private void AddValueRef(object obj) => _valueRefs.Add(obj, _valueRefs.Count);

        private bool TryWriteReference(object obj)
        {
            if (_valueRefs.TryGetValue(obj, out var idx))
            {
                // have seen object before... write reference
                _streamWriter.WriteByte(0x51);
                WriteInt(idx);
                return true;
            }
            return false;
        }

        private void WriteByteArray(byte[] value, int startIndex, int count)
        {
            // # 8-bit binary data split into 64k chunks
            if (count <= 15)
            {
                // ::= [x20-x2f] <binary-data>        # binary data of length 0-15
                _streamWriter.WriteByte((byte)(0x20 + count));
                _streamWriter.WriteBytes(value, startIndex, count);
            }
            else if (count <= 1023)
            {
                // ::= [x34-x37] b0 <binary-data>        # binary data of length 0-1023
                _streamWriter.WriteByte((byte)(0x34 + (count >> 8)));
                _streamWriter.WriteByte((byte)count);
                _streamWriter.WriteBytes(value, startIndex, count);
            }
            else if (count <= 65535)
            {
                // ::= 'B' b1 b0 <binary-data>        # final chunk
                _streamWriter.WriteChar('B');
                _streamWriter.WriteShort(count);
                _streamWriter.WriteBytes(value, startIndex, count);
            }
            else
            {
                // ::= x41 b1 b0 <binary-data> binary # non-final chunk
                _streamWriter.WriteByte(0x41);
                _streamWriter.WriteShort(65535);
                _streamWriter.WriteBytes(value, startIndex, 65535);

                // next chunk
                WriteByteArray(value, startIndex + 65535, value.Length - (startIndex + 65535));
            }
        }

        private void WriteType(string typeName)
        {
            if (_typeRefs.TryGetValue(typeName, out var idx))
                WriteInt(idx);
            else
            {
                _typeRefs.Add(typeName, _typeRefs.Count);
                WriteString(typeName);
            }
        }
    }
}