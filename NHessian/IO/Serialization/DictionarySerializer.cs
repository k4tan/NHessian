using System.Collections;

namespace NHessian.IO.Serialization
{
    internal class DictionarySerializer : MapSerializer
    {
        public override void Serialize(HessianOutput output, object map, string customTypeName)
        {
            var dict = (IDictionary)map;
            var typeName = customTypeName;

            output.WriteMapStart(typeName);

            foreach (var key in dict.Keys)
            {
                output.WriteObject(key);
                output.WriteObject(dict[key]);
            }

            output.WriteMapEnd();
        }
    }
}