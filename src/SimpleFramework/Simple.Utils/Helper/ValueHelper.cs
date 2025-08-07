using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Simple.Utils.Helper
{
    public class ValueHelper
    {
        public static bool IsNullOrEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value is string)
            {
                return string.IsNullOrEmpty(value.ToString());
            }
            if (value is Guid v)
            {
                return v == Guid.Empty;
            }
            if (value is Guid?)
            {
                var nullableGuid = (Guid?)value;
                return !nullableGuid.HasValue || nullableGuid.Value == Guid.Empty;
            }
            if (value is ICollection collection)
            {
                return collection.Count == 0;
            }
            if (value is IEnumerable enumerable)
            {
                return !enumerable.GetEnumerator().MoveNext();
            }
            return false;
        }
    }
}