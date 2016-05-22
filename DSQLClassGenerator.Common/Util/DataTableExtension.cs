using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DSQLClassGenerator.Util
{
    public static class DataTableExtension
    {
        public static IEnumerable<IDictionary<string, object>> ToDictionary(this DataTable dt)
        {
            return dt.Rows.Cast<DataRow>()
                .Select(dr =>
                {
                    return dt.Columns.Cast<DataColumn>()
                        .Select(dc =>
                        {
                            return new { name = dc.ColumnName, value = dr[dc] };
                        })
                        .ToDictionary(x => x.name, x => x.value);
                        ;
                });
        }
    }
}
