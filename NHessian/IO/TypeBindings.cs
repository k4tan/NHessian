using System;

namespace NHessian.IO
{
    /// <summary>
    /// <see cref="TypeBindings"/> allows it to bind type strings
    /// to specific types and the other way around.
    /// </summary>
    /// <remarks>
    /// Hessian doesn't really specify what remoted type strings look like.
    /// Type strings usually refer to an actuall <see cref="Type"/> but they don't have to.
    /// For example, java uses "[int" for int arrays.
    /// <para>
    /// Per default, NHessian will treat a remoted type string as the actual
    /// type name. To override this behavior for certain strings/types, derive
    /// from this class to specify the mappings.
    /// </para>
    /// <para>
    /// A default implementation for java is included (<see cref="JavaTypeBindings"/>)
    /// and can be used as the bases for custom java bindings.
    /// </para>
    /// </remarks>
    public abstract class TypeBindings
    {
        /// <summary>
        /// Gets the default java bindings. This is a shortcut for
        /// <see cref="JavaTypeBindings"/>.
        /// </summary>
        public static TypeBindings Java { get; } = new JavaTypeBindings();

        /// <summary>
        /// Returns a bound type for <paramref name="typeString"/>
        /// if a binding exists.
        /// </summary>
        /// <param name="typeString">
        /// The type string.
        /// </param>
        /// <returns>
        /// Returns the bound type if a binding exists; otherwise null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="typeString"/> is null.
        /// </exception>
        public abstract Type TypeStringToType(string typeString);

        /// <summary>
        /// Returns a bound type string for <paramref name="type"/>
        /// if a binding exists.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// Returns the bound type string if a binding exists; otherwise null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is null.
        /// </exception>
        public abstract string TypeToTypeString(Type type);
    }
}