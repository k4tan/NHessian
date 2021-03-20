using example;
using NHessian.IO;
using NUnit.Framework;
using qa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NHessian.Tests.IO
{
    [TestFixture]
    public class HessianOutputV1Tests
    {
        [Test]
        public void Binary_MultiChunk()
        {
            // length must be larger than 16bit in order to get multi-chunk
            // provoke 3 chunks in this test
            const int maxChunkLength = 65535;
            var strLength = 3 * maxChunkLength;
            var byteArr = Enumerable.Range(0, strLength).Select(i => (byte)(i % 10)).ToArray();

            var actual = Serialize(byteArr);

            var expected = new HessianDataBuilder()
                .WriteChar('b').WriteBytes(0xFF, 0xFF).WriteBytes(byteArr.Take(maxChunkLength).ToArray())
                .WriteChar('b').WriteBytes(0xFF, 0xFF).WriteBytes(byteArr.Skip(maxChunkLength).Take(maxChunkLength).ToArray())
                .WriteChar('B').WriteBytes(0xFF, 0xFF).WriteBytes(byteArr.Skip(2 * maxChunkLength).Take(maxChunkLength).ToArray())
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Binary_SingleChunk()
        {
            var byteArr = new byte[] { 0x28 };
            var actual = Serialize(byteArr);

            var expected = new HessianDataBuilder()
                .WriteChar('B').WriteBytes(0, 0x01).WriteBytes(byteArr)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void BooleanFalse()
        {
            var actual = Serialize(false);

            var expected = new HessianDataBuilder()
                .WriteChar('F')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void BooleanTrue()
        {
            var actual = Serialize(true);

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
                new HessianOutputV1(writer).WriteCall("add2", new object[] { 2, 3 });

                actual = ms.ToArray();
            }

            var expected = new HessianDataBuilder()
                // call
                .WriteChar('c')
                // version (major version 1, minor 0)
                .WriteBytes(1, 0)
                // method name
                .WriteChar('m').WriteBytes(0, 0x04).WriteUtf8("add2")
                // arg1
                .WriteChar('I').WriteBytes(0, 0, 0, 0x02)
                // arg2
                .WriteChar('I').WriteBytes(0, 0, 0, 0x03)
                // call end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Call_WithHeader()
        {
            var header = new Tuple<string, object>("transaction", 0);

            byte[] actual;
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                new HessianOutputV1(writer).WriteCall("debug", new object[] { 197067 }, new[] { header });
                actual = ms.ToArray();
            }

            var expected = new HessianDataBuilder()
                // call
                .WriteChar('c')
                // version (major version 1, minor 0)
                .WriteBytes(1, 0)
                // header(s)
                .WriteChar('H')
                    .WriteBytes(0, 0x0B).WriteUtf8("transaction")
                    .WriteChar('I').WriteBytes(0, 0, 0, 0)
                // method name
                .WriteChar('m').WriteBytes(0, 0x05).WriteUtf8("debug")
                // arg1
                .WriteChar('I').WriteBytes(0, 0x03, 0x01, 0xCB)
                // call end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Call_WithRef()
        {
            var bean = new Bean { foo = 13 };

            byte[] actual;
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                new HessianOutputV1(writer).WriteCall("eq", new object[] { bean, bean });
                actual = ms.ToArray();
            }

            var expected = new HessianDataBuilder()
                // call
                .WriteChar('c')
                // version (major version 1, minor 0)
                .WriteBytes(1, 0)
                // method name
                .WriteChar('m').WriteBytes(0, 0x02).WriteUtf8("eq")
                // arg1
                .WriteChar('M')
                    .WriteChar('t').WriteBytes(0, 0x07).WriteUtf8("qa.Bean")
                    // field foo
                    .WriteChar('S').WriteBytes(0, 0x03).WriteUtf8("foo")
                    .WriteChar('I').WriteBytes(0, 0, 0, 0x0D)
                    // end of map
                    .WriteChar('z')
                // arg2: ref to arg 1
                .WriteChar('R').WriteBytes(0, 0, 0, 0)
                // call end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Date()
        {
            var date = new DateTime(1998, 5, 8, 9, 51, 31, DateTimeKind.Utc);
            var actual = Serialize(date);

            // 9:51:31 May 8, 1998
            var expected = new HessianDataBuilder()
                .WriteChar('d').WriteBytes(0, 0, 0, 0xD0, 0x4B, 0x92, 0x84, 0xB8)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Date_MinValue()
        {
            var value = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

            var actual = Serialize(value);

            // 00:00:00 Jan 1, 0001 UTC
            // -62135596800000 ms since epoch
            var expected = new HessianDataBuilder()
                .WriteChar('d').WriteBytes(0xff, 0xff, 0xc7, 0x7c, 0xed, 0xd3, 0x28, 0x00)
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
                .WriteChar('d').WriteBytes(0x00, 0x00, 0xe6, 0x77, 0xd2, 0x1f, 0xdb, 0xff)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Double12_25()
        {
            var actual = Serialize(12.25);
            // treat float as double
            var actualFloat = Serialize(12.25);

            var expected = new HessianDataBuilder()
                .WriteChar('D').WriteBytes(0x40, 0x28, 0x80, 0, 0, 0, 0, 0)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
            CollectionAssert.AreEqual(expected, actualFloat);
        }

        [Test]
        public void Enum()
        {
            var value = Color.GREEN;
            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                .WriteChar('M')
                .WriteChar('t').WriteBytes(0, 0x0d).WriteUtf8("example.Color")
                .WriteChar('S').WriteBytes(0, 0x04).WriteUtf8("name")
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8("GREEN")
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Int300()
        {
            var actual = Serialize(300);
            // treat char as int
            var actualChar = Serialize((char)300);

            var expected = new HessianDataBuilder()
                .WriteChar('I').WriteBytes(0, 0, 0x01, 0x2C)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
            CollectionAssert.AreEqual(expected, actualChar);
        }

        [Test]
        public void List_NonTypedVariableLength()
        {
            // use enumeration (concat) to avoid length in serialization
            var value = new object[] { 0 }.Concat(new object[] { "foobar" });

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                // list
                .WriteChar('V')
                // item1 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0)
                // item2 (string)
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("foobar")
                // list end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void List_TypedFixedLength()
        {
            var actualArr = Serialize(new[] { 0, 1 });

            var expected = new HessianDataBuilder()
                // list
                .WriteChar('V')
                // type (from the website example)
                .WriteChar('t').WriteBytes(0, 0x04).WriteUtf8("[int")
                // length of list
                .WriteChar('l').WriteBytes(0, 0, 0, 0x02)
                // item1 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0)
                // item2 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0x01)
                // list end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actualArr);
        }

        [Test]
        public void List_TypedFixedLength_WithCustomType()
        {
            // treat list same as array
            var actualList = Serialize(new List<int> { 0, 1 }, new CustomListBinding());

            var expected = new HessianDataBuilder()
                // list
                .WriteChar('V')
                // type (from the website example)
                .WriteChar('t').WriteBytes(0, 0x11).WriteUtf8("CustomIntListType")
                // length of list
                .WriteChar('l').WriteBytes(0, 0, 0, 0x02)
                // item1 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0)
                // item2 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0x01)
                // list end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actualList);
        }

        [Test]
        public void Long300()
        {
            var actual = Serialize((long)300);

            var expected = new HessianDataBuilder()
                .WriteChar('L').WriteBytes(0, 0, 0, 0, 0, 0, 0x01, 0x2C)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_Object_1()
        {
            // car example is taken from hessian spec wesite
            var value = new com.caucho.test.Car
            {
                model = "Beetle",
                color = "aquamarine",
                mileage = 65536
            };

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                // map
                .WriteChar('M')
                // type (from the website example)
                .WriteChar('t').WriteBytes(0, 0x13).WriteUtf8("com.caucho.test.Car")
                // model field
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8("model")
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("Beetle")
                // color field
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8("color")
                .WriteChar('S').WriteBytes(0, 0x0A).WriteUtf8("aquamarine")
                // mileage field
                .WriteChar('S').WriteBytes(0, 0x07).WriteUtf8("mileage")
                .WriteChar('I').WriteBytes(0, 0x01, 0, 0)
                // map end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Map_Object_2()
        {
            // test several different field types
            var value = new Stubs.TestClass("string");

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                // map
                .WriteChar('M')
                // type
                .WriteChar('t').WriteBytes(0, 0x21).WriteUtf8("NHessian.Tests.IO.Stubs.TestClass")
                // publicStr
                .WriteChar('S').WriteBytes(0, 0x09).WriteUtf8("publicStr")
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("string")
                // protectedStr
                .WriteChar('S').WriteBytes(0, 0x0C).WriteUtf8("protectedStr")
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("string")
                // privateStr
                .WriteChar('S').WriteBytes(0, 0x0A).WriteUtf8("privateStr")
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("string")
                // readonlyStr
                .WriteChar('S').WriteBytes(0, 0x0B).WriteUtf8("readonlyStr")
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("string")
                // map end
                .WriteChar('z')
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
                // map
                .WriteChar('M')
                // key/value 1
                .WriteChar('I').WriteBytes(0, 0, 0, 0x01)
                .WriteChar('S').WriteBytes(0, 0x03).WriteUtf8("fee")
                // key/value 2
                .WriteChar('I').WriteBytes(0, 0, 0, 0x10)
                .WriteChar('S').WriteBytes(0, 0x03).WriteUtf8("fie")
                // key/value 3
                .WriteChar('I').WriteBytes(0, 0, 0x01, 0)
                .WriteChar('S').WriteBytes(0, 0x03).WriteUtf8("foe")
                // map end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Null()
        {
            var actual = Serialize(null);

            var expected = new HessianDataBuilder()
                .WriteChar('N')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void Ref_CircularList()
        {
            var value = new com.caucho.test.LinkedList { head = 1 };
            value.tail = value;

            var actual = Serialize(value);

            var expected = new HessianDataBuilder()
                // map
                .WriteChar('M')
                // type
                .WriteChar('t').WriteBytes(0, 0x1A).WriteUtf8("com.caucho.test.LinkedList")
                // head field
                .WriteChar('S').WriteBytes(0, 0x04).WriteUtf8("head")
                .WriteChar('I').WriteBytes(0, 0, 0, 0x01)
                // tail field
                .WriteChar('S').WriteBytes(0, 0x04).WriteUtf8("tail")
                .WriteChar('R').WriteBytes(0, 0, 0, 0)
                // map end
                .WriteChar('z')
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_MultiByteUnicode()
        {
            var str = "세미";
            var actual = Serialize(str);

            var expected = new HessianDataBuilder()
                .WriteChar('S').WriteBytes(0, 0x02).WriteUtf8(str)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_MultiChunk()
        {
            // we need to provoke string length larger than 16bit
            // in order to get multi-chunk
            // provoke 3 chunks in this test
            const int maxChunkLength = 65535;
            var strLength = 3 * maxChunkLength;
            var str = string.Join("", Enumerable.Range(0, strLength).Select(i => (i % 10).ToString()));

            var actual = Serialize(str);

            Assert.AreEqual(strLength, str.Length);

            var expected = new HessianDataBuilder()
                .WriteChar('s').WriteBytes(0xFF, 0xFF).WriteUtf8(str.Substring(0, maxChunkLength))
                .WriteChar('s').WriteBytes(0xFF, 0xFF).WriteUtf8(str.Substring(maxChunkLength, maxChunkLength))
                .WriteChar('S').WriteBytes(0xFF, 0xFF).WriteUtf8(str.Substring(2 * maxChunkLength, maxChunkLength))
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void String_SingleByteUnicode()
        {
            var str = "hello";
            var actual = Serialize(str);

            var expected = new HessianDataBuilder()
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8(str)
                .ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        private static byte[] Serialize(object obj, TypeBindings bindings = null)
        {
            using (var ms = new MemoryStream())
            using (var writer = new HessianStreamWriter(ms))
            {
                new HessianOutputV1(writer, bindings ?? TypeBindings.Java).WriteObject(obj);

                return ms.ToArray();
            }
        }

        private class CustomListBinding : TypeBindings
        {
            public override Type TypeStringToType(string typeString) => null;

            public override string TypeToTypeString(Type type)
            {
                if (type == typeof(List<int>))
                    return "CustomIntListType";

                return null;
            }
        }
    }
}