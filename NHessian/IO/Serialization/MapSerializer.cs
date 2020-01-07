using System;

namespace NHessian.IO.Serialization
{
    internal abstract class MapSerializer
    {
        public abstract void Serialize(HessianOutput output, object map, string customTypeName);
    }
}