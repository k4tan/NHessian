using System;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    internal abstract class FieldDeserializer
    {
        protected FieldDeserializer(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        }

        public FieldInfo FieldInfo { get; }

        public abstract void PopulateField(HessianInput input, object obj);
    }
}