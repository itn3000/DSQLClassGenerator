using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Util
{
    using System.Data;
    public static class DbCommandExtension
    {
        public static IDbCommand AddParameter(this IDbCommand cmd, string name, object value, DbType? t = null, int? size = null)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            if(t.HasValue)
            {
                p.DbType = t.Value;
            }
            if (size.HasValue)
            {
                p.Size = size.Value;
            }
            cmd.Parameters.Add(p);
            return cmd;
        }
    }
}
