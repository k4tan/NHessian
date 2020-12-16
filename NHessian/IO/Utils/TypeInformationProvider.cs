using System;

namespace NHessian.IO.Utils
{
    public static class TypeInformationProvider
    {
        private static ITypeInformationProvider _current = new DefaultTypeInformationProvider();

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
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}