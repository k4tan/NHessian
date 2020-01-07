using System;
using System.Collections;
using System.Collections.Concurrent;

namespace NHessian.IO.Serialization
{
    internal class Serializer
    {
        private static readonly DictionarySerializer _dictSerializer = new DictionarySerializer();
        private static readonly ListSerializer _listSerializer = new ListSerializer();

        private static readonly ConcurrentDictionary<Type, MapSerializer> _objSerializers
            = new ConcurrentDictionary<Type, MapSerializer>();

        private readonly HessianOutput _output;
        private readonly TypeBindings _typeBindings;

        public Serializer(HessianOutput output, TypeBindings typeBindings = null)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _typeBindings = typeBindings;
        }

        public void WriteList(IEnumerable list)
        {
            var customTypeName = _typeBindings?.TypeToTypeString(list.GetType());
            _listSerializer.Serialize(_output, list, customTypeName);
        }

        public void WriteMap(object obj)
        {
            var objType = obj.GetType();

            var customTypeName = _typeBindings?.TypeToTypeString(objType);

            MapSerializer serializer;

            if (obj is IDictionary)
            {
                serializer = _dictSerializer;
            }
            else if (_objSerializers.TryGetValue(objType, out serializer)) { }
            else
            {
                if (obj is Exception)
                    serializer = new ExceptionSerializer(objType);
                else if (objType.IsEnum)
                    serializer = new EnumSerializer(objType);
                else
                    serializer = new ObjectSerializer(objType);

                _objSerializers[objType] = serializer;
            }

            serializer.Serialize(_output, obj, customTypeName);
        }    }
}