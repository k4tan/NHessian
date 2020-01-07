using System;
using System.Collections;

namespace NHessian.IO.Deserialization
{
    internal class ArrayDeserializer : ListDeserializer
    {
        public ArrayDeserializer(Type arrayType)
            : base(arrayType)
        {
            if (!arrayType.IsArray)
                throw new ArgumentException("arrayType must be an array type", nameof(arrayType));
        }

        public override IList CreateList(int length) => Array.CreateInstance(ElementType, length);

        public override void PopulateList(HessianInput input, IList list, int length)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (list is null) throw new ArgumentNullException(nameof(list));

            if (length < 0)
                throw new ArgumentOutOfRangeException("length is less than zero.", nameof(length));

            for (int i = 0; i < length; i++)
                list[i] = input.ReadObject(ElementType);
        }
    }
}