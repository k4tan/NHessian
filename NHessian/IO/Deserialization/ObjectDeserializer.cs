using NHessian.IO.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    internal class ObjectDeserializer : MapDeserializer
    {
        private readonly Func<object> _activator;

        private readonly FieldDeserializer[] _definedFields;
        private readonly ConcurrentDictionary<string, FieldDeserializer> _fieldDeserializers;

        public ObjectDeserializer(Type objectType)
            : base(objectType)
        {
            _activator = (Func<object>)Expression.Lambda(typeof(Func<object>), Expression.New(objectType)).Compile();
            _fieldDeserializers = new ConcurrentDictionary<string, FieldDeserializer>();
        }

        protected ObjectDeserializer(ObjectDeserializer parent, ClassDefinition definition)
            : base(parent.MapType)
        {
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            _activator = parent._activator;
            _fieldDeserializers = parent._fieldDeserializers;

            var fieldList = new FieldDeserializer[definition.FieldNames.Length];

            for (int i = 0; i < definition.FieldNames.Length; i++)
                fieldList[i] = GetFieldDeserializer(definition.FieldNames[i]);

            _definedFields = fieldList;
        }

        public override MapDeserializer AsCompact(ClassDefinition definition)
        {
            return new ObjectDeserializer(this, definition);
        }

        public override object CreateMap(HessianInput input) => _activator();

        public override void PopulateMap(HessianInput input, object map)
        {
            if (_definedFields != null)
            {
                // compact
                foreach (var fieldDeserializer in _definedFields)
                {
                    if (fieldDeserializer != null)
                        fieldDeserializer.PopulateField(input, map);
                    else
                        input.ReadObject(); // ignore value
                }
            }
            else
            {
                // read key / value tuples
                while (!input.IsEnd())
                {
                    var key = input.ReadObject();
                    if (key is string fieldName)
                    {
                        var fieldDeserializer = GetFieldDeserializer(fieldName);
                        if (fieldDeserializer != null)
                        {
                            fieldDeserializer.PopulateField(input, map);
                            continue; // next
                        }
                    }

                    input.ReadObject(); // ignore value
                }
            }
        }

        protected virtual FieldDeserializer CreateFieldDeserializer(FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
                throw new ArgumentNullException(nameof(fieldInfo));

            if (fieldInfo.FieldType == typeof(bool))
                return new ValueFieldDeserializer<bool>(fieldInfo, input => input.ReadBool());

            if (fieldInfo.FieldType == typeof(int))
                return new ValueFieldDeserializer<int>(fieldInfo, input => input.ReadInt());

            if (fieldInfo.FieldType == typeof(long))
                return new ValueFieldDeserializer<long>(fieldInfo, input => input.ReadLong());

            if (fieldInfo.FieldType == typeof(double))
                return new ValueFieldDeserializer<double>(fieldInfo, input => input.ReadDouble());

            if (fieldInfo.FieldType == typeof(DateTime))
                return new ValueFieldDeserializer<DateTime>(fieldInfo, input => input.ReadDate());

            if (fieldInfo.FieldType == typeof(string))
                return new ValueFieldDeserializer<string>(fieldInfo, input => input.ReadString());

            return new DefaultFieldDeserializer(fieldInfo);
        }

        protected virtual FieldDeserializer GetFieldDeserializer(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            if (!_fieldDeserializers.TryGetValue(fieldName, out var deserializer))
            {
                var fieldInfo = GetField(fieldName);
                if (!(fieldInfo is null))
                {
                    _fieldDeserializers[fieldName] = deserializer = CreateFieldDeserializer(fieldInfo);
                }
            }

            return deserializer;
        }

        private FieldInfo GetField(string fieldName)
        {
            var fields = TypeInformationProvider.Default.GetDeserializableFields(MapType);

            for (int i = 0; i < fields.Length; i++)
                if (fields[i].Name == fieldName)
                    return fields[i];

            return null;
        }
    }
}