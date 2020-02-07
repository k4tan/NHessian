using System;
using System.Net.Http;
using NHessian.Client;
using NUnit.Framework;
using com.caucho.hessian.test;
using System.Threading.Tasks;
using java.lang;
using NHessian.IO;

namespace NHessian.Tests.Client
{
    [TestFixture(ProtocolVersion.V1)]
    [TestFixture(ProtocolVersion.V2)]
    [Parallelizable(ParallelScope.Children)]
    public class TestServiceTests
    {
        private readonly ITestService _service;

        public TestServiceTests(ProtocolVersion protocolVersion)
        {
            /*
             * test server is a `hessian-test.jar`
             * downloadable here:
             *     http://www.java2s.com/Code/JarDownload/hessian/hessian-test.jar.zip
             *  OR http://hessian.caucho.com/#Java
             */
            _service = new HttpClient()
                .HessianService<ITestService>(
                //new Uri("http://localhost:8080/hessian/test"),
                new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test"),
                TypeBindings.Java,
                protocolVersion);
        }

        [Test]
        public async Task NullCall()
        {
            await _service.nullCall();
        }

        [Test]
        public async Task Hello()
        {
            Assert.AreEqual("Hello, World", await _service.hello());
        }

        [Test]
        public async Task Subtract()
        {
            Assert.AreEqual(3, await _service.subtract(5, 2));
            Assert.AreEqual(0x7ffbfffe, await _service.subtract(0x7fffffff, 0x40001));
            Assert.AreEqual(0x3f801, await _service.subtract(0x40000, 0x7ff));
            Assert.AreEqual(-0x3F800, await _service.subtract(-0x40000, -0x800));
        }

        [Test]
        public async Task Echo()
        {
            object[] values = new object[]
            {
                true,
                0x7ff,
                -0x80000001,
                3.14159,
                new DateTime(1969, 7, 16, 13, 32, 0),
                "any string"
            };

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], await _service.echo(values[i]));
            }
        }

        [Test]
        public async Task Echo_Enum()
        {
            var value = example.Color.GREEN;
            Assert.AreEqual(value, await _service.echo<example.Color>(value));
        }

        [Test]
        public async Task Echo_ListEnum()
        {
            var values = new example.Color[] { example.Color.GREEN, example.Color.RED };
            CollectionAssert.AreEqual(values, await _service.echo<example.Color[]>(values));
        }

        [Test]
        public async Task Echo_EnumObject()
        {
            var value = new example.TestObjectWithEnum() { _value = example.Color.GREEN };
            var result = await _service.echo<example.TestObjectWithEnum>(value);
            Assert.AreEqual(value._value, result._value);
        }

        [Test]
        public async Task Fault()
        {
            try
            {
                await _service.fault();
                Assert.Fail("Exception expected");
            }
            catch (NullPointerException e)
            {
                Assert.NotNull(e);
                Assert.AreEqual("sample exception", e.Message);
            }
        }
    }
}