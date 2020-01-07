using System;
using System.Collections;
using System.Linq.Expressions;

namespace NHessian.IO.Deserialization
{
    internal class CollectionDeserializer : ListDeserializer
    {
        private readonly Func<IList> _activator;

        public CollectionDeserializer(Type collectionType)
            : base(collectionType)
        {
            _activator = (Func<IList>)Expression.Lambda(typeof(Func<IList>), Expression.New(collectionType)).Compile();
        }

        public override IList CreateList(int len) => _activator();

        public override void PopulateList(HessianInput input, IList list, int len)
        {
            if (len >= 0)
            {
                for (var i = 0; i < len; i++)
                    list.Add(input.ReadObject(ElementType));
            }
            else
            {
                while (!input.IsEnd())
                    list.Add(input.ReadObject(ElementType));
            }
        }
    }
}