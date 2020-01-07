using System;
using System.Collections.Generic;
using System.Reflection;

namespace NHessian.IO.Serialization
{
    internal class ObjectSerializer : MapSerializer
    {
        private readonly IReadOnlyList<FieldInfo> _fields;
        private readonly FieldSerializer[] _fieldSerializers;
        private readonly Type _type;
        private string[] _fieldNames;

        public ObjectSerializer(Type type)
        {
            _fields = CollectFields(type);
            _fieldNames = new string[_fields.Count];
            _fieldSerializers = new FieldSerializer[_fields.Count];
            for (int i = 0; i < _fields.Count; i++)
            {
                _fieldNames[i] = _fields[i].Name;
                _fieldSerializers[i] = CreateFieldSerializer(_fields[i]);
            }

            _type = type;
        }

        public override void Serialize(HessianOutput output, object map, string customTypeName)
        {
            var typeName = customTypeName ?? _type.FullName;
            var defIdx = output.WriteMapDefinition(typeName, _fieldNames);

            if (defIdx < 0)
            {
                output.WriteMapStart(typeName);
                for (int i = 0; i < _fields.Count; i++)
                {
                    var fieldInfo = _fields[i];
                    output.WriteString(_fieldNames[i]);
                    _fieldSerializers[i].WriteField(output, map);
                }
                output.WriteMapEnd();
            }
            else
            {
                output.WriteMapStart(defIdx);

                for (int i = 0; i < _fields.Count; i++)
                    _fieldSerializers[i].WriteField(output, map);
            }
        }

        private static IReadOnlyList<FieldInfo> CollectFields(Type type)
        {
            var fields = new List<FieldInfo>();
            for (; type != null; type = type.BaseType)
            {
                fields.AddRange(
                    type.GetFields(
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.GetField |
                        BindingFlags.DeclaredOnly));
            }
            return fields;
        }

        private static FieldSerializer CreateFieldSerializer(FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (fieldInfo.FieldType == typeof(bool))
                return new ValueFieldSerializer<bool>(fieldInfo, (output, value) => output.WriteBool(value));

            if (fieldInfo.FieldType == typeof(int))
                return new ValueFieldSerializer<int>(fieldInfo, (output, value) => output.WriteInt(value));

            if (fieldInfo.FieldType == typeof(long))
                return new ValueFieldSerializer<long>(fieldInfo, (output, value) => output.WriteLong(value));

            if (fieldInfo.FieldType == typeof(double))
                return new ValueFieldSerializer<double>(fieldInfo, (output, value) => output.WriteDouble(value));

            if (fieldInfo.FieldType == typeof(DateTime))
                return new ValueFieldSerializer<DateTime>(fieldInfo, (output, value) => output.WriteDate(value));

            if (fieldInfo.FieldType == typeof(string))
                return new ValueFieldSerializer<string>(fieldInfo, (output, value) => output.WriteString(value));

            return new DefaultFieldSerializer(fieldInfo);
        }
    }
}