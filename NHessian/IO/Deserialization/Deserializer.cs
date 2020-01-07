using NHessian.IO.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NHessian.IO.Deserialization
{
    internal class Deserializer
    {
        private static readonly ConcurrentDictionary<Type, ListDeserializer> _listDeserializers
            = new ConcurrentDictionary<Type, ListDeserializer>();

        private static readonly ConcurrentDictionary<Type, MapDeserializer> _mapDeserializers
            = new ConcurrentDictionary<Type, MapDeserializer>();

        private readonly Dictionary<ClassDefinition, MapDeserializer> _compactMapDeserializers
            = new Dictionary<ClassDefinition, MapDeserializer>(ReferenceEqualityComparer.Default);

        private readonly HessianInput _input;
        private readonly Dictionary<string, Type> _localTypeCache = new Dictionary<string, Type>();
        private readonly TypeBindings _typeBindings;

        public Deserializer(HessianInput input, TypeBindings typeBindings = null)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _typeBindings = typeBindings;
        }

        public object ReadList(Type expectedType)
        {
            var hasEnd = _input.ReadListStart(out var remoteTypeName, out var length);

            var remoteType = string.IsNullOrWhiteSpace(remoteTypeName)
                ? null
                : ResolveTypeString(remoteTypeName);

            var finalListType = ResolveFinalListType(expectedType, remoteType, length >= 0);
            var deserializer = GetListDeserializer(finalListType);

            var list = deserializer.CreateList(length);
            _input.AddRef(list);
            deserializer.PopulateList(_input, list, length);

            if (hasEnd)
                _input.ReadEnd();

            return list;
        }

        public object ReadMap(Type expectedType)
        {
            var hasEnd = _input.ReadMapStart(out var remoteTypeName, out var typeDef);

            var remoteType = string.IsNullOrWhiteSpace(remoteTypeName)
                ? null
                : ResolveTypeString(remoteTypeName);

            var finalMapType = ResolveFinalMapType(expectedType, remoteType);
            var deserializer = GetMapDeserializer(finalMapType, typeDef);

            var map = deserializer.CreateMap(_input);
            _input.AddRef(map);
            deserializer.PopulateMap(_input, map);

            if (hasEnd)
                _input.ReadEnd();

            return map;
        }

        private static ListDeserializer CreateListDeserializer(Type listType)
        {
            if (listType.IsArray)
            {
                if (listType == typeof(int[]))
                    return ValueArrayDeserializer.Int;

                if (listType == typeof(long[]))
                    return ValueArrayDeserializer.Long;

                if (listType == typeof(double[]))
                    return ValueArrayDeserializer.Double;

                if (listType == typeof(DateTime[]))
                    return ValueArrayDeserializer.Date;

                if (listType == typeof(bool[]))
                    return ValueArrayDeserializer.Bool;

                if (listType == typeof(string[]))
                    return ValueArrayDeserializer.String;

                return new ArrayDeserializer(listType);
            }

            if (typeof(ICollection<int>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<int>(listType, input => input.ReadInt());

            if (typeof(ICollection<long>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<long>(listType, input => input.ReadLong());

            if (typeof(ICollection<double>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<double>(listType, input => input.ReadDouble());

            if (typeof(ICollection<DateTime>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<DateTime>(listType, input => input.ReadDate());

            if (typeof(ICollection<bool>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<bool>(listType, input => input.ReadBool());

            if (typeof(ICollection<string>).IsAssignableFrom(listType))
                return new ValueCollectionDeserializer<string>(listType, input => input.ReadString());

            return new CollectionDeserializer(listType);
        }

        private static MapDeserializer CreateMapDeserializer(Type mapType)
        {
            if (mapType.IsEnum)
                return new EnumDeserializer(mapType);

            if (typeof(IDictionary).IsAssignableFrom(mapType))
                return new DictionaryDeserializer(mapType);

            if (typeof(Exception).IsAssignableFrom(mapType))
                return new ExceptionDeserializer(mapType);

            return new ObjectDeserializer(mapType);
        }

        private static ListDeserializer GetListDeserializer(Type listType)
        {
            if (listType is null) throw new ArgumentNullException(nameof(listType));

            if (!_listDeserializers.TryGetValue(listType, out var deserializer))
                _listDeserializers[listType] = deserializer = CreateListDeserializer(listType);

            return deserializer;
        }

        private static Type ResolveFinalListType(Type expectedType, Type remoteType, bool hasFixedLength)
        {
            if (remoteType != null && (expectedType == null || expectedType.IsAssignableFrom(remoteType)))
                return remoteType;

            if (expectedType != null && typeof(IList).IsAssignableFrom(expectedType))
                return expectedType;

            return hasFixedLength ? typeof(object[]) : typeof(List<object>);
        }

        private static Type ResolveFinalMapType(Type expectedType, Type remoteType)
        {
            if (remoteType != null && (expectedType == null || expectedType.IsAssignableFrom(remoteType)))
                return remoteType;

            if (expectedType != null)
                return expectedType;

            return typeof(Dictionary<object, object>);
        }

        private MapDeserializer GetMapDeserializer(Type mapType, ClassDefinition typeDef)
        {
            MapDeserializer deserializer;
            if (typeDef != null)
            {
                if (!_compactMapDeserializers.TryGetValue(typeDef, out deserializer))
                {
                    deserializer = GetMapDeserializer(mapType, null).AsCompact(typeDef);
                    _compactMapDeserializers[typeDef] = deserializer;
                }
            }
            else
            {
                if (!_mapDeserializers.TryGetValue(mapType, out deserializer))
                    _mapDeserializers[mapType] = deserializer = CreateMapDeserializer(mapType);
            }

            return deserializer;
        }

        private Type ResolveTypeString(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
                return null;

            // use local cache to improve lookup performance
            if (_localTypeCache.TryGetValue(typeString, out var type))
                return type;

            // check bindings
            type = _typeBindings?.TypeStringToType(typeString);

            if (type == null)
                TypeResolver.TryResolve(typeString, out type);

            _localTypeCache.Add(typeString, type);
            return type;
        }
    }
}