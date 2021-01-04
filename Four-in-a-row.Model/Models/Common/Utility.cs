using System.Collections.Generic;

namespace Four_in_a_row.Models.Common
{
    public static class Utility
    {
        public static V TryGetValue<T, V>(this IDictionary<T, V> dic, T key)
        {
            if (dic?.ContainsKey(key) ?? false)
            {
                return dic[key];
            }
            return default(V);
        }
    }
}
