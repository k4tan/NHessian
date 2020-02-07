using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NHessian.IO.Serialization
{
    internal class DefaultFieldSerializer : FieldSerializer
    {
        protected readonly Func<object, object> _getter;

        public DefaultFieldSerializer(FieldInfo fieldInfo)
            : base(fieldInfo)
        {
            ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
            MemberExpression access = Expression.MakeMemberAccess(
                Expression.Convert(objExp, fieldInfo.DeclaringType),
                fieldInfo);

            _getter = Expression
                .Lambda<Func<object, object>>(Expression.Convert(access, typeof(object)), objExp)
                .Compile();
        }

        public override void WriteField(HessianOutput output, object obj)
        {
            output.WriteObject(_getter(obj));
        }
    }
}