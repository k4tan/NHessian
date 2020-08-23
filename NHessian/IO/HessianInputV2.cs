using NHessian.IO.Deserialization;
using NHessian.IO.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="HessianInput"/> implementation for hessian V2.
    /// http://hessian.caucho.com/doc/hessian-serialization.html
    /// http://hessian.caucho.com/doc/hessian-ws.html
    /// </summary>
    public sealed class HessianInputV2 : HessianInput
    {
        /// <summary>
        /// Instance used to deserialize complex values like lists and maps.
        /// </summary>
        private readonly Deserializer _deserializer;

        private readonly List<object> _refs = new List<object>();
        private readonly List<string> _typeRefs = new List<string>();
        private readonly List<ClassDefinition> _typeDefinitionRefs = new List<ClassDefinition>();

        /// <summary>
        /// Initializes a new instance of <see cref="HessianInputV2"/>.
        /// </summary>
        /// <param name="streamReader">
        /// The instance used to read from the stream.
        /// </param>
        /// <param name="typeBindings">
        /// Custom bindings remoted type strings.
        /// </param>
        public HessianInputV2(HessianStreamReader streamReader, TypeBindings typeBindings = null)
            : base(streamReader)
        {
            _deserializer = new Deserializer(this, typeBindings);
        }

        /// <inheritdoc/>
        public override object ReadObject(Type expectedType = null)
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // null       ::= 'N'
                case 'N':
                    return null;

                // ref        ::= x51 int       # reference to nth map/list/object
                case 0x51:
                    return _refs[ReadInt()];

                // boolean    ::= 'T'           # true
                //            ::= 'F'           # false
                case 'T':
                    return BooleanBoxes.TrueBox;

                case 'F':
                    return BooleanBoxes.FalseBox;

                /* INT variations */
                // Compact: single octet integers
                // ::= [x80-xbf]     # -x10 to x3f
#pragma warning disable format
                case 0x80: case 0x81: case 0x82: case 0x83: case 0x84: case 0x85: case 0x86: case 0x87:
                case 0x88: case 0x89: case 0x8a: case 0x8b: case 0x8c: case 0x8d: case 0x8e: case 0x8f:
                case 0x90: case 0x91: case 0x92: case 0x93: case 0x94: case 0x95: case 0x96: case 0x97:
                case 0x98: case 0x99: case 0x9a: case 0x9b: case 0x9c: case 0x9d: case 0x9e: case 0x9f:
                case 0xa0: case 0xa1: case 0xa2: case 0xa3: case 0xa4: case 0xa5: case 0xa6: case 0xa7:
                case 0xa8: case 0xa9: case 0xaa: case 0xab: case 0xac: case 0xad: case 0xae: case 0xaf:
                case 0xb0: case 0xb1: case 0xb2: case 0xb3: case 0xb4: case 0xb5: case 0xb6: case 0xb7:
                case 0xb8: case 0xb9: case 0xba: case 0xbb: case 0xbc: case 0xbd: case 0xbe: case 0xbf:
#pragma warning restore format
                    // value = code - 0x90
                    return tag - 0x90;

                // Compact: two octet integers
                // ::= [xc0-xcf] b0  # -x800 to x7ff
#pragma warning disable format
                case 0xc0: case 0xc1: case 0xc2: case 0xc3: case 0xc4: case 0xc5: case 0xc6: case 0xc7:
                case 0xc8: case 0xc9: case 0xca: case 0xcb: case 0xcc: case 0xcd: case 0xce: case 0xcf:
#pragma warning restore format
                    // value = ((code - 0xc8) << 8) + b0;
                    return (tag - 0xc8 << 8) + _streamReader.Read();

                // Compact: three octet integers
                // ::= [xd0-xd7] b1 b0  # -x40000 to x3ffff
#pragma warning disable format
                case 0xd0: case 0xd1: case 0xd2: case 0xd3: case 0xd4: case 0xd5: case 0xd6: case 0xd7:
#pragma warning restore format
                    // value = ((code - 0xd4) << 16) + (b1 << 8) + b0;
                    return (tag - 0xd4 << 16) + (_streamReader.Read() << 8) + _streamReader.Read();

                // ::= 'I' b3 b2 b1 b0
                case 'I':
                    return _streamReader.ReadInt();

                /* LONG variations */
                // Compact: single octet longs
                // ::= [xd8-xef]     # -x08 to x0f
#pragma warning disable format
                case 0xd8: case 0xd9: case 0xda: case 0xdb: case 0xdc: case 0xdd: case 0xde: case 0xdf:
                case 0xe0: case 0xe1: case 0xe2: case 0xe3: case 0xe4: case 0xe5: case 0xe6: case 0xe7:
                case 0xe8: case 0xe9: case 0xea: case 0xeb: case 0xec: case 0xed: case 0xee: case 0xef:
#pragma warning restore format
                    // value = (code - 0xe0)
                    return (long)(tag - 0xe0);

                // Compact: two octet longs
                // ::=[xf0-xff] b0   # -x800 to x7ff
#pragma warning disable format
                case 0xf0: case 0xf1: case 0xf2: case 0xf3: case 0xf4: case 0xf5: case 0xf6: case 0xf7:
                case 0xf8: case 0xf9: case 0xfa: case 0xfb: case 0xfc: case 0xfd: case 0xfe: case 0xff:
#pragma warning restore format
                    // value = ((code - 0xf8) << 8) + b0
                    return (long)((tag - 0xf8 << 8) + _streamReader.Read());

                // Compact: three octet longs
                // ::= [x38-x3f] b1 b0   # -x40000 to x3ffff
#pragma warning disable format
                case 0x38: case 0x39: case 0x3a: case 0x3b: case 0x3c: case 0x3d: case 0x3e: case 0x3f:
#pragma warning restore format
                    // value = ((code - 0x3c) << 16) + (b1 << 8) + b0
                    return (long)((tag - 0x3c << 16) + (_streamReader.Read() << 8) + _streamReader.Read());

                // Compact: four octet longs
                // ::= x59 b3 b2 b1 b0   # 32-bit integer cast to long
                case 0x59:
                    return (long)_streamReader.ReadInt();

                // ::= 'L' b7 b6 b5 b4 b3 b2 b1 b0
                case 'L':
                    return _streamReader.ReadLong();

                /* DOUBLE variations */
                // Compact: double zero
                // ::= x5b # 0.0
                case 0x5b:
                    return DoubleBoxes.Zero;

                // Compact: double one
                // ::= x5c # 1.0
                case 0x5c:
                    return DoubleBoxes.One;

                // Compact: double octet
                // ::= x5d b0
                case 0x5d:
                    // value = (double) b0 # (-128.0 to 127.0)
                    return (double)(sbyte)_streamReader.Read();

                // Compact: double short
                // ::= x5e b1 b0
                case 0x5e:
                    // value = (double) (256 * b1 + b0)
                    return (double)(short)_streamReader.ReadShort();

                // Compact: double float
                // ::= x5f b3 b2 b1 b0
                // NOTE This seems to be broken
                //      Nothing in the spec says something about * 0.001.
                //      Maybe a quirk in the java implementation?
                case 0x5f:
                    return _streamReader.ReadInt() * 0.001;

                // ::= 'D' b7 b6 b5 b4 b3 b2 b1 b0
                case 'D':
                    return _streamReader.ReadDouble();

                /* DATE */
                // Date represented by a 64-bits long of milliseconds since the epoch.
                // ::= x4a b7 b6 b5 b4 b3 b2 b1 b0
                case 0x4a:
                    var unixTimeMillis = _streamReader.ReadLong();
                    return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMillis).DateTime.ToLocalTime();

                // Date represented by a 32-bits int of minutes since the epoch.
                // ::= x4b b3 b2 b1 b0       # minutes since epoch
                case 0x4b:
                    var unixTimeMinutes = _streamReader.ReadInt();
                    return DateTimeOffset.FromUnixTimeSeconds(unixTimeMinutes * 60).DateTime.ToLocalTime();

                /* STRING */
                // Compact: short strings (code is the length)
                // ::= [x00-x1f] <utf8-data>       # string of length 0-31
#pragma warning disable format
                case 0x00: case 0x01: case 0x02: case 0x03: case 0x04: case 0x05: case 0x06: case 0x07:
                case 0x08: case 0x09: case 0x0a: case 0x0b: case 0x0c: case 0x0d: case 0x0e: case 0x0f:
                case 0x10: case 0x11: case 0x12: case 0x13: case 0x14: case 0x15: case 0x16: case 0x17:
                case 0x18: case 0x19: case 0x1a: case 0x1b: case 0x1c: case 0x1d: case 0x1e: case 0x1f:
#pragma warning restore format
                    return _streamReader.ReadInternedString(tag);

                // ::= [x30-x33] b0 <utf8-data>   # string of length 0-1023
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    var strLen = ((tag - 0x30) << 8) + _streamReader.Read();
                    return strLen <= STRING_INTERN_THRESHOLD ? _streamReader.ReadInternedString(strLen) : _streamReader.ReadString(strLen);

                // ::= 'S' b1 b0 <utf8-data> # string of length 0-65535
                case 'S':
                    return _streamReader.ReadString(_streamReader.ReadShort());

                // ::= x52 b1 b0 <utf8-data> string # non-final chunk
                case 0x52:
                    return ReadMultiPartString();

                /* BINARY */
                // Compact: short binary
                // ::= [x20-x2f] <binary-data>    # binary data of length 0-15
#pragma warning disable format
                case 0x20: case 0x21: case 0x22: case 0x23: case 0x24: case 0x25: case 0x26: case 0x27:
                case 0x28: case 0x29: case 0x2a: case 0x2b: case 0x2c: case 0x2d: case 0x2e: case 0x2f:
#pragma warning restore format
                    // len = code - 0x20
                    var shortBinary = new byte[tag - 0x20];
                    _streamReader.Read(shortBinary);
                    return shortBinary;

                // ::= [x34-x37] <binary-data>        # binary data of length 0-1023
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                    var mediumBinary = new byte[((tag - 0x34) << 8) + _streamReader.Read()];
                    _streamReader.Read(mediumBinary);
                    return mediumBinary;

                // ::= x41 b1 b0 <binary-data> binary # non-final chunk
                case 0x41:
                    var arr1 = ReadRawBytes();
                    var rest = (byte[])ReadObject();
                    var combined = new byte[arr1.Length + rest.Length];
                    Buffer.BlockCopy(arr1, 0, combined, 0, arr1.Length);
                    Buffer.BlockCopy(rest, 0, combined, arr1.Length, rest.Length);
                    return combined;

                // Binary data is encoded in chunks. 'B' represents the final chunk and 'b' represents any initial chunk.
                // ::= 'B' b1 b0 <binary-data>        # final chunk
                case 'B':
                    return ReadRawBytes();

                /* LIST variations */
                // ::= x55 type value* 'Z'    # variable-length list
                case 0x55:

                // ::= 'V' type int value*    # fixed-length list
                case 'V': // 0x56

                // ::= x57 value * 'Z'        # variable-length untyped list
                case 0x57:

                // ::= x58 int value*         # fixed-length untyped list
                case 0x58:

                // ::= [x70-77] type value*   # fixed-length typed list
#pragma warning disable format
                case 0x70: case 0x71: case 0x72: case 0x73: case 0x74: case 0x75: case 0x76: case 0x77:
                // ::= [x78-7f] value*       # fixed-length untyped list
                case 0x78: case 0x79: case 0x7a: case 0x7b: case 0x7c: case 0x7d: case 0x7e: case 0x7f:
#pragma warning restore format
                    return _deserializer.ReadList(expectedType);

                // class-def  ::= 'C' string int string*    # definition for an object (compact map)
                case 'C':
                    var typeName = ReadString();
                    var fieldCount = ReadInt();
                    var fieldNames = new string[fieldCount];
                    for (int i = 0; i < fieldCount; i++)
                        fieldNames[i] = ReadString();
                    _typeDefinitionRefs.Add(new ClassDefinition(typeName, fieldNames));

                    return ReadObject(expectedType);

                /* MAP variations */
                // ::= 'H' (value value)* 'Z'       # untyped key, value
                case 'H':

                // ::= 'M' type (value value)* 'Z'  # key, value map pairs
                case 'M':

                /* OBJECT variations */
                // ::= 'O' int value*
                case 'O':

                // ::= [x60-x6f] value*
#pragma warning disable format
                case 0x60: case 0x61: case 0x62: case 0x63: case 0x64: case 0x65: case 0x66: case 0x67:
                case 0x68: case 0x69: case 0x6a: case 0x6b: case 0x6c: case 0x6d: case 0x6e: case 0x6f:
#pragma warning restore format
                    return _deserializer.ReadMap(expectedType);

                default:
                    throw new NotSupportedException($"Unknown code 0x{tag:X2} ('{(char)tag}')");
            }
        }

        /// <inheritdoc/>
        public override bool ReadBool()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // boolean    ::= 'T'           # true
                //            ::= 'F'           # false
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
            switch (tag)
            {
                // Date represented by a 64-bits long of milliseconds since the epoch.
                // ::= x4a b7 b6 b5 b4 b3 b2 b1 b0
                case 0x4a:
                    var unixTimeMillis = _streamReader.ReadLong();
                    return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMillis).DateTime.ToLocalTime();

                // Date represented by a 32-bits int of minutes since the epoch.
                // ::= x4b b3 b2 b1 b0       # minutes since epoch
                case 0x4b:
                    var unixTimeMinutes = _streamReader.ReadInt();
                    return DateTimeOffset.FromUnixTimeSeconds(unixTimeMinutes * 60).DateTime.ToLocalTime();

                default:
                    throw new UnsupportedTagException("date", tag);
            }
        }

        /// <inheritdoc/>
        public override int ReadInt()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // Compact: single octet integers
                // ::= [x80-xbf]
#pragma warning disable format
                case 0x80: case 0x81: case 0x82: case 0x83: case 0x84: case 0x85: case 0x86: case 0x87:
                case 0x88: case 0x89: case 0x8a: case 0x8b: case 0x8c: case 0x8d: case 0x8e: case 0x8f:
                case 0x90: case 0x91: case 0x92: case 0x93: case 0x94: case 0x95: case 0x96: case 0x97:
                case 0x98: case 0x99: case 0x9a: case 0x9b: case 0x9c: case 0x9d: case 0x9e: case 0x9f:
                case 0xa0: case 0xa1: case 0xa2: case 0xa3: case 0xa4: case 0xa5: case 0xa6: case 0xa7:
                case 0xa8: case 0xa9: case 0xaa: case 0xab: case 0xac: case 0xad: case 0xae: case 0xaf:
                case 0xb0: case 0xb1: case 0xb2: case 0xb3: case 0xb4: case 0xb5: case 0xb6: case 0xb7:
                case 0xb8: case 0xb9: case 0xba: case 0xbb: case 0xbc: case 0xbd: case 0xbe: case 0xbf:
#pragma warning restore format
                    // value = code - 0x90
                    return tag - 0x90;

                // Compact: two octet integers
                // [xc0-xcf] b0
#pragma warning disable format
                case 0xc0: case 0xc1: case 0xc2: case 0xc3: case 0xc4: case 0xc5: case 0xc6: case 0xc7:
                case 0xc8: case 0xc9: case 0xca: case 0xcb: case 0xcc: case 0xcd: case 0xce: case 0xcf:
#pragma warning restore format
                    // value = ((code - 0xc8) << 8) + b0;
                    return (tag - 0xc8 << 8) + _streamReader.Read();

                // Compact: three octet integers
                // ::= [xd0-xd7] b1 b0
#pragma warning disable format
                case 0xd0: case 0xd1: case 0xd2: case 0xd3: case 0xd4: case 0xd5: case 0xd6: case 0xd7:
#pragma warning restore format
                    // value = ((code - 0xd4) << 16) + (b1 << 8) + b0;
                    return (tag - 0xd4 << 16) + (_streamReader.Read() << 8) + _streamReader.Read();

                // ::= 'I' b3 b2 b1 b0
                case 'I':
                    return _streamReader.ReadInt();

                default:
                    throw new UnsupportedTagException("int", tag);
            }
        }

        /// <inheritdoc/>
        public override long ReadLong()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // Compact: single octet longs
                // ::= [xd8-xef]
#pragma warning disable format
                case 0xd8: case 0xd9: case 0xda: case 0xdb: case 0xdc: case 0xdd: case 0xde: case 0xdf:
                case 0xe0: case 0xe1: case 0xe2: case 0xe3: case 0xe4: case 0xe5: case 0xe6: case 0xe7:
                case 0xe8: case 0xe9: case 0xea: case 0xeb: case 0xec: case 0xed: case 0xee: case 0xef:
#pragma warning restore format
                    // value = (code - 0xe0)
                    return tag - 0xe0;

                // Compact: two octet longs
                // [xf0-xff] b0
#pragma warning disable format
                case 0xf0: case 0xf1: case 0xf2: case 0xf3: case 0xf4: case 0xf5: case 0xf6: case 0xf7:
                case 0xf8: case 0xf9: case 0xfa: case 0xfb: case 0xfc: case 0xfd: case 0xfe: case 0xff:
#pragma warning restore format
                    // value = ((code - 0xf8) << 8) + b0
                    return (tag - 0xf8 << 8) + _streamReader.Read();

                // Compact: three octet longs
                // [x38-x3f] b1 b0
#pragma warning disable format
                case 0x38: case 0x39: case 0x3a: case 0x3b: case 0x3c: case 0x3d: case 0x3e: case 0x3f:
#pragma warning restore format
                    // value = ((code - 0x3c) << 16) + (b1 << 8) + b0
                    return (tag - 0x3c << 16) + (_streamReader.Read() << 8) + _streamReader.Read();

                // Compact: four octet longs
                // ::= x59 b3 b2 b1 b0 # 32-bit integer cast to long
                case 0x59:
                    return _streamReader.ReadInt();

                // ::= 'L' b7 b6 b5 b4 b3 b2 b1 b0
                case 'L':
                    return _streamReader.ReadLong();

                default:
                    throw new UnsupportedTagException("long", tag);
            }
        }

        /// <inheritdoc/>
        public override double ReadDouble()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                // Compact: double zero
                // ::= x5b # 0.0
                case 0x5b:
                    return 0.0;

                // Compact: double one
                // ::= x5c # 1.0
                case 0x5c:
                    return 1.0;

                // Compact: double octet
                // ::= x5d b0
                case 0x5d:
                    // value = (double) b0 # (-128.0 to 127.0)
                    return (sbyte)_streamReader.Read();

                // Compact: double short
                // ::= x5e b1 b0
                case 0x5e:
                    // value = (double) (256 * b1 + b0)
                    return (short)_streamReader.ReadShort();

                // Compact: double float
                // ::= x5f b3 b2 b1 b0
                // NOTE This seems to be broken
                //      Nothing in the spec says something about * 0.001.
                //      Maybe a quirk in the java implementation?
                case 0x5f:
                    return _streamReader.ReadInt() * 0.001;

                // ::= 'D' b7 b6 b5 b4 b3 b2 b1 b0
                case 'D':
                    return _streamReader.ReadDouble();

                default:
                    throw new UnsupportedTagException("double", tag);
            }
        }

        /// <inheritdoc/>
        public override string ReadString()
        {
            var tag = _streamReader.Read();
            switch (tag)
            {
                case 'N':
                    return null;

                // Compact: short strings
                // ::= [x00-x1f] <utf8-data>       # string of length 0-31
#pragma warning disable format
                case 0x00: case 0x01: case 0x02: case 0x03: case 0x04: case 0x05: case 0x06: case 0x07:
                case 0x08: case 0x09: case 0x0a: case 0x0b: case 0x0c: case 0x0d: case 0x0e: case 0x0f:
                case 0x10: case 0x11: case 0x12: case 0x13: case 0x14: case 0x15: case 0x16: case 0x17:
                case 0x18: case 0x19: case 0x1a: case 0x1b: case 0x1c: case 0x1d: case 0x1e: case 0x1f:
#pragma warning restore format
                    return _streamReader.ReadInternedString(tag);

                // ::= [x30-x33] b0 <utf8-data>           # string of length 0-1023
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    var len = ((tag - 0x30) << 8) + _streamReader.Read();
                    return len <= STRING_INTERN_THRESHOLD ? _streamReader.ReadInternedString(len) : _streamReader.ReadString(len);

                // ::= 'S' b1 b0 <utf8-data>              # string of length 0-65535
                case 'S':
                    return _streamReader.ReadString(_streamReader.ReadShort());

                // ::= x52 b1 b0 <utf8-data> string       # non-final chunk
                case 0x52:
                    return ReadMultiPartString();

                default:
                    throw new UnsupportedTagException("string", tag);
            }
        }

        internal override void AddRef(object obj) => _refs.Add(obj);

        internal override bool IsEnd() => _streamReader.Peek() == 'Z';

        internal override void ReadEnd()
        {
            var tag = _streamReader.Read();
            if (tag == 'Z')
                return;

            throw new UnsupportedTagException("end", tag);
        }

        internal override string ReadType()
        {
            var tag = _streamReader.Peek();
            switch (tag)
            {
                // check whether its an integer (ref)
#pragma warning disable format
                case 'I':
                case 0x80: case 0x81: case 0x82: case 0x83: case 0x84: case 0x85: case 0x86: case 0x87:
                case 0x88: case 0x89: case 0x8a: case 0x8b: case 0x8c: case 0x8d: case 0x8e: case 0x8f:
                case 0x90: case 0x91: case 0x92: case 0x93: case 0x94: case 0x95: case 0x96: case 0x97:
                case 0x98: case 0x99: case 0x9a: case 0x9b: case 0x9c: case 0x9d: case 0x9e: case 0x9f:
                case 0xa0: case 0xa1: case 0xa2: case 0xa3: case 0xa4: case 0xa5: case 0xa6: case 0xa7:
                case 0xa8: case 0xa9: case 0xaa: case 0xab: case 0xac: case 0xad: case 0xae: case 0xaf:
                case 0xb0: case 0xb1: case 0xb2: case 0xb3: case 0xb4: case 0xb5: case 0xb6: case 0xb7:
                case 0xb8: case 0xb9: case 0xba: case 0xbb: case 0xbc: case 0xbd: case 0xbe: case 0xbf:
                case 0xc0: case 0xc1: case 0xc2: case 0xc3: case 0xc4: case 0xc5: case 0xc6: case 0xc7:
                case 0xc8: case 0xc9: case 0xca: case 0xcb: case 0xcc: case 0xcd: case 0xce: case 0xcf:
                case 0xd0: case 0xd1: case 0xd2: case 0xd3: case 0xd4: case 0xd5: case 0xd6: case 0xd7:
#pragma warning restore format
                    return _typeRefs[ReadInt()];

                default:
                    var typename = ReadString();
                    _typeRefs.Add(typename);
                    return typename;
            }
        }

        internal override bool ReadListStart(out string typeName, out int length)
        {
            length = -1;
            typeName = null;

            var tag = _streamReader.ReadLast();
            switch (tag)
            {
                // ::= x55 type value* 'Z' # variable-length list
                case 0x55:
                    typeName = ReadType();
                    return true;

                // ::= 'V' type int value* # fixed-length list
                case 'V':
                    typeName = ReadType();
                    length = ReadInt();
                    return false;

                // ::= x57 value* 'Z' # variable-length untyped list
                case 0x57:
                    return true;

                // ::= x58 int value* # fixed-length untyped list
                case 0x58:
                    length = ReadInt();
                    return false;

                // ::= [x70-77] type value* # fixed-length typed list
#pragma warning disable format
                case 0x70: case 0x71: case 0x72: case 0x73: case 0x74: case 0x75: case 0x76: case 0x77:
#pragma warning restore format
                    length = tag - 0x70;
                    typeName = ReadType();
                    return false;

                // ::= [x78-7f] value* # fixed-length untyped list
#pragma warning disable format
                case 0x78: case 0x79: case 0x7a: case 0x7b: case 0x7c: case 0x7d: case 0x7e: case 0x7f:
#pragma warning restore format
                    length = tag - 0x78;
                    return false;

                default:
                    throw new UnsupportedTagException("list", tag);
            }
        }

        internal override bool ReadMapStart(out string typeName, out ClassDefinition typeDef)
        {
            typeName = null;
            typeDef = null;

            var tag = _streamReader.ReadLast();
            switch (tag)
            {
                // ::= 'H' (value value)* 'Z'       # untyped key, value
                case 'H':
                    return true;

                // ::= 'M' type (value value)* 'Z'  # key, value map pairs
                case 'M':
                    typeName = ReadType();
                    return true;

                // ::= 'O' int value*
                case 'O':
                    typeDef = _typeDefinitionRefs[ReadInt()];
                    typeName = typeDef.TypeName;
                    return false;

                // ::= [x60-x6f] value*
#pragma warning disable format
                case 0x60: case 0x61: case 0x62: case 0x63: case 0x64: case 0x65: case 0x66: case 0x67:
                case 0x68: case 0x69: case 0x6a: case 0x6b: case 0x6c: case 0x6d: case 0x6e: case 0x6f:
#pragma warning restore format
                    typeDef = _typeDefinitionRefs[tag - 0x60];
                    typeName = typeDef.TypeName;
                    return false;

                default:
                    throw new UnsupportedTagException("map", tag);
            }
        }

        internal override bool ReadReplyStart()
        {
            // version   ::= 'H' x02 x00
            var tag = _streamReader.Read();
            var major = _streamReader.Read();
            var minor = _streamReader.Read();

            if (tag != 'H' || major != 0x02 || minor != 0x00)
                throw new InvalidOperationException($"Invalid hessian v2 header. Expected ['H', 0x02, 0x00] but was ['{(char)tag}', 0x{major:X2}, 0x{minor:X2}]");

            // read actual reply
            tag = _streamReader.Read();
            switch (tag)
            {
                // reply       ::= R value
                case 'R':
                    return false;

                // fault       ::= F map
                case 'F':
                    return true;

                default:
                    throw new UnsupportedTagException("reply", tag);
            }
        }

        internal override HessianRemoteException ReadFault()
        {
            var dict = (IDictionary<object, object>)ReadObject(typeof(Dictionary<object, object>));
            return HessianRemoteException.FromRawMap(dict);
        }

        internal override void ReadReplyEnd()
        {
        }

        private byte[] ReadRawBytes()
        {
            var len = _streamReader.ReadShort();
            var buf = new byte[len];
            _streamReader.Read(buf);
            return buf;
        }

        private string ReadMultiPartString()
        {
            var strBuilder = new StringBuilder(_streamReader.ReadString(_streamReader.ReadShort()));
            var isEnd = false;
            while (!isEnd)
            {
                var tag = _streamReader.Read();
                switch (tag)
                {
                    // Compact: short strings
                    // ::= [x00-x1f] <utf8-data>       # string of length 0-31
#pragma warning disable format
                    case 0x00: case 0x01: case 0x02: case 0x03: case 0x04: case 0x05: case 0x06: case 0x07:
                    case 0x08: case 0x09: case 0x0a: case 0x0b: case 0x0c: case 0x0d: case 0x0e: case 0x0f:
                    case 0x10: case 0x11: case 0x12: case 0x13: case 0x14: case 0x15: case 0x16: case 0x17:
                    case 0x18: case 0x19: case 0x1a: case 0x1b: case 0x1c: case 0x1d: case 0x1e: case 0x1f:
#pragma warning restore format
                        strBuilder.Append(_streamReader.ReadString(tag));
                        isEnd = true;
                        break;

                    // ::= [x30-x33] b0 <utf8-data>           # string of length 0-1023
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                        var len = ((tag - 0x30) << 8) + _streamReader.Read();
                        strBuilder.Append(_streamReader.ReadString(len));
                        isEnd = true;
                        break;

                    // ::= 'S' b1 b0 <utf8-data>              # string of length 0-65535
                    case 'S':
                        strBuilder.Append(_streamReader.ReadString(_streamReader.ReadShort()));
                        isEnd = true;
                        break;

                    // ::= x52 b1 b0 <utf8-data> string       # non-final chunk
                    case 0x52:
                        strBuilder.Append(_streamReader.ReadString(_streamReader.ReadShort()));
                        break;

                    default:
                        throw new UnsupportedTagException("multiPartString", tag);
                }
            }
            return strBuilder.ToString();
        }
    }
}