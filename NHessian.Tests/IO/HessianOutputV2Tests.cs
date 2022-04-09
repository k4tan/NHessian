using example;
using NHessian.IO;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NHessian.Tests.IO
{
    [TestFixture]
    public class HessianOutputV2Tests
    {
        [Test]
        public void Binary_Chunks()
        {
            // can be up to 1023 bytes
            var chunk1 = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();
            var chunk2 = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();
            var chunk3 = Enumerable.Range(0, 1).Select(i => (byte)i).ToArray();
            var actual = Serialize(chunk1.Concat(chunk2).Concat(chunk3).ToArray());

            var expected = new HessianDataBuilder()
                .WriteBytes(0x41).WriteBytes(0xff, 0xff).WriteBytes(chunk1)
                .WriteBytes(0x41).WriteBytes(0xff, 0xff).WriteBytes(chunk2)
                .WriteBytes(0x21).WriteBytes(chunk3)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Binary_Long()
        {
            // can be up to 1023 bytes
            var value = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('B').WriteBytes(0xff, 0xff).WriteBytes(value)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Binary_Medium()
        {
            // can be up to 1023 bytes
            var value = Enumerable.Range(0, 1023).Select(i => (byte)i).ToArray();
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x37, 0xff).WriteBytes(value)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(new byte[0], new byte[] { 0x20 })]
        [TestCase(new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x23, 0x01, 0x02, 0x03 })]
        public void Binary_Short(byte[] value, byte[] expected)
        {
            // can be up to 15 bytes
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void BooleanFalse()
        {
            var value = false;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                    .WriteChar('F')
                    .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void BooleanTrue()
        {
            var value = true;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('T')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Call()
        {
            byte[] actual;
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                new HessianOutputV2(writer).WriteCall("add2", new object[] { 2, 3 });
                actual = ms.ToArray();
            }

            var expected = new HessianDataBuilder()
                .WriteChar('H').WriteBytes(0x02, 0x00) // version
                .WriteChar('C') // call
                .WriteBytes(0x04).WriteUtf8("add2") // method
                .WriteBytes(0x92) // 2 arguments
                .WriteBytes(0x92) // arg1
                .WriteBytes(0x93) // arg2
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Date()
        {
            var value = new DateTime(1998, 5, 8, 9, 51, 31, DateTimeKind.Utc);
            var actual = Serialize(value);

            // 09:51:31 May 8, 1998 UTC
            // 894621091000 ms since epoch
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4a).WriteBytes(0x00, 0x00, 0x00, 0xd0, 0x4b, 0x92, 0x84, 0xb8)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Date_MinValue()
        {
            var value = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            // force long format by specifying seconds
            value = value.AddSeconds(1);

            var actual = Serialize(value);

            // 00:00:01 Jan 1, 0001 UTC
            // -62135596800000 ms since epoch
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4a).WriteBytes(0xff, 0xff, 0xc7, 0x7c, 0xed, 0xd3, 0x2b, 0xe8)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Date_MaxValue()
        {
            // hessian only supports millisecond precision
            var value = DateTime.SpecifyKind(DateTime.Parse(DateTime.MaxValue.ToString("yyyy-MM-ddTH:mm:ss.fff")), DateTimeKind.Utc);

            var actual = Serialize(value);

            // 23:59:59.999 Dec 31, 9999 UTC
            // 253402300799999 ms since epoch
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4a).WriteBytes(0x00, 0x00, 0xe6, 0x77, 0xd2, 0x1f, 0xdb, 0xff)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DateShort()
        {
            // short format should be used if no minute granularity is used
            var value = new DateTime(1998, 5, 8, 9, 51, 0, DateTimeKind.Utc);
            var actual = Serialize(value);

            // x4b x00 xe3 x83 x8f  # 09:51:00 May 8, 1998 UTC
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4b).WriteBytes(0, 0xe3, 0x83, 0x8f)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DateShort_MinValue()
        {
            // Although short date is limited by Int32, DateTime.MinValue
            // is small enough to not underflow. So short date can be used
            // all the way down to DateTime.MinValue
            var value = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            var actual = Serialize(value);

            // 00:00:00 Jan 1, 0001 UTC
            // -1035593280 minutes since epoch
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4b).WriteBytes(0xc2, 0x46, 0x19, 0xc0)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DateShort_MaxValue()
        {
            // short format is limited by Int32. So the max date that can
            // be sent as short_date is Int32.MaxValue since epoch
            var value = DateTime.UnixEpoch.AddMinutes(int.MaxValue);
            var actual = Serialize(value);

            // 02:07:00 Jan 23, 6053 UTC
            // 2147483647 minutes since epoch (Int.Max)
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4b).WriteBytes(0x7f, 0xff, 0xff, 0xff)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void DateShort_Overflow()
        {
            // short format is limited by Int32. On overflow, we should
            // fallback to the long version
            var value = DateTime.UnixEpoch.AddMinutes(((long)int.MaxValue) + 1);
            var actual = Serialize(value);

            // 02:08:00 Jan 23, 6053 UTC
            // 128849018880000 ms since epoch (Int.MaxValue minutes + 1)
            var expected = new HessianDataBuilder()
                .WriteBytes(0x4a).WriteBytes(0x00, 0x00, 0x75, 0x30, 0x00, 0x00, 0x00, 0x00)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Double()
        {
            var value = 10e50d;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('D').WriteBytes(0x4a, 0x85, 0x61, 0xd2, 0x76, 0xdd, 0xfd, 0xc0)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test, Ignore("Java is doing some weird stuff with float representation. Doesn't seem to be spec...")]
        public void Double_Float()
        {
            //var value = 12.25d;
            //var actual = Serialize(value);

            //var expected = new HessianDataBuilder()
            //    .WriteChar('D').WriteBytes(0x40, 0x28, 0x80, 0, 0, 0, 0, 0)
            //    .ToArray();

            //CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(-128.0d, new byte[] { 0x5d, 0x80 })]
        [TestCase(127.0d, new byte[] { 0x5d, 0x7f })]
        public void Double_Octet(double value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Double_One()
        {
            var value = 1.0d;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x5c)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(-32768.0d, new byte[] { 0x5e, 0x80, 0x00 })]
        [TestCase(32767.0d, new byte[] { 0x5e, 0x7f, 0xff })]
        public void Double_Short(double value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Double_Zero()
        {
            var value = 0.0d;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x5b)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(268435456, new byte[] { (byte)'I', 0x10, 0, 0, 0 })]
        public void Int_FourOctet(int value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(0, new byte[] { 0x90 })]
        [TestCase(-16, new byte[] { 0x80 })]
        [TestCase(47, new byte[] { 0xbf })]
        public void Int_OneOctet(int value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(-262144, new byte[] { 0xd0, 0, 0 })]
        [TestCase(262143, new byte[] { 0xd7, 0xff, 0xff })]
        public void Int_ThreeOctet(int value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(48, new byte[] { 0xc8, 0x30 })]
        [TestCase(-2048, new byte[] { 0xc0, 0 })]
        [TestCase(-256, new byte[] { 0xc7, 0 })]
        [TestCase(2047, new byte[] { 0xcf, 0xff })]
        public void Int_TwoOctet(int value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Compact_Fixed()
        {
            var value = new int[] { 0, 1 };
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x72) // list tag (length 2)
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Compact_Fixed_UsingTypeRef()
        {
            var first = new int[] { 0, 1 };
            var second = new int[] { 2, 3, 4 };

            var expected = new HessianDataBuilder()
                // first list
                .WriteBytes(0x72) // list tag
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                                  // second list
                .WriteBytes(0x73) // list tag
                .WriteBytes(0x90) // type ref 0 (above [int)
                .WriteBytes(0x92) // int 2
                .WriteBytes(0x93) // int 3
                .WriteBytes(0x94) // int 4
                .ToArray();

            byte[] actual;
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                var output = new HessianOutputV2(writer, TypeBindings.Java);
                output.WriteObject(first);
                output.WriteObject(second);
                actual = ms.ToArray();
            }

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Compact_Untyped_Fixed()
        {
            var value = new FixedIntList { 0, 1 };
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x7A) // list tag (length 2)
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Fixed()
        {
            var value = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('V') // list tag
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x98) // length 8 (must be longer than 7 to avoid compact)
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteBytes(0x92) // int 2
                .WriteBytes(0x93) // int 3
                .WriteBytes(0x94) // int 4
                .WriteBytes(0x95) // int 5
                .WriteBytes(0x96) // int 6
                .WriteBytes(0x97) // int 7
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Untyped_Fixed()
        {
            var value = new FixedIntList { 0, 1, 2, 3, 4, 5, 6, 7 };
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x58) // list tag
                .WriteBytes(0x98) // length 8 (must be longer than 7 to avoid compact)
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteBytes(0x92) // int 2
                .WriteBytes(0x93) // int 3
                .WriteBytes(0x94) // int 4
                .WriteBytes(0x95) // int 5
                .WriteBytes(0x96) // int 6
                .WriteBytes(0x97) // int 7
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Untyped_Variable()
        {
            var value = Enumerable.Empty<int>().Concat(new int[] { 0, 1 });
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x57) // list tag
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteChar('Z') // end
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_Variable()
        {
            var value = new VariableIntList(new int[] { 0, 1 });
            var actual = Serialize(value, new CustomListBinding());

            var expected = new HessianDataBuilder()
                .WriteBytes(0x55) // list tag
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteChar('Z') // end
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(1152921504606846976L, new byte[] { (byte)'L', 0x10, 0, 0, 0, 0, 0, 0, 0 })]
        public void Long_EightOctet(long value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(268435456L, new byte[] { 0x59, 0x10, 0, 0, 0 })]
        public void Long_FourOctet(long value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(0L, new byte[] { 0xe0 })]
        [TestCase(-8L, new byte[] { 0xd8 })]
        [TestCase(15L, new byte[] { 0xef })]
        public void Long_OneOctet(long value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(-262144L, new byte[] { 0x38, 0, 0 })]
        [TestCase(262143L, new byte[] { 0x3f, 0xff, 0xff })]
        public void Long_ThreeOctet(long value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(-2048L, new byte[] { 0xf0, 0 })]
        [TestCase(-256L, new byte[] { 0xf7, 0 })]
        [TestCase(2047L, new byte[] { 0xff, 0xff })]
        public void Long_TwoOctet(long value, byte[] expected)
        {
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_CompactEnum()
        {
            var first = Color.RED;
            var second = Color.GREEN;
            var third = Color.BLUE;
            var fourth = Color.GREEN;

            byte[] actual;
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                var output = new HessianOutputV2(writer);

                output.WriteObject(first);
                output.WriteObject(second);
                output.WriteObject(third);
                output.WriteObject(fourth);

                actual = ms.ToArray();
            }

            var expected = new HessianDataBuilder()
                .WriteChar('C')
                .WriteBytes(0x0d).WriteUtf8("example.Color")
                .WriteBytes(0x91) // one field
                .WriteBytes(0x04).WriteUtf8("name")

                .WriteBytes(0x60) // reference definition
                .WriteBytes(0x03).WriteUtf8("RED")

                .WriteBytes(0x60)
                .WriteBytes(0x05).WriteUtf8("GREEN")

                .WriteBytes(0x60)
                .WriteBytes(0x04).WriteUtf8("BLUE")

                .WriteChar('Q').WriteBytes(0x91) //  GREEN

                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_CompactObject_1()
        {
            // car example is taken from hessian spec wesite
            var value = new com.caucho.test.Car
            {
                model = "Beetle",
                color = "aquamarine",
                mileage = 268435456
            };

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('C') // definition
                .WriteBytes(0x13).WriteUtf8("com.caucho.test.Car")
                .WriteBytes(0x93) // 3 fields
                .WriteBytes(0x05).WriteUtf8("model")
                .WriteBytes(0x05).WriteUtf8("color")
                .WriteBytes(0x07).WriteUtf8("mileage")

                .WriteBytes(0x60) // instance
                .WriteBytes(0x06).WriteUtf8("Beetle")
                .WriteBytes(0x0a).WriteUtf8("aquamarine")
                .WriteChar('I').WriteBytes(0x10, 0, 0, 0)

                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_CompactObject_2()
        {
            // test several different field types
            var value = new Stubs.TestClass("string");

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('C') // definition
                .WriteBytes(0x30, 0x21).WriteUtf8("NHessian.Tests.IO.Stubs.TestClass")
                .WriteBytes(0x94) // 4 fields
                .WriteBytes(0x09).WriteUtf8("publicStr")
                .WriteBytes(0x0c).WriteUtf8("protectedStr")
                .WriteBytes(0x0a).WriteUtf8("privateStr")
                .WriteBytes(0x0b).WriteUtf8("readonlyStr")

                .WriteBytes(0x60) // instance
                .WriteBytes(0x06).WriteUtf8("string")
                .WriteBytes(0x06).WriteUtf8("string")
                .WriteBytes(0x06).WriteUtf8("string")
                .WriteBytes(0x06).WriteUtf8("string")

                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_SparseArray()
        {
            var value = new Dictionary<object, object>
            {
                { 1, "fee" },
                { 16, "fie" },
                { 256, "foe" }
            };

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('H') // untyped map

                .WriteBytes(0x91)                  // 1
                .WriteBytes(0x03).WriteUtf8("fee") // "fee"

                .WriteBytes(0xa0)                  // 16
                .WriteBytes(0x03).WriteUtf8("fie") // "fie"

                .WriteBytes(0xc9, 0x00)               // 256
                .WriteBytes(0x03).WriteUtf8("foe") // foe

                .WriteChar('Z') // map end
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Null()
        {
            object value = null;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('N')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_Chunks()
        {
            // can be up to 65535 chars
            var chunk1 = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));
            var chunk2 = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));
            var chunk3 = string.Join("", Enumerable.Range(0, 1).Select(i => (i % 10).ToString()));
            var actual = Serialize(chunk1 + chunk2 + chunk3);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x52).WriteBytes(0xff, 0xff).WriteUtf8(chunk1)
                .WriteBytes(0x52).WriteBytes(0xff, 0xff).WriteUtf8(chunk2)
                .WriteBytes(0x01).WriteUtf8(chunk3)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_Long()
        {
            // can be up to 65535 chars
            var value = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                    .WriteChar('S').WriteBytes(0xff, 0xff).WriteUtf8(value)
                    .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_Medium()
        {
            // can be up to 1023 chars
            var value = string.Join("", Enumerable.Range(0, 1023).Select(i => (i % 10).ToString()));
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteBytes(0x33, 0xff).WriteUtf8(value)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase("", new byte[] { 0 })]
        [TestCase("hello", new byte[] { 0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [TestCase("\u00C3", new byte[] { 0x01, 0xc3, 0x83 })]
        [TestCase("\uFFAA", new byte[] { 0x01, 0xef, 0xbe, 0xaa })]
        public void String_Short(string value, byte[] expected)
        {
            // can be up to 31 chars
            var actual = Serialize(value);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_Surrogate()
        {
            // 𠀀 - Can be represented as:
            //    Unicode         U+20000         (4-byte UTF8)
            //    Surrogate Pair  U+D840, U+DC00  (2x 3-byte UTF8)
            var str = "\ud840\udc00";

            var actual = Serialize(str);

            // use surrogate pair
            var expected = new HessianDataBuilder()
                .WriteBytes(0x02).WriteBytes(0xED, 0xA1, 0x80, 0xED, 0xB0, 0x80)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        private static byte[] Serialize(object obj, TypeBindings typeBindings = null)
        {
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                new HessianOutputV2(writer, typeBindings ?? TypeBindings.Java).WriteObject(obj);

                return ms.ToArray();
            }
        }

        private class CustomListBinding : TypeBindings
        {
            public override Type TypeStringToType(string typeString) => null;

            public override string TypeToTypeString(Type type)
            {
                if (type == typeof(VariableIntList))
                    return "[int";
                return null;
            }
        }

        private class FixedIntList : List<int> { }

        private class VariableIntList : IEnumerable<int>
        {
            private IReadOnlyList<int> _innerList;

            public VariableIntList(IReadOnlyList<int> innerList) => _innerList = innerList;

            public IEnumerator<int> GetEnumerator() => _innerList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();
        }
    }
}