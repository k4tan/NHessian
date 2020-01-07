using System;
using System.Collections;
using System.Linq;

namespace NHessian.IO.Deserialization
{
    internal abstract class ListDeserializer
    {
        protected ListDeserializer(Type listType)
        {
            if (listType is null) throw new ArgumentNullException(nameof(listType));

            if (!typeof(IList).IsAssignableFrom(listType))
                throw new ArgumentException("Type is not a list type", nameof(listType));

            ListType = listType;
            ElementType = GetElementType(listType);
        }

        protected Type ElementType { get; }
        protected Type ListType { get; }

        public abstract IList CreateList(int len);

        public abstract void PopulateList(HessianInput input, IList list, int len);

        private static Type GetElementType(Type listType)
        {
            if (listType.IsArray)
            {
                if (listType.HasElementType)
                    return listType.GetElementType();
            }
            else if (listType.IsGenericType)
                return listType.GetGenericArguments().FirstOrDefault();

            return typeof(object);
        }
    }
}