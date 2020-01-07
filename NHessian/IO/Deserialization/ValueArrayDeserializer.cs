using System;
using System.Collections;
using System.Collections.Generic;

namespace NHessian.IO.Deserialization
{
    internal static class ValueArrayDeserializer
    {
        public static ValueArrayDeserializer<bool> Bool
            = new ValueArrayDeserializer<bool>(input => input.ReadBool());

        public static ValueArrayDeserializer<DateTime> Date
            = new ValueArrayDeserializer<DateTime>(input => input.ReadDate());

        public static ValueArrayDeserializer<double> Double
            = new ValueArrayDeserializer<double>(input => input.ReadDouble());

        public static ValueArrayDeserializer<int> Int
           = new ValueArrayDeserializer<int>(input => input.ReadInt());

        public static ValueArrayDeserializer<long> Long
            = new ValueArrayDeserializer<long>(input => input.ReadLong());

        public static ValueArrayDeserializer<string> String
            = new ValueArrayDeserializer<string>(input => input.ReadString());
    }

    internal sealed class ValueArrayDeserializer<T> : ListDeserializer
    {
        private readonly Func<HessianInput, T> valueReader;

        public ValueArrayDeserializer(Func<HessianInput, T> valueReader)
            : base(typeof(T[]))
        {
            this.valueReader = valueReader ?? throw new ArgumentNullException(nameof(valueReader));
        }

        public sealed override IList CreateList(int len) => new T[len];

        public sealed override void PopulateList(HessianInput input, IList list, int length)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (list is null) throw new ArgumentNullException(nameof(list));

            if (length < 0)
                throw new ArgumentOutOfRangeException("length is less than zero.", nameof(length));

            var typedList = (IList<T>)list;

            for (int i = 0; i < length; i++)
                typedList[i] = valueReader(input);
        }
    }
}