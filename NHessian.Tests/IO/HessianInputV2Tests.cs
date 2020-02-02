﻿using example;
using NHessian.IO;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NHessian.Tests.IO
{
    [TestFixture]
    public class HessianInputV2Tests
    {
        [Test]
        public void Null()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('N')
                .ToReader();

            Assert.Null(new HessianInputV2(reader).ReadObject());
        }

        [Test]
        public void BooleanTrue()
        {
            var data = new HessianDataBuilder()
                .WriteChar('T');

            Assert.True((bool)new HessianInputV2(data.ToReader()).ReadObject());
            Assert.True(new HessianInputV2(data.ToReader()).ReadBool());
        }

        [Test]
        public void BooleanFalse()
        {
            var data = new HessianDataBuilder()
                .WriteChar('F');

            Assert.False((bool)new HessianInputV2(data.ToReader()).ReadObject());
            Assert.False(new HessianInputV2(data.ToReader()).ReadBool());
        }

        [Test]
        public void Date()
        {
            // 9:51:31 May 8, 1998
            var data = new HessianDataBuilder()
                .WriteBytes(0x4a).WriteBytes(0, 0, 0, 0xd0, 0x4b, 0x92, 0x84, 0xb8);

            var expected = new DateTime(1998, 5, 8, 9, 51, 31, DateTimeKind.Utc).ToLocalTime();

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDate());
        }

        [Test]
        public void DateShort()
        {
            // short format should be used if no minute granularity is used

            // x4b x00 xe3 x83 x8f  # 09:51:00 May 8, 1998 UTC
            var data = new HessianDataBuilder()
                .WriteBytes(0x4b).WriteBytes(0, 0xe3, 0x83, 0x8f);

            var expected = new DateTime(1998, 5, 8, 9, 51, 0, DateTimeKind.Utc).ToLocalTime();

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDate());
        }

        [TestCase(0, new byte[] { 0x90 })]
        [TestCase(-16, new byte[] { 0x80 })]
        [TestCase(47, new byte[] { 0xbf })]
        public void Int_OneOctet(int expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadInt());
        }

        [TestCase(48, new byte[] { 0xc8, 0x30 })]
        [TestCase(-2048, new byte[] { 0xc0, 0 })]
        [TestCase(-256, new byte[] { 0xc7, 0 })]
        [TestCase(2047, new byte[] { 0xcf, 0xff })]
        public void Int_TwoOctet(int expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadInt());
        }

        [TestCase(-262144, new byte[] { 0xd0, 0, 0 })]
        [TestCase(262143, new byte[] { 0xd7, 0xff, 0xff })]
        public void Int_ThreeOctet(int expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadInt());
        }

        [TestCase(268435456, new byte[] { (byte)'I', 0x10, 0, 0, 0 })]
        public void Int_FourOctet(int expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadInt());
        }

        [TestCase(0L, new byte[] { 0xe0 })]
        [TestCase(-8L, new byte[] { 0xd8 })]
        [TestCase(15L, new byte[] { 0xef })]
        public void Long_OneOctet(long expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadLong());
        }

        [TestCase(-2048L, new byte[] { 0xf0, 0 })]
        [TestCase(-256L, new byte[] { 0xf7, 0 })]
        [TestCase(2047L, new byte[] { 0xff, 0xff })]
        public void Long_TwoOctet(long expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadLong());
        }

        [TestCase(-262144L, new byte[] { 0x38, 0, 0 })]
        [TestCase(262143L, new byte[] { 0x3f, 0xff, 0xff })]
        public void Long_ThreeOctet(long expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadLong());
        }

        [TestCase(268435456L, new byte[] { 0x59, 0x10, 0, 0, 0 })]
        public void Long_FourOctet(long expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadLong());
        }

        [TestCase(1152921504606846976L, new byte[] { (byte)'L', 0x10, 0, 0, 0, 0, 0, 0, 0 })]
        public void Long_EightOctet(long expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadLong());
        }

        [Test]
        public void Double_Zero()
        {
            var expected = 0.0;

            var data = new HessianDataBuilder().WriteBytes(0x5b);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [Test]
        public void Double_One()
        {
            var expected = 1.0;

            var data = new HessianDataBuilder().WriteBytes(0x5c);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [TestCase(-128.0d, new byte[] { 0x5d, 0x80 })]
        [TestCase(127.0d, new byte[] { 0x5d, 0x7f })]
        public void Double_Octet(double expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [TestCase(-32768.0d, new byte[] { 0x5e, 0x80, 0x00 })]
        [TestCase(32767.0d, new byte[] { 0x5e, 0x7f, 0xff })]
        public void Double_Short(double expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [Test]
        public void Double_Float()
        {
            var expected = 12.25d;

            var data = new HessianDataBuilder()
                .WriteChar('D').WriteBytes(0x40, 0x28, 0x80, 0, 0, 0, 0, 0);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [Test]
        public void Double()
        {
            var expected = 10e50d;

            var data = new HessianDataBuilder()
                .WriteChar('D').WriteBytes(0x4a, 0x85, 0x61, 0xd2, 0x76, 0xdd, 0xfd, 0xc0);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadDouble());
        }

        [TestCase("", new byte[] { 0 })]
        [TestCase("hello", new byte[] { 0x05, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' })]
        [TestCase("\u00C3", new byte[] { 0x01, 0xc3, 0x83 })]
        public void String_Short(string expected, byte[] bytes)
        {
            // can be up to 31 chars
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadString());
        }

        [Test]
        public void String_Medium()
        {
            // can be up to 1023 chars
            var expected = string.Join("", Enumerable.Range(0, 1023).Select(i => (i % 10).ToString()));

            var data = new HessianDataBuilder()
                .WriteBytes(0x33, 0xff).WriteUtf8(expected);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadString());
        }

        [Test]
        public void String_Long()
        {
            // can be up to 65535 chars
            var expected = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));

            var data = new HessianDataBuilder()
                .WriteChar('S').WriteBytes(0xff, 0xff).WriteUtf8(expected);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadString());
        }

        [Test]
        public void String_Chunks()
        {
            // can be up to 65535 chars
            var chunk1 = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));
            var chunk2 = string.Join("", Enumerable.Range(0, 65535).Select(i => (i % 10).ToString()));
            var chunk3 = string.Join("", Enumerable.Range(0, 1).Select(i => (i % 10).ToString()));
            var expected = chunk1 + chunk2 + chunk3;

            var data = new HessianDataBuilder()
                .WriteBytes(0x52).WriteBytes(0xff, 0xff).WriteUtf8(chunk1)
                .WriteBytes(0x52).WriteBytes(0xff, 0xff).WriteUtf8(chunk2)
                .WriteBytes(0x01).WriteUtf8(chunk3);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadString());
        }

        [TestCase(new byte[0], new byte[] { 0x20 })]
        [TestCase(new byte[] { 0x01, 0x02, 0x03 }, new byte[] { 0x23, 0x01, 0x02, 0x03 })]
        public void Binary_Short(byte[] expected, byte[] bytes)
        {
            var data = new HessianDataBuilder().WriteBytes(bytes);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
        }

        [Test]
        public void Binary_Medium()
        {
            // can be up to 1023 bytes
            var expected = Enumerable.Range(0, 1023).Select(i => (byte)i).ToArray();

            var data = new HessianDataBuilder()
                .WriteBytes(0x37, 0xff).WriteBytes(expected);

            Assert.AreEqual(expected, new HessianInputV2(data.ToReader()).ReadObject());
        }

        [Test]
        public void Binary_Long()
        {
            // can be up to 1023 bytes
            var expected = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();

            var data = new HessianDataBuilder()
                .WriteChar('B').WriteBytes(0xff, 0xff).WriteBytes(expected);

            CollectionAssert.AreEqual(expected, (byte[])new HessianInputV2(data.ToReader()).ReadObject());
        }

        [Test]
        public void Binary_Chunks()
        {
            // can be up to 1023 bytes
            var chunk1 = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();
            var chunk2 = Enumerable.Range(0, 65535).Select(i => (byte)i).ToArray();
            var chunk3 = Enumerable.Range(0, 1).Select(i => (byte)i).ToArray();
            var expected = chunk1.Concat(chunk2).Concat(chunk3).ToArray();

            var data = new HessianDataBuilder()
                .WriteBytes(0x41).WriteBytes(0xff, 0xff).WriteBytes(chunk1)
                .WriteBytes(0x41).WriteBytes(0xff, 0xff).WriteBytes(chunk2)
                .WriteBytes(0x21).WriteBytes(chunk3);

            CollectionAssert.AreEqual(
                expected,
                (byte[])new HessianInputV2(data.ToReader()).ReadObject());
        }

        [Test]
        public void List_Compact_Fixed()
        {
            var expected = new int[] { 0, 1 };

            var data = new HessianDataBuilder()
                .WriteBytes(0x72) // list tag (length 2)
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91); // int 1

            CollectionAssert.AreEqual(
                expected,
                (int[])new HessianInputV2(data.ToReader(), TypeBindings.Java).ReadObject());
        }

        [Test]
        public void List_Compact_Untyped_Fixed()
        {
            var expected = new List<int> { 0, 1 };

            var data = new HessianDataBuilder()
                .WriteBytes(0x7A) // list tag (length 2)
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91); // int 1

            CollectionAssert.AreEqual(
                expected,
                (List<int>)new HessianInputV2(data.ToReader()).ReadObject(typeof(List<int>)));
        }

        [Test]
        public void List_Fixed()
        {
            var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };

            var data = new HessianDataBuilder()
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
                .WriteBytes(0x97); // int 7

            CollectionAssert.AreEqual(
                expected,
                (int[])new HessianInputV2(data.ToReader(), TypeBindings.Java).ReadObject());
        }

        [Test]
        public void List_Untyped_Fixed()
        {
            var expected = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

            var data = new HessianDataBuilder()
                .WriteBytes(0x58) // list tag
                .WriteBytes(0x98) // length 8 (must be longer than 7 to avoid compact)
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteBytes(0x92) // int 2
                .WriteBytes(0x93) // int 3
                .WriteBytes(0x94) // int 4
                .WriteBytes(0x95) // int 5
                .WriteBytes(0x96) // int 6
                .WriteBytes(0x97); // int 7

            CollectionAssert.AreEqual(
                expected,
                (int[])new HessianInputV2(data.ToReader()).ReadObject(typeof(int[])));
        }

        [Test]
        public void List_Variable()
        {
            var expected = new int[] { 0, 1 };

            var data = new HessianDataBuilder()
                .WriteBytes(0x55) // list tag
                .WriteBytes(0x04).WriteUtf8("[int") // type
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteChar('Z'); // end

            CollectionAssert.AreEqual(
                expected,
                (List<int>)new HessianInputV2(data.ToReader()).ReadObject(typeof(List<int>)));
        }

        [Test]
        public void List_Untyped_Variable()
        {
            var expected = new int[] { 0, 1 };

            var data = new HessianDataBuilder()
                .WriteBytes(0x57) // list tag
                .WriteBytes(0x90) // int 0
                .WriteBytes(0x91) // int 1
                .WriteChar('Z'); // end

            CollectionAssert.AreEqual(
                expected,
                (List<object>)new HessianInputV2(data.ToReader()).ReadObject());
        }

        [Test]
        public void List_Compact_Fixed_UsingTypeRef()
        {
            var first = new int[] { 0, 1 };
            var second = new int[] { 2, 3, 4 };

            var data = new HessianDataBuilder()
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
                .WriteBytes(0x94); // int 4

            var input = new HessianInputV2(data.ToReader());

            CollectionAssert.AreEqual(
                first,
                (IEnumerable)input.ReadObject());

            CollectionAssert.AreEqual(
                second,
                (IEnumerable)input.ReadObject());
        }

        [Test]
        public void Map_SparseArray()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('H') // untyped map

                .WriteBytes(0x91)                  // 1
                .WriteBytes(0x03).WriteUtf8("fee") // "fee"

                .WriteBytes(0xa0)                  // 16
                .WriteBytes(0x03).WriteUtf8("fie") // "fie"

                .WriteBytes(0xc9, 0x00)            // 256
                .WriteBytes(0x03).WriteUtf8("foe") // foe

                .WriteChar('Z') // map end
                .ToReader(); // map end

            var actual = (IDictionary<object, object>)new HessianInputV2(reader).ReadObject();

            var expected = new Dictionary<object, object>
            {
                { 1, "fee" },
                { 16, "fie" },
                { 256, "foe" }
            };

            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var key in expected.Keys)
            {
                Assert.AreEqual(expected[key], actual[key]);
            }
        }

        [Test]
        public void Map_Object()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('M') // map

                .WriteBytes(0x13).WriteUtf8("com.caucho.test.Car") // type

                .WriteBytes(0x05).WriteUtf8("model")
                .WriteBytes(0x06).WriteUtf8("Beetle")

                .WriteBytes(0x05).WriteUtf8("color")
                .WriteBytes(0x0a).WriteUtf8("aquamarine")

                .WriteBytes(0x07).WriteUtf8("mileage")
                .WriteChar('I').WriteBytes(0, 0x01, 0, 0)

                .WriteChar('Z') // map end
                .ToReader();

            var expected = new com.caucho.test.Car
            {
                model = "Beetle",
                color = "aquamarine",
                mileage = 65536
            };

            Assert.AreEqual(expected, new HessianInputV2(reader).ReadObject());
        }

        [Test]
        public void Map_CompactObject()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('C') // definition
                .WriteBytes(0x13).WriteUtf8("com.caucho.test.Car")
                .WriteBytes(0x93) // 3 fields
                .WriteBytes(0x05).WriteUtf8("model")
                .WriteBytes(0x05).WriteUtf8("color")
                .WriteBytes(0x07).WriteUtf8("mileage")

                .WriteBytes(0x60) // instance
                .WriteBytes(0x06).WriteUtf8("Beetle")
                .WriteBytes(0x0a).WriteUtf8("aquamarine")
                .WriteChar('I').WriteBytes(0, 0x01, 0, 0)

                .ToReader();

            var expected = new com.caucho.test.Car
            {
                model = "Beetle",
                color = "aquamarine",
                mileage = 65536
            };

            Assert.AreEqual(expected, new HessianInputV2(reader).ReadObject());
        }

        [Test]
        public void Map_CompactEnum()
        {
            var first = Color.RED;
            var second = Color.GREEN;
            var third = Color.BLUE;
            var fourth = Color.GREEN;

            var reader = new HessianDataBuilder()
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

                .ToReader();

            var input = new HessianInputV2(reader);

            Assert.AreEqual(first, input.ReadObject());
            Assert.AreEqual(second, input.ReadObject());
            Assert.AreEqual(third, input.ReadObject());
            Assert.AreEqual(fourth, input.ReadObject());
        }

        [Test]
        public void Reply_Integer()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('H')
                .WriteBytes(2, 0) // version

                .WriteChar('R')
                .WriteBytes(0x95) // integer 5
                .ToReader();

            Assert.AreEqual(5, new HessianInputV2(reader).ReadReply(typeof(int)));
        }

        [Test]
        public void Reply_Fault()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('H')
                .WriteBytes(2, 0) // version

                .WriteChar('F') // fault

                .WriteChar('H') // map
                .WriteBytes(0x04).WriteUtf8("code")
                .WriteBytes(0x10).WriteUtf8("ServiceException")

                .WriteBytes(0x07).WriteUtf8("message")
                .WriteBytes(0x0e).WriteUtf8("File Not Found")

                .WriteBytes(0x06).WriteUtf8("detail")
                .WriteChar('M')
                    .WriteBytes(0x1d).WriteUtf8("java.io.FileNotFoundException")
                    .WriteChar('Z')

                .WriteChar('Z')
                .ToReader();

            var actual = (HessianRemoteException)new HessianInputV2(reader).ReadReply();

            Assert.AreEqual(FaultCode.ServiceException, actual.Code);
            Assert.AreEqual("File Not Found", actual.OriginalMessage);
            Assert.AreEqual("The hessian server reponded with 'File Not Found' (code: 'ServiceException')", actual.Message);
            Assert.IsInstanceOf<java.io.FileNotFoundException>(actual.InnerException);
        }
    }
}