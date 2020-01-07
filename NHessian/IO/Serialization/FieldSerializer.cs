using System;
using System.Reflection;

namespace NHessian.IO.Serialization
{
    internal abstract class FieldSerializer
    {
        protected FieldSerializer(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        }

        public FieldInfo FieldInfo { get; }

        public abstract void WriteField(HessianOutput output, object obj);
    }
}