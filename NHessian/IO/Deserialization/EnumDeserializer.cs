using System;

namespace NHessian.IO.Deserialization
{
    internal class EnumDeserializer : MapDeserializer
    {
        private bool isCompact = false;

        public EnumDeserializer(Type enumType)
            : base(enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must be an enum type", nameof(enumType));
        }

        private EnumDeserializer(Type enumType, ClassDefinition typeDef)
            : this(enumType)
        {
            isCompact = true;
        }

        public override MapDeserializer AsCompact(ClassDefinition typeDef)
        {
            return new EnumDeserializer(MapType, typeDef);
        }

        public override object CreateMap(HessianInput input)
        {
            if (!isCompact)
            {
                if (input.ReadString() != "name")
                    throw new Exception("Not a valid enum");
            }

            return Enum.Parse(MapType, input.ReadString());
        }

        public override void PopulateMap(HessianInput input, object map)
        {
        }
    }
}