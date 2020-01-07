using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NHessian.IO.Deserialization
{
    internal class ValueCollectionDeserializer<T> : ListDeserializer
    {
        private readonly Func<IList> _activator;
        private readonly Func<HessianInput, T> _valueReader;

        public ValueCollectionDeserializer(Type collectionType, Func<HessianInput, T> valueReader) : base(collectionType)
        {
            _activator = (Func<IList>)Expression.Lambda(typeof(Func<IList>), Expression.New(collectionType)).Compile();
            _valueReader = valueReader;
        }

        public override IList CreateList(int len) => _activator();

        public override void PopulateList(HessianInput input, IList list, int len)
        {
            var typedCollection = (ICollection<T>)list;

            if (len >= 0)
            {
                for (var i = 0; i < len; i++)
                    typedCollection.Add(_valueReader(input));
            }
            else
            {
                while (!input.IsEnd())
                    typedCollection.Add(_valueReader(input));
            }
        }
    }
}