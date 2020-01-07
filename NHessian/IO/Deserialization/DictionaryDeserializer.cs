using System;
using System.Collections;
using System.Linq.Expressions;

namespace NHessian.IO.Deserialization
{
    internal class DictionaryDeserializer : MapDeserializer
    {
        private readonly Func<IDictionary> _activator;

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

        private Type KeyType { get; }
        private Type ValueType { get; }

        public override MapDeserializer AsCompact(ClassDefinition typeDef)
        {
            throw new NotSupportedException("There is no compact version of a dictionary");
        }

        public override object CreateMap(HessianInput input) => _activator();

        public override void PopulateMap(HessianInput input, object map)
        {
            var dict = (IDictionary)map;

            // NOTE potential boxing here; probably not worth figuring out how to avoid
            while (!input.IsEnd())
                dict.Add(input.ReadObject(KeyType), input.ReadObject(ValueType));
        }
    }
}