using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace DSQLClassGenerator
{
    public interface ISchemaGetter
    {
        /// <summary>
        /// interface for tableinfo getter
        /// </summary>
        /// <param name="targetSchemas">target schemas.if null or empty,all schemas are targetted.</param>
        /// <param name="con">connection to database</param>
        /// <returns>table definitions</returns>
        IEnumerable<TableInfo> Get(IEnumerable<string> targetSchemas, DbConnection con);
    }
}
