using System;
using System.Collections.Generic;
using System.Reflection;

namespace NHessian.IO.Serialization
{
    internal class ExceptionSerializer : MapSerializer
    {
        private readonly IReadOnlyList<FieldInfo> _fields;
        private readonly Type _type;

        public ExceptionSerializer(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        public override void Serialize(HessianOutput output, object map, string customTypeName)
        {
            var typeName = customTypeName ?? _type.FullName;

            output.WriteMapStart(typeName);

            // entries
            for (int i = 0; i < _fields.Count; i++)
            {
                var fieldInfo = _fields[i];
                output.WriteString(fieldInfo.Name);
                output.WriteObject(fieldInfo.GetValue(map));
            }

            output.WriteMapEnd();
        }
    }
}