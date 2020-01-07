using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    internal class DefaultFieldDeserializer : FieldDeserializer
    {
        protected readonly Action<object, object> _assign;

        public DefaultFieldDeserializer(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
            ParameterExpression targetExp = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");

            MemberExpression fieldExp = Expression.Field(
                Expression.Convert(targetExp, fieldInfo.DeclaringType),
                fieldInfo);
            BinaryExpression assignExp = Expression.Assign(
                fieldExp,
                Expression.Convert(valueExp, fieldInfo.FieldType));

            _assign = Expression.Lambda<Action<object, object>>(assignExp, targetExp, valueExp).Compile();
        }

        public override void PopulateField(HessianInput input, object obj)
        {
            _assign(obj, input.ReadObject(FieldInfo.FieldType));
        }
    }
}