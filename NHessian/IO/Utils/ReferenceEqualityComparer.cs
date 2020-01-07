using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NHessian.IO.Utils
{
    /// <summary>
    /// Uses reference equality / hashCode (ignores overrides)
    /// with the exception of enum values. Enum values use regular
    /// equality comparison.
    /// </summary>
    internal class ReferenceEqualityComparer : EqualityComparer<object>
    {
        private static IEqualityComparer<object> _defaultComparer;

        public new static IEqualityComparer<object> Default => _defaultComparer ?? (_defaultComparer = new ReferenceEqualityComparer());

        /// <inheritdoc/>
        public override bool Equals(object x, object y)
        {
            // reference comparisons do not work with enums
            if (x?.GetType().IsEnum ?? false)
                return x.Equals(y);

            return ReferenceEquals(x, y);
        }

        /// <inheritdoc/>
        public override int GetHashCode(object obj)
        {
            if (obj?.GetType().IsEnum ?? false)
                return obj.GetHashCode();

            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}