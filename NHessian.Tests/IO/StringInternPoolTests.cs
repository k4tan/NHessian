using System;
using System.Linq;
using NHessian.IO;
using NUnit.Framework;

namespace NHessian.Tests.IO
{
    [TestFixture]
    public class StringInternPoolTests
    {
        private StringInternPool _stringPool;

        [SetUp]
        public void Setup() => _stringPool = new StringInternPool();

        [TearDown]
        public void TearDown() => _stringPool.Dispose();

        [Test]
        public void GetOrAdd_LengthExceedsBuffer_ShouldThrow()
        {
            var buf = new char[1];

            Assert.Throws<ArgumentException>(() => _stringPool.GetOrAdd(buf, 2));
        }

        [Test]
        public void GetOrAdd_NothingInterned_ShouldReturnString()
        {
            var buf = "abcdefgh".ToCharArray();

            var actual = _stringPool.GetOrAdd(buf, buf.Length);

            Assert.AreEqual("abcdefgh", actual);
        }

        [Test]
        public void GetOrAdd_StringInterned_ShouldReturnSingleton()
        {
            var buf1 = "abcdefgh".ToCharArray();
            var buf2 = "abcdefgh".ToCharArray();

            var s1 = _stringPool.GetOrAdd(buf1, buf1.Length);
            var s2 = _stringPool.GetOrAdd(buf2, buf2.Length);

            Assert.AreSame(s1, s2);
        }

        [Test]
        public void GetOrAdd_SubsetOfCharBuffer_ShouldReturnStringFromSubset()
        {
            var buf1 = "abcdefgh some padding".ToCharArray();
            var buf2 = "abcdefgh another padding".ToCharArray();

            var s1 = _stringPool.GetOrAdd(buf1, 8);
            var s2 = _stringPool.GetOrAdd(buf2, 8);

            Assert.AreSame(s1, s2);
        }

        [Test]
        public void GetOrAdd_StressTest_FastAccessToInterned()
        {
            var bufs = new[]
            {
                "abcdefghfgmbopldsnvlmdxc v,sdmdnfsdjnfoa".ToCharArray(),
                "ldjkhflska[so;pkojfdwsodjnfsldkmfdn,zdmhfoas".ToCharArray(),
                "pwdeihosfdsopdkidifcdhjcwidbnfwqkerjnciewqrfjhb".ToCharArray(),
            };

            var interned = bufs.Select(buf => _stringPool.GetOrAdd(buf, buf.Length)).ToArray();

            for (int i = 0; i < 50000; i++)
            {
                var idx = i % 3;
                var s = _stringPool.GetOrAdd(bufs[idx], bufs[idx].Length);
                Assert.AreSame(interned[idx], s);
            }
        }

        [Test]
        public void GetOrAdd_StressTest_Capacity()
        {
            var random = new Random(0);
            string RandomString(int length)
            {
                var buf = new char[length];
                for (int i = 0; i < length; i++)
                    buf[i] = (char)random.Next(32, 127);

                return new string(buf);
            }

            for (int i = 0; i < 10000; i++)
            {
                var s = RandomString(48);

                var actual = _stringPool.GetOrAdd(s.ToCharArray(), s.Length);

                Assert.AreEqual(s, actual);
            }
        }
    }
}