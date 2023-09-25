using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NHessian.IO.Deserialization
{
    internal class DictionaryDeserializer : MapDeserializer
    {
        private readonly Func<IDictionary> _activator;
        private readonly string[] _fieldNames;

        public DictionaryDeserializer(Type dictionaryType)
            : base(dictionaryType)
        {
            if (!typeof(IDictionary).IsAssignableFrom(dictionaryType))
                throw new ArgumentException("dictionaryType must be a dictionary type", nameof(dictionaryType));

            _activator = (Func<IDictionary>)Expression.Lambda(typeof(Func<IDictionary>), Expression.New(dictionaryType)).Compile();

            if (dictionaryType.IsGenericType)
            {
                var genericArgs = dictionaryType.GetGenericArguments();
                KeyType = genericArgs[0];
                ValueType = genericArgs[1];
            }
            else
            {
                KeyType = ValueType = typeof(object);
            }
        }

        protected DictionaryDeserializer(ClassDefinition definition)
            : base(typeof(Dictionary<object, object>))
        {
            // constructor for compact serialization
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            _activator = (Func<IDictionary>)Expression.Lambda(typeof(Func<IDictionary>), Expression.New(MapType)).Compile();

            KeyType = ValueType = typeof(string);

            _fieldNames = definition.FieldNames;
        }

        private Type KeyType { get; }
        private Type ValueType { get; }

        public override MapDeserializer AsCompact(ClassDefinition typeDef)
        {
            return new DictionaryDeserializer(typeDef);
        }

        public override object CreateMap(HessianInput input) => _activator();

        public override void PopulateMap(HessianInput input, object map)
        {
            var dict = (IDictionary)map;

            if (_fieldNames is not null)
            {
                // compact representation
                for (int i = 0; i < _fieldNames.Length; i++)
                {
                    dict.Add(_fieldNames[i], input.ReadObject(ValueType));
                }
            }
            else
            {
                // NOTE potential boxing here; probably not worth figuring out how to avoid
                while (!input.IsEnd())
                    dict.Add(input.ReadObject(KeyType), input.ReadObject(ValueType));
            }
        }
    }
}