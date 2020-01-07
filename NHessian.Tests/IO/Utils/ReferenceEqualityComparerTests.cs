using NHessian.IO.Utils;
using NUnit.Framework;

namespace NHessian.Tests.IO.Utils
{
    [TestFixture]
    public class ReferenceEqualityComparerTests
    {
        [Test]
        public void Equals_SameInstance_RetrurnsTrue()
        {
            var item = new TestClass("123");

            Assert.True(ReferenceEqualityComparer.Default.Equals(item, item));
        }

        [Test]
        public void Equals_DifferentInstances_UsesReferenceEquals()
        {
            var item1 = new TestClass("123");
            var item2 = new TestClass("123");

            // verify regulat equality works
            Assert.True(item1.Equals(item2));

            // reference equality should be false
            Assert.False(ReferenceEqualityComparer.Default.Equals(item1, item2));
        }

        [Test]
        public void Equals_SameEnum_ReturnsTrue()
        {
            Assert.True(ReferenceEqualityComparer.Default.Equals(TestEnum.a, TestEnum.a));
        }

        [Test]
        public void Equals_DifferentEnum_ReturnsFalse()
        {
            Assert.False(ReferenceEqualityComparer.Default.Equals(TestEnum.a, TestEnum.b));
        }

        [Test]
        public void GetHashCode_SameInstance_SameHashCode()
        {
            var item = new TestClass("123");

            var hash1 = ReferenceEqualityComparer.Default.GetHashCode(item);
            var hash2 = ReferenceEqualityComparer.Default.GetHashCode(item);
            Assert.True(hash1 == hash2);
        }

        [Test]
        public void GetHashCode_DifferentInstances_UsesObjectGetHashCode()
        {
            var item1 = new TestClass("123");
            var item2 = new TestClass("123");

            // verify regulat GetHashCode works
            Assert.True(item1.GetHashCode() == item2.GetHashCode());

            // reference equality should be false
            var hash1 = ReferenceEqualityComparer.Default.GetHashCode(item1);
            var hash2 = ReferenceEqualityComparer.Default.GetHashCode(item2);
            Assert.False(hash1 == hash2);
        }

        [Test]
        public void GetHashCode_SameEnum_SameHashCode()
        {
            var hash1 = ReferenceEqualityComparer.Default.GetHashCode(TestEnum.a);
            var hash2 = ReferenceEqualityComparer.Default.GetHashCode(TestEnum.a);
            Assert.True(hash1 == hash2);
        }

        /// <summary>
        /// Test class that overrides equality.
        /// Used to verify that override is ignored and reference equality is used.
        /// </summary>
        private class TestClass
        {
            public TestClass(string id) => Id = id;
            public string Id { get; }
            public override bool Equals(object obj)
            {
                return obj is TestClass other ? Equals(Id, other.Id) : false;
            }
            public override int GetHashCode() => Id.GetHashCode();
        }

        private enum TestEnum
        {
            a,
            b
        }
    }
}
