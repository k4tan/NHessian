using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NHessian.IO.Utils
{
    /// <summary>
    /// Default implementation of <see cref="ITypeInformationProvider"/>.
    /// This class caches all results for faster access.
    /// </summary>
    /// <remarks>
    /// Derive from this class to override behavior for the different methods.
    /// All results from the overrides will still be cached.
    /// </remarks>
    public class DefaultTypeInformationProvider : ITypeInformationProvider
    {
        private readonly ConcurrentDictionary<string, Type> _typeCache
            = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentDictionary<Type, FieldInfo[]> _serializableFieldsCache
            = new ConcurrentDictionary<Type, FieldInfo[]>();

        private readonly ConcurrentDictionary<Type, FieldInfo[]> _deserializableFieldsCache
            = new ConcurrentDictionary<Type, FieldInfo[]>();



        public bool TryGetTypeByName(string fullTypeName, out Type type)
        {
            if (fullTypeName is null)
                throw new ArgumentNullException(nameof(fullTypeName));

            if (_typeCache.TryGetValue(fullTypeName, out type))
                return true;

            type = GetTypeByNameOverride(fullTypeName);
            if (type != null)
            {
                _typeCache[fullTypeName] = type;
                return true;
            }
            return false;
        }

        public FieldInfo[] GetSerializableFields(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (_serializableFieldsCache.TryGetValue(type, out var fields))
                return fields;

            fields = GetSerializableFieldsOverride(type) ?? Array.Empty<FieldInfo>();

            return _serializableFieldsCache[type] = fields;
        }

        public FieldInfo[] GetDeserializableFields(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (_deserializableFieldsCache.TryGetValue(type, out var fields))
                return fields;

            fields = GetDeserializableFieldsOverride(type) ?? Array.Empty<FieldInfo>();

            return _deserializableFieldsCache[type] = fields;
        }



        protected virtual Type GetTypeByNameOverride(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        protected virtual FieldInfo[] GetSerializableFieldsOverride(Type type)
        {
            var fieldsList = new List<FieldInfo>();
            for (; type != null; type = type.BaseType)
            {
                fieldsList.AddRange(
                    type.GetFields(
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.GetField |
                        BindingFlags.DeclaredOnly)
                    .Where(f => !f.IsNotSerialized));
            }
            return fieldsList.ToArray();
        }

        protected virtual FieldInfo[] GetDeserializableFieldsOverride(Type type)
        {
            var fieldsList = new List<FieldInfo>();
            for (; type != null; type = type.BaseType)
            {
                fieldsList.AddRange(
                    type.GetFields(
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.GetField |
                        BindingFlags.DeclaredOnly)
                    .Where(f => !f.IsNotSerialized && !f.IsLiteral && !f.IsInitOnly));
            }
            return fieldsList.ToArray();
        }
    }
}