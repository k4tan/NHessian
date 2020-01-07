using System;

namespace NHessian.IO.Serialization
{
    internal class EnumSerializer : MapSerializer
    {
        private static string[] _fieldNames;
        private readonly Type _type;

        public EnumSerializer(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _fieldNames = new string[] { "name" };
        }

        public override void Serialize(HessianOutput output, object map, string customTypeName)
        {
            var typeName = customTypeName ?? _type.FullName;
            var value = Enum.GetName(_type, map);

            var defIdx = output.WriteMapDefinition(typeName, _fieldNames);
            if (defIdx < 0)
            {
                output.WriteMapStart(typeName);
                output.WriteString("name");
                output.WriteString(value);
                output.WriteMapEnd();
            }
            else
            {
                output.WriteMapStart(defIdx);
                output.WriteString(value);
            }
        }
    }
}