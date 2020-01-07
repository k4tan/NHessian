using com.caucho.hessian.test;
using NHessian.Client;
using NHessian.IO;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NHessian.Tests.Client
{
    [TestFixture(ProtocolVersion.V1)]
    [TestFixture(ProtocolVersion.V2)]
    [Parallelizable(ParallelScope.Children)]
    public class Test2ServiceTests
    {
        private readonly ITest2Service _service;

        public Test2ServiceTests(ProtocolVersion protocolVersion)
        {
            /*
             * test server is a `hessian-test.jar`
             * downloadable here:
             *     http://www.java2s.com/Code/JarDownload/hessian/hessian-test.jar.zip
             *  OR http://hessian.caucho.com/#Java
             */
            _service = new HttpClient()
                .HessianService<ITest2Service>(
                //new Uri("http://localhost:8080/hessian/test2"),
                new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test2"),
                TypeBindings.Java,
                protocolVersion);
        }

        [Test]
        public async Task ArgNull()
        {
            await _service.argNull(null);
        }

        [Test]
        public async Task MethodNull()
        {
            await _service.methodNull();
        }

        [Test]
        public void ReplyNull()
        {
            _service.replyNull();
        }

        [Test]
        public async Task Bool()
        {
            Assert.True(_service.argTrue(_service.replyTrue()));
            Assert.True(await _service.argFalse(await _service.replyFalse()));
        }

        [Test]
        public async Task Int()
        {
            Assert.True(await _service.argInt_0(await _service.replyInt_0()));
            Assert.True(_service.argInt_1(_service.replyInt_1()));
            Assert.True(_service.argInt_47(_service.replyInt_47()));
            Assert.True(_service.argInt_m16(_service.replyInt_m16()));
            Assert.True(_service.argInt_0x30(_service.replyInt_0x30()));
            Assert.True(_service.argInt_0x7ff(_service.replyInt_0x7ff()));
            Assert.True(_service.argInt_m17(_service.replyInt_m17()));
            Assert.True(_service.argInt_m0x800(_service.replyInt_m0x800()));
            Assert.True(_service.argInt_0x800(_service.replyInt_0x800()));
            Assert.True(_service.argInt_0x3ffff(_service.replyInt_0x3ffff()));
            Assert.True(_service.argInt_m0x801(_service.replyInt_m0x801()));
            Assert.True(_service.argInt_m0x40000(_service.replyInt_m0x40000()));
            Assert.True(_service.argInt_0x40000(_service.replyInt_0x40000()));
            Assert.True(_service.argInt_0x7fffffff(_service.replyInt_0x7fffffff()));
            Assert.True(_service.argInt_m0x40001(_service.replyInt_m0x40001()));
            Assert.True(_service.argInt_m0x80000000(_service.replyInt_m0x80000000()));
        }

        [Test]
        public async Task Long()
        {
            Assert.True(await _service.argLong_0(await _service.replyLong_0()));
            Assert.True(_service.argLong_1(_service.replyLong_1()));
            Assert.True(_service.argLong_15(_service.replyLong_15()));
            Assert.True(_service.argLong_m8(_service.replyLong_m8()));
            Assert.True(_service.argLong_0x10(_service.replyLong_0x10()));
            Assert.True(_service.argLong_0x7ff(_service.replyLong_0x7ff()));
            Assert.True(_service.argLong_m9(_service.replyLong_m9()));
            Assert.True(_service.argLong_m0x800(_service.replyLong_m0x800()));
            Assert.True(_service.argLong_0x800(_service.replyLong_0x800()));
            Assert.True(_service.argLong_0x3ffff(_service.replyLong_0x3ffff()));
            Assert.True(_service.argLong_m0x801(_service.replyLong_m0x801()));
            Assert.True(_service.argLong_m0x40000(_service.replyLong_m0x40000()));
            Assert.True(_service.argLong_0x40000(_service.replyLong_0x40000()));
            Assert.True(_service.argLong_0x7fffffff(_service.replyLong_0x7fffffff()));
            Assert.True(_service.argLong_m0x40001(_service.replyLong_m0x40001()));
            Assert.True(_service.argLong_m0x80000000(_service.replyLong_m0x80000000()));
            Assert.True(_service.argLong_0x80000000(_service.replyLong_0x80000000()));
            Assert.True(_service.argLong_m0x80000001(_service.replyLong_m0x80000001()));
        }

        [Test]
        public async Task Double()
        {
            Assert.True(await _service.argDouble_0_0(await _service.replyDouble_0_0()));
            Assert.True(_service.argDouble_1_0(_service.replyDouble_1_0()));
            Assert.True(_service.argDouble_2_0(_service.replyDouble_2_0()));
            Assert.True(_service.argDouble_127_0(_service.replyDouble_127_0()));
            Assert.True(_service.argDouble_m128_0(_service.replyDouble_m128_0()));
            Assert.True(_service.argDouble_128_0(_service.replyDouble_128_0()));
            Assert.True(_service.argDouble_m129_0(_service.replyDouble_m129_0()));
            Assert.True(_service.argDouble_32767_0(_service.replyDouble_32767_0()));
            Assert.True(_service.argDouble_m32768_0(_service.replyDouble_m32768_0()));
            Assert.True(_service.argDouble_0_001(_service.replyDouble_0_001()));
            Assert.True(_service.argDouble_m0_001(_service.replyDouble_m0_001()));
            Assert.True(_service.argDouble_65_536(_service.replyDouble_65_536()));
            Assert.True(_service.argDouble_3_14159(_service.replyDouble_3_14159()));
        }

        [Test]
        public async Task Date()
        {
            Assert.True(await _service.argDate_0(await _service.replyDate_0()));
            Assert.True(_service.argDate_1(_service.replyDate_1()));
            Assert.True(_service.argDate_2(_service.replyDate_2()));
        }

        [Test]
        public async Task Binary()
        {
            Assert.Null(await _service.replyBinary_null());
            Assert.True(_service.argBinary_0(await _service.replyBinary_0()));
            Assert.True(_service.argBinary_1(_service.replyBinary_1()));
            Assert.True(_service.argBinary_15(_service.replyBinary_15()));
            Assert.True(_service.argBinary_16(_service.replyBinary_16()));
            Assert.True(_service.argBinary_1023(_service.replyBinary_1023()));
            Assert.True(_service.argBinary_1024(_service.replyBinary_1024()));
            Assert.True(_service.argBinary_65536(_service.replyBinary_65536()));
        }

        [Test]
        public async Task String()
        {
            Assert.Null(_service.replyString_null());
            Assert.True(_service.argString_0(await _service.replyString_0()));
            Assert.True(_service.argString_1(_service.replyString_1()));
            Assert.True(_service.argString_31(_service.replyString_31()));
            Assert.True(_service.argString_32(_service.replyString_32()));
            Assert.True(_service.argString_1023(_service.replyString_1023()));
            Assert.True(_service.argString_1024(_service.replyString_1024()));
            Assert.True(_service.argString_65536(_service.replyString_65536()));
        }

        [Test]
        public void Object()
        {
            Assert.True(_service.argObject_0(_service.replyObject_0()));
            Assert.True(_service.argObject_16(_service.replyObject_16()));
            Assert.True(_service.argObject_1(_service.replyObject_1()));
            Assert.True(_service.argObject_2(_service.replyObject_2()));
            Assert.True(_service.argObject_2a(_service.replyObject_2a()));
            Assert.True(_service.argObject_2b(_service.replyObject_2b()));
            Assert.True(_service.argObject_3(_service.replyObject_3()));
        }

        [Test]
        public void TypedFixedList()
        {
            // IMPORTANT Serializing List does not work here!! (de-serializing does)

            Assert.True(_service.argTypedFixedList_0(_service.replyTypedFixedList_0()));
            Assert.True(_service.argTypedFixedList_1(_service.replyTypedFixedList_1()));
            Assert.True(_service.argTypedFixedList_7(_service.replyTypedFixedList_7()));
            Assert.True(_service.argTypedFixedList_8(_service.replyTypedFixedList_8()));
        }

        [Test]
        public void TypedMap()
        {
            Assert.True(_service.argTypedMap_0(_service.replyTypedMap_0()));
            Assert.True(_service.argTypedMap_1(_service.replyTypedMap_1()));
            Assert.True(_service.argTypedMap_2(_service.replyTypedMap_2()));
            Assert.True(_service.argTypedMap_3(_service.replyTypedMap_3()));
        }

        [Test]
        public void UntypedList()
        {
            Assert.True(_service.argUntypedFixedList_0(_service.replyUntypedFixedList_0()));
            Assert.True(_service.argUntypedFixedList_1(_service.replyUntypedFixedList_1()));
            Assert.True(_service.argUntypedFixedList_7(_service.replyUntypedFixedList_7()));
            Assert.True(_service.argUntypedFixedList_8(_service.replyUntypedFixedList_8()));
        }

        [Test]
        public void UntypedMap()
        {
            Assert.True(_service.argUntypedMap_0(_service.replyUntypedMap_0()));
            Assert.True(_service.argUntypedMap_1(_service.replyUntypedMap_1()));
            Assert.True(_service.argUntypedMap_2(_service.replyUntypedMap_2()));
            Assert.True(_service.argUntypedMap_3(_service.replyUntypedMap_3()));
        }
    }
}