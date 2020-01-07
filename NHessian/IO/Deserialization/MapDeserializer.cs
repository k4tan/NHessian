using System;

namespace NHessian.IO.Deserialization
{
    internal abstract class MapDeserializer
    {
        protected MapDeserializer(Type mapType)
        {
            MapType = mapType ?? throw new ArgumentNullException(nameof(mapType));
        }

        public Type MapType { get; }

        public abstract MapDeserializer AsCompact(ClassDefinition typeDef);

        public abstract object CreateMap(HessianInput input);

        public abstract void PopulateMap(HessianInput input, object map);
    }
}