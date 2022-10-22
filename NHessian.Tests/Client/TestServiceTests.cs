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
                new Uri("https://nhessian-hessian-test.herokuapp.com/hessian/test"),
                new ClientOptions()
                {
                    TypeBindings = TypeBindings.Java,
                    ProtocolVersion = protocolVersion
                });
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
                "any string",
                // 2 byte UTF-8 (߿)
                "\u07FF",
                // 3 byte UTF-8 (ﾯ)
                "\uffaf",
                // 4 byte UTF-8 (𠀀 / Surrogate Pair)
                "\ud840\udc00"
            };

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], await _service.echo(values[i]));
            }
        }

        [Test]
        public async Task Echo_Dates()
        {
            /**Tests that dates and times survive a roundtrip.
             * The following rules are important:
             * - Hessian encodes/transmitts DateTime in UTC format
             * - NHession library deserializes as UTC but returns in DateTimKind.Local format
             * - DateTime serialized with NHessian that have DateTimeKind.Unspecified are treated as Local
             * - Hessian V2 optimizes dates that do not have seconds/ms with a special short format (Int32 instead of Int64)
             * - The short date format overflows at some point (~year 6000). The library falls back to long format in that case.
             */
            var values = new DateTime[]
            {
                // DateTimeKind tests
                new DateTime(1969, 7, 16, 13, 32, 0, DateTimeKind.Local),
                new DateTime(1969, 7, 16, 13, 32, 55, DateTimeKind.Local),

                // unspecified should be treated as local
                // NOTE this test is only conclusive if local time is not same as UTC
                new DateTime(1969, 7, 16, 13, 32, 0, DateTimeKind.Unspecified),
                new DateTime(1969, 7, 16, 13, 32, 55, DateTimeKind.Unspecified),

                // Short date overflow test for hessian v2 (DateTimeKind is Utc).
                // The short format overflows at some point an fallbacks to the long format.
                // This tests that behavior.
                DateTime.UnixEpoch.AddMinutes(((long)int.MaxValue) + 1),

                /* extrems (MinValue and MaxValues are DateTimeKind.Unspecified) */
                // MinValue
                DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                // forces long date format in hessian 2
                DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc).AddSeconds(1),
                // Max date without seconds/ms (hessian 2 short date format)
                DateTime.SpecifyKind(DateTime.Parse(DateTime.MaxValue.ToString("yyyy-MM-ddTH:mm")), DateTimeKind.Utc),
                // Max DateTime (ms is the maximum precision of hessian. ns are lost on roundtrip)
                DateTime.SpecifyKind(DateTime.Parse(DateTime.MaxValue.ToString("yyyy-MM-ddTH:mm:ss.fff")), DateTimeKind.Utc),
            };

            for (int i = 0; i < values.Length; i++)
            {
                var result = (DateTime)await _service.echo(values[i]);

                var expected = values[i];
                if (expected.Kind == DateTimeKind.Unspecified)
                {
                    // unspecified should be treated as local
                    expected = DateTime.SpecifyKind(expected, DateTimeKind.Local);
                }

                // NHessian returns values as DateTimeKind.Local
                Assert.AreEqual(expected.ToLocalTime(), result);
            }
        }

        [Test]
        public async Task Echo_Object()
        {
            var value = new IO.Stubs.TestClass("string");

            var actual = await _service.echo<IO.Stubs.TestClass>(value);

            // survive roun-drip
            Assert.AreEqual("string", actual.publicStr);
            Assert.AreEqual("string", actual.getProtectedStr());
            Assert.AreEqual("string", actual.getPrivateStr());
            // do not survive roun-drip
            Assert.IsNull(actual.readonlyStr);
            Assert.IsNull(actual.nonSerializedStr);
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