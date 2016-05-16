using System.Collections.Generic;
using System.Linq;

namespace Pomelo.NetCore.Node.Common
{
    public static class DictionaryHelper
    {
        public static Dictionary<string, string> UpdateBy(this Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            if (dict2 == null)
                return dict1;

            foreach (var valuePair in dict2)
            {
                var key = valuePair.Key;
                if (dict1.Keys.Contains(key))
                {
                    dict1[key] = valuePair.Value;
                }
                else
                {
                    dict1.Add(key, valuePair.Value);
                }
            }
            return dict1;
        }
    }
}
