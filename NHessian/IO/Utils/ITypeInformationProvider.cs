using System;
using System.Reflection;

namespace NHessian.IO.Utils
{
    public interface ITypeInformationProvider
    {
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
        bool TryGetTypeByName(string fullTypeName, out Type type);

        /// <summary>
        /// Gets all serializable fields for a given type.
        /// </summary>
        /// <remarks>
        /// All fields returned by this method will be serialized 
        /// into hessian binary data.
        /// </remarks>
        /// <param name="type">
        /// The type for which the serializable fields should be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is null.
        /// </exception>
        /// <returns>
        /// Returns an array of serializable fields.
        /// </returns>
        FieldInfo[] GetSerializableFields(Type type);

        /// <summary>
        /// Gets all deserializable fields for a given type.
        /// </summary>
        /// <remarks>
        /// All fields not(!) returned by this method will be ignored
        /// by the deserialized.
        /// </remarks>
        /// <param name="type">
        /// The type for which the deserializable fields should be returned.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is null.
        /// </exception>
        /// <returns>
        /// Returns an array of deserializable fields.
        /// </returns>
        FieldInfo[] GetDeserializableFields(Type type);
    }
}