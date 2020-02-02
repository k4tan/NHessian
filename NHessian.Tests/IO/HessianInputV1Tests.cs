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
    public class HessianInputV1Tests
    {
        [Test]
        public void Null()
        {
            var reader = new HessianDataBuilder()
                .WriteChar('N')
                .ToReader();

            Assert.Null(new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void BooleanTrue()
        {
            var data = new HessianDataBuilder()
                .WriteChar('T');

            Assert.True((bool)new HessianInputV1(data.ToReader()).ReadObject());
            Assert.True(new HessianInputV1(data.ToReader()).ReadBool());
        }

        [Test]
        public void BooleanFalse()
        {
            var data = new HessianDataBuilder()
                .WriteChar('F');

            Assert.False((bool)new HessianInputV1(data.ToReader()).ReadObject());
            Assert.False(new HessianInputV1(data.ToReader()).ReadBool());
        }

        [Test]
        public void Int300()
        {
            var data = new HessianDataBuilder()
                .WriteChar('I').WriteBytes(0, 0, 0x01, 0x2C);

            Assert.AreEqual(300, new HessianInputV1(data.ToReader()).ReadObject());
            Assert.AreEqual(300, new HessianInputV1(data.ToReader()).ReadInt());
        }

        [Test]
        public void Long300()
        {
            var data = new HessianDataBuilder()
                .WriteChar('L').WriteBytes(0, 0, 0, 0, 0, 0, 0x01, 0x2C);

            Assert.AreEqual(300, new HessianInputV1(data.ToReader()).ReadObject());
            Assert.AreEqual(300, new HessianInputV1(data.ToReader()).ReadLong());
        }

        [Test]
        public void Double12_25()
        {
            var data = new HessianDataBuilder()
                .WriteChar('D').WriteBytes(0x40, 0x28, 0x80, 0, 0, 0, 0, 0);

            Assert.AreEqual(12.25, new HessianInputV1(data.ToReader()).ReadObject());
            Assert.AreEqual(12.25, new HessianInputV1(data.ToReader()).ReadDouble());
        }

        [Test]
        public void Date()
        {
            // 9:51:31 May 8, 1998
            var data = new HessianDataBuilder()
                .WriteChar('d').WriteBytes(0, 0, 0, 0xD0, 0x4B, 0x92, 0x84, 0xB8);

            var expected = new DateTime(1998, 5, 8, 9, 51, 31, DateTimeKind.Utc).ToLocalTime();

            Assert.AreEqual(expected, new HessianInputV1(data.ToReader()).ReadObject());
            Assert.AreEqual(expected, new HessianInputV1(data.ToReader()).ReadDate());
        }

        [Test]
        public void String_SingleByteUnicode()
        {
            var str = "hello";
            var reader = new HessianDataBuilder()
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8(str)
                .ToReader();

            Assert.AreEqual(str, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void String_MultiByteUnicode()
        {
            var str = "세미";
            var reader = new HessianDataBuilder()
                .WriteChar('S').WriteBytes(0, 0x02).WriteUtf8(str)
                .ToReader();

            Assert.AreEqual(str, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void String_MultiChunk()
        {
            var chunk1 = "hello, ";
            var chunk2 = "세미";

            var reader = new HessianDataBuilder()
                // hello
                .WriteChar('s').WriteBytes(0, 0x07).WriteUtf8(chunk1)
                // 세미
                .WriteChar('S').WriteBytes(0, 0x02).WriteUtf8(chunk2)
                .ToReader();

            Assert.AreEqual(chunk1 + chunk2, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Xml()
        {
            var str = "<top>hello</top>";
            var reader = new HessianDataBuilder()
                // xml
                .WriteChar('X').WriteBytes(0, 0x10).WriteUtf8(str)
                .ToReader();

            Assert.AreEqual(str, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Xml_MultiChunk()
        {
            var chunk1 = "<top>hello, ";
            var chunk2 = "세미</top>";

            var reader = new HessianDataBuilder()
                // chunk1
                .WriteChar('x').WriteBytes(0, 0x0C).WriteUtf8(chunk1)
                // chunk2
                .WriteChar('X').WriteBytes(0, 0x08).WriteUtf8(chunk2)

                .ToReader();

            Assert.AreEqual(chunk1 + chunk2, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Binary_SingleChunk()
        {
            var byteArr = new byte[] { 0x28 };
            var reader = new HessianDataBuilder()
                // byte array
                .WriteChar('B').WriteBytes(0, 0x01).WriteBytes(byteArr)
                .ToReader();

            CollectionAssert.AreEqual(byteArr, (IEnumerable)new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Binary_MultiChunk()
        {
            var chunk1 = new byte[] { 0x28 };
            var chunk2 = new byte[] { 0x50, 0x70 };
            var chunk3 = new byte[] { 0x90 };
            var reader = new HessianDataBuilder()
                // chunk1
                .WriteChar('b').WriteBytes(0, 0x01).WriteBytes(chunk1)
                // chunk2
                .WriteChar('b').WriteBytes(0, 0x02).WriteBytes(chunk2)
                // chunk3 (final)
                .WriteChar('B').WriteBytes(0, 0x01).WriteBytes(chunk3)
                .ToReader();

            CollectionAssert.AreEqual(
                chunk1.Concat(chunk2).Concat(chunk3),
                (IEnumerable)new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void List_TypedFixedLength()
        {
            var reader = new HessianDataBuilder()
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
                .ToReader();

            // for now, we do not created type lists
            var expected = new object[] { 0, 1 };
            var actual = new HessianInputV1(reader, TypeBindings.Java).ReadObject();

            Assert.IsInstanceOf<int[]>(actual);
            CollectionAssert.AreEqual(expected, (int[])actual);
        }

        [Test]
        public void List_NonTypedVariableLength()
        {
            var reader = new HessianDataBuilder()
                // list
                .WriteChar('V')
                // item1 (int)
                .WriteChar('I').WriteBytes(0, 0, 0, 0)
                // item2 (string)
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("foobar")
                // list end
                .WriteChar('z')
                .ToReader();

            // for now, we do not created type lists
            var expected = new List<object> { 0, "foobar" };

            CollectionAssert.AreEqual(expected, (IEnumerable)new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Map_Object()
        {
            var reader = new HessianDataBuilder()
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
                .ToReader();

            var expected = new com.caucho.test.Car { model = "Beetle", color = "aquamarine", mileage = 65536 };

            Assert.AreEqual(expected, new HessianInputV1(reader).ReadObject());
        }

        [Test]
        public void Map_SparseArray()
        {
            var reader = new HessianDataBuilder()
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
                .ToReader();

            var actual = (IDictionary<object, object>)new HessianInputV1(reader).ReadObject();

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
        public void Ref_CircularList()
        {
            var reader = new HessianDataBuilder()
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
                .ToReader();

            // expectation:
            //   head is an integer
            //   tail is object itself (circular ref)
            var expected = new com.caucho.test.LinkedList { head = 1 };
            expected.tail = expected;

            var actual = (com.caucho.test.LinkedList)new HessianInputV1(reader).ReadObject();

            Assert.AreEqual(expected.head, actual.head);
            // check that ref was restored correctly
            Assert.AreSame(expected, expected.tail);
        }

        [Test]
        public void Enum()
        {
            var expected = Color.GREEN;
            var data = new HessianDataBuilder()
                .WriteChar('M')
                .WriteChar('t').WriteBytes(0, 0x0d).WriteUtf8("example.Color")
                .WriteChar('S').WriteBytes(0, 0x04).WriteUtf8("name")
                .WriteChar('S').WriteBytes(0, 0x05).WriteUtf8("GREEN")
                .WriteChar('z');

            Assert.AreEqual(
                expected,
                new HessianInputV1(data.ToReader()).ReadObject());
        }

        //[Test]
        //public void Call()
        //{
        //    var reader = new HessianDataBuilder()
        //        // call
        //        .WriteChar('c')
        //        // version (major version 1, minor 0)
        //        .WriteBytes(1, 0)
        //        // method name
        //        .WriteChar('m').WriteBytes(0, 0x04).WriteUtf8("add2")
        //        // arg1
        //        .WriteChar('I').WriteBytes(0, 0, 0, 0x02)
        //        // arg2
        //        .WriteChar('I').WriteBytes(0, 0, 0, 0x03)
        //        // call end
        //        .WriteChar('z')
        //        .ToReader();

        //    var actual = new HessianInputV1(reader).ReadCall();

        //    Assert.AreEqual("add2", actual.MethodName);
        //    CollectionAssert.IsEmpty(actual.Headers);
        //    CollectionAssert.AreEqual(new object[] { 2, 3 }, actual.Args);
        //}

        //[Test]
        //public void Call_WithRef()
        //{
        //    var reader = new HessianDataBuilder()
        //        // call
        //        .WriteChar('c')
        //        // version (major version 1, minor 0)
        //        .WriteBytes(1, 0)
        //        // method name
        //        .WriteChar('m').WriteBytes(0, 0x02).WriteUtf8("eq")
        //        // arg1
        //        .WriteChar('M')
        //            .WriteChar('t').WriteBytes(0, 0x07).WriteUtf8("qa.Bean")
        //            // field foo
        //            .WriteChar('S').WriteBytes(0, 0x03).WriteUtf8("foo")
        //            .WriteChar('I').WriteBytes(0, 0, 0, 0x0D)
        //            // end of map
        //            .WriteChar('z')
        //        // arg2: ref to arg 1
        //        .WriteChar('R').WriteBytes(0, 0, 0, 0)
        //        // call end
        //        .WriteChar('z')
        //        .ToReader();

        //    var actual = new HessianInputV1(reader).ReadCall();

        //    Assert.AreEqual("eq", actual.MethodName);
        //    CollectionAssert.IsEmpty(actual.Headers);
        //    // check args
        //    Assert.AreEqual(2, actual.Args.Count);
        //    Assert.IsInstanceOf<Bean>(actual.Args[0]);
        //    Assert.AreEqual(13, ((Bean)actual.Args[0]).foo);
        //    Assert.AreSame(actual.Args[0], actual.Args[1]);
        //}

        //[Test]
        //public void Call_WithHeader()
        //{
        //    var reader = new HessianDataBuilder()
        //        // call
        //        .WriteChar('c')
        //        // version (major version 1, minor 0)
        //        .WriteBytes(1, 0)
        //        // header(s)
        //        .WriteChar('H')
        //            .WriteBytes(0, 0x0B).WriteUtf8("transaction")
        //            .WriteChar('I').WriteBytes(0, 0, 0, 0)
        //        // method name
        //        .WriteChar('m').WriteBytes(0, 0x05).WriteUtf8("debug")
        //        // arg1
        //        .WriteChar('I').WriteBytes(0, 0x03, 0x01, 0xCB)
        //        // call end
        //        .WriteChar('z')
        //        .ToReader();

        //    var actual = new HessianInputV1(reader).ReadCall();

        //    Assert.AreEqual("debug", actual.MethodName);

        //    // check headers
        //    Assert.AreEqual(1, actual.Headers.Count);
        //    Assert.AreEqual("transaction", actual.Headers[0].Item1);
        //    Assert.AreEqual(0, actual.Headers[0].Item2);

        //    // check args
        //    Assert.AreEqual(1, actual.Args.Count);
        //    Assert.AreEqual(197067, actual.Args[0]);
        //}

        [Test]
        public void Reply_Integer()
        {
            var reader = new HessianDataBuilder()
                // reply
                .WriteChar('r')
                // version (major version 1, minor 0)
                .WriteBytes(1, 0)
                // content
                .WriteChar('I').WriteBytes(0, 0, 0, 0x05)
                // reply end
                .WriteChar('z')
                .ToReader();

            Assert.AreEqual(5, new HessianInputV1(reader).ReadReply(typeof(int)));
        }

        [Test]
        public void Reply_Fault()
        {
            var reader = new HessianDataBuilder()
                // reply
                .WriteChar('r')
                // version (major version 1, minor 0)
                .WriteBytes(1, 0)
                // fault
                .WriteChar('f')
                // code field
                .WriteChar('S').WriteBytes(0, 0x04).WriteUtf8("code")
                .WriteChar('S').WriteBytes(0, 0x10).WriteUtf8("ServiceException")
                // message field
                .WriteChar('S').WriteBytes(0, 0x07).WriteUtf8("message")
                .WriteChar('S').WriteBytes(0, 0x0E).WriteUtf8("File Not Found")
                // detail field
                .WriteChar('S').WriteBytes(0, 0x06).WriteUtf8("detail")
                .WriteChar('M')
                    // type
                    .WriteChar('t').WriteBytes(0, 0x1D).WriteUtf8("java.io.FileNotFoundException")
                    // map end
                    .WriteChar('z')
                // reply end
                .WriteChar('z')
                .ToReader();

            var actual = (HessianRemoteException)new HessianInputV1(reader).ReadReply();

            Assert.AreEqual(FaultCode.ServiceException, actual.Code);
            Assert.AreEqual("File Not Found", actual.OriginalMessage);
            Assert.AreEqual("The hessian server reponded with 'File Not Found' (code: 'ServiceException')", actual.Message);
            Assert.IsInstanceOf<java.io.FileNotFoundException>(actual.InnerException);
        }
    }
}