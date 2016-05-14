using System.Collections.Generic;
using System.Linq;

namespace Polemo.NetCore.Node.Common
{
    public static class DictionaryHelper
    {
        public static Dictionary<string, string> Update(this Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            foreach (var valuePair in dict2)
            {
                var key = valuePair.Key;
                if (dict1.Keys.Contains(key))
                    dict1[key] = valuePair.Value;
            }
            return dict1;
        }
    }
}
