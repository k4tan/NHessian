using System;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    /// <summary>
    /// Java Exception contain cyclic references in their "cause" field.
    /// .NET crashes if that is translated to the "_innerException" field.
    /// This special FieldDeserializer is meant for "_innerException" fields only!
    /// It breaks resolves the cycles so .NET is happy.
    /// </summary>
    internal class InnerExceptionFieldDeserializer : DefaultFieldDeserializer
    {
        public InnerExceptionFieldDeserializer(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
        }

        public override void PopulateField(HessianInput input, object obj)
        {
            if (obj is Exception ex)
            {
                var fieldValue = input.ReadObject(FieldInfo.FieldType);
                if (fieldValue is Exception inner && InnerContainsEx(ex, inner))
                {
                    // we cannot assign inner exception if it already contains ex.
                    // This would give us a cycle in _innerException that crashes .Net.
                    return;
                }

                _assign(obj, fieldValue);
            }
            else
            {
                base.PopulateField(input, obj);
            }
        }

        private bool InnerContainsEx(Exception ex, Exception inner)
        {
            var current = inner;
            while (current != null)
            {
                if (current == ex)
                    return true;

                current = current.InnerException;
            }
            return false;
        }
    }
}