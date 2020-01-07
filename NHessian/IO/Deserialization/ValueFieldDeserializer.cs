using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    internal class ValueFieldDeserializer<T> : FieldDeserializer
    {
        protected readonly Action<object, T> _assign;
        private readonly Func<HessianInput, T> _valueReader;

        public ValueFieldDeserializer(FieldInfo fieldInfo, Func<HessianInput, T> valueReader) : base(fieldInfo)
        {
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(T), "value");

            MemberExpression fieldExp = Expression.Field(
                Expression.Convert(targetExp, fieldInfo.DeclaringType),
                fieldInfo);
            BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

            _assign = Expression.Lambda<Action<object, T>>(assignExp, targetExp, valueExp).Compile();
            _valueReader = valueReader;
        }

        public override void PopulateField(HessianInput input, object obj)
        {
            _assign(obj, _valueReader(input));
        }
    }
}