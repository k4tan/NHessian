using System;
using System.Collections.Generic;

namespace NHessian.IO
{
    /// <summary>
    /// class-def as described here:
    /// http://hessian.caucho.com/doc/hessian-serialization.html#anchor28
    /// </summary>
    internal class ClassDefinition
    {
        public ClassDefinition(string typeName, IReadOnlyList<string> fieldNames)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            if (fieldNames is null)
                throw new ArgumentNullException(nameof(typeName));

            TypeName = typeName;
            FieldNames = fieldNames;
        }

        public IReadOnlyList<string> FieldNames { get; }
        public string TypeName { get; }
    }
}