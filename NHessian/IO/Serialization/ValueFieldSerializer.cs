using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NHessian.IO.Serialization
{
    internal class ValueFieldSerializer<T> : FieldSerializer
    {
        protected readonly Func<object, T> _getter;
        private readonly Action<HessianOutput, T> _valueWriter;

        public ValueFieldSerializer(FieldInfo fieldInfo, Action<HessianOutput, T> valueWriter) : base(fieldInfo)
        {
            ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
            MemberExpression access = Expression.MakeMemberAccess(
                Expression.Convert(objExp, fieldInfo.DeclaringType),
                fieldInfo);

            _getter = Expression.Lambda<Func<object, T>>(access, objExp).Compile();
            _valueWriter = valueWriter;
        }

        public override void WriteField(HessianOutput output, object obj)
        {
            _valueWriter(output, _getter(obj));
        }
    }
}