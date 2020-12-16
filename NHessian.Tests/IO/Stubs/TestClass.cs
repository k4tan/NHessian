using System;

namespace NHessian.Tests.IO.Stubs
{
    /// <summary>
    /// Designed to test various field de-/serialization behaviors. 
    /// </summary>
    public class TestClass
    {
        /** non-instance fields (non de-/serialized) */
        public const string CONST_STR = "const";
        public static string StaticStr = "static";

        /** instance fields with different visibilities (de-/serializable) */
        public string publicStr;
        protected string protectedStr;
        private string privateStr;

        /** non de-/serializable */
        [NonSerialized]
        public string nonSerializedStr;

        /** serializable but not deserializable (one way) */
        public readonly string readonlyStr;

        public TestClass()
        {
        }

        public TestClass(string str)
        {
            publicStr = str;
            protectedStr = str;
            privateStr = str;
            nonSerializedStr = str;
            readonlyStr = str;
        }

        public string getPrivateStr() => privateStr;
        public string getProtectedStr() => protectedStr;
    }
}
