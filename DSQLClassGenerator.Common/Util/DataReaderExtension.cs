using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Util
{
    using System.Data;
    public static class DataReaderExtension
    {
        public static IEnumerable<IDictionary<string, object>> ToDictionaryArray(this IDataReader reader)
        {
            while (reader.Read())
            {
                var ret = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    ret[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader[i];
                }
                yield return ret;
            }
        }
    }
}
