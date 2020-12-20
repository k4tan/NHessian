using System;

namespace NHessian.IO.Utils
{
    /// <summary>
    /// Gives access to the default <see cref="ITypeInformationProvider"/>
    /// used by NHessian.
    /// </summary>
    public static class TypeInformationProvider
    {
        private static ITypeInformationProvider _default = new DefaultTypeInformationProvider();

        /// <summary>
        /// Gets the <see cref="ITypeInformationProvider"/> instance that
        /// is used during de-/serialization for type information gathering.
        /// </summary>
        /// <remarks>
        /// You can set a different implementation to customize type information
        /// used by hessian.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If value is null.
        /// </exception>
        public static ITypeInformationProvider Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}