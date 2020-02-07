using System;
using System.Collections;
using System.Collections.Generic;

namespace NHessian.IO.Serialization
{
    internal class ListSerializer
    {
        public void Serialize(HessianOutput output, IEnumerable list, string customTypeName)
        {
            // not much use to serializer the concrete list type
            var typeName = customTypeName;

            var length = list is ICollection collection ? collection.Count : -1;

            var hasEnd = output.WriteListStart(length, typeName);

            switch (list)
            {
                //NOTE enum arrays get "falsly" matched by IList<int> and IEnumerable<int>
                case IList<int> intList when !IsEnumArray(list.GetType()):
                    for (int i = 0; i < intList.Count; i++) output.WriteInt(intList[i]);
                    break;

                case IEnumerable<int> intEnum when !IsEnumArray(list.GetType()):
                    foreach (var item in intEnum) output.WriteInt(item);
                    break;

                case IList<long> longList:
                    for (int i = 0; i < longList.Count; i++) output.WriteLong(longList[i]);
                    break;

                case IEnumerable<long> longEnum:
                    foreach (var item in longEnum) output.WriteLong(item);
                    break;

                case IList<double> doubleList:
                    for (int i = 0; i < doubleList.Count; i++) output.WriteDouble(doubleList[i]);
                    break;

                case IEnumerable<double> doubleEnum:
                    foreach (var item in doubleEnum) output.WriteDouble(item);
                    break;

                case IList<DateTime> dateList:
                    for (int i = 0; i < dateList.Count; i++) output.WriteDate(dateList[i]);
                    break;

                case IEnumerable<DateTime> dateEnum:
                    foreach (var item in dateEnum) output.WriteDate(item);
                    break;

                case IList<bool> boolList:
                    for (int i = 0; i < boolList.Count; i++) output.WriteBool(boolList[i]);
                    break;

                case IEnumerable<bool> boolEnum:
                    foreach (var item in boolEnum) output.WriteBool(item);
                    break;

                case IList objList:
                    for (int i = 0; i < objList.Count; i++) output.WriteObject(objList[i]);
                    break;

                default:
                    foreach (var item in list) output.WriteObject(item);
                    break;
            }

            if (hasEnd)
                output.WriteListEnd();
        }

        private static bool IsEnumArray(Type type)
        {
            return type.IsArray && (type.GetElementType()?.IsEnum ?? false);
        }
    }
}