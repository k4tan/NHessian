using System;
using System.Collections.Concurrent;

namespace NHessian.IO.Utils
{
    /// <summary>
    /// Resolves type names against types.
    /// </summary>
    public static class TypeResolver
    {
        private readonly static ConcurrentDictionary<string, Type> _cache
            = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Tries to resolve a type name against a <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// This methods searches in all loaded assemblies.
        /// <para>
        /// NOTE: Results are cached.
        /// </para>
        /// </remarks>
        /// <param name="fullTypeName">
        /// The full name of the type.
        /// </param>
        /// <param name="type">
        /// The type if resolved; otherwise null.
        /// </param>
        /// <returns>
        /// Returns true if the type was successfully resolved; otherwise false.
        /// </returns>
        public static bool TryResolve(string fullTypeName, out Type type)
        {
            if (fullTypeName is null)
                throw new ArgumentNullException(nameof(fullTypeName));

            if (_cache.TryGetValue(fullTypeName, out type))
                return true;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                {
                    _cache[fullTypeName] = type;
                    return true;
                }
            }

            return false;
        }
    }
}