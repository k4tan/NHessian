using System;

namespace NHessian.IO
{
    /// <summary>
    /// Adds bindings for java service endpoints.
    /// http://hessian.caucho.com/doc/hessian-java-binding-draft-spec.xtp
    /// Not all java bindings are currently included; only native hessian types
    /// (int, long, double, bool, string).
    /// Non native types (short, float, etc) are not currently included.
    /// </summary>
    public class JavaTypeBindings : TypeBindings
    {
        /// <inheritdoc/>
        public override Type TypeStringToType(string typeString)
        {
            if (typeString is null)
                throw new ArgumentNullException(nameof(typeString));

            switch (typeString)
            {
                case "[boolean":
                    return typeof(bool[]);

                case "[int":
                    return typeof(int[]);

                case "[long":
                    return typeof(long[]);

                case "[double":
                    return typeof(double[]);

                case "[string":
                    return typeof(string[]);
            }

            return null;
        }

        /// <inheritdoc/>
        public override string TypeToTypeString(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(bool[]))
                return "[boolean";

            if (type == typeof(int[]))
                return "[int";

            if (type == typeof(long[]))
                return "[long";

            if (type == typeof(double[]))
                return "[double";

            if (type == typeof(string[]))
                return "[string";

            return null;
        }
    }
}