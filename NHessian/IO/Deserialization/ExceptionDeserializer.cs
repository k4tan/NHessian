using System;
using System.Reflection;

namespace NHessian.IO.Deserialization
{
    internal class ExceptionDeserializer : ObjectDeserializer
    {
        public ExceptionDeserializer(Type objectType)
            : base(objectType) { }

        public ExceptionDeserializer(ExceptionDeserializer parent, ClassDefinition definition)
            : base(parent, definition) { }

        public override MapDeserializer AsCompact(ClassDefinition definition)
        {
            return new ExceptionDeserializer(this, definition);
        }

        protected override FieldDeserializer CreateFieldDeserializer(FieldInfo fieldInfo)
        {
            if (fieldInfo.Name == "_innerException")
            {
                // innerException field is special as it can't contain cycles!
                // use specialized deserializer
                return new InnerExceptionFieldDeserializer(fieldInfo);
            }

            return base.CreateFieldDeserializer(fieldInfo);
        }

        protected override FieldDeserializer GetFieldDeserializer(string fieldName)
        {
            var originalName = fieldName;

            // java mappings
            switch (fieldName)
            {
                case "detailMessage":
                    fieldName = "_message";
                    break;

                case "cause":
                    fieldName = "_innerException";
                    break;
            }

            return base.GetFieldDeserializer(fieldName) ?? base.GetFieldDeserializer(originalName);
        }
    }
}