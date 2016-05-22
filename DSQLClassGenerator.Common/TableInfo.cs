using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator
{
    using System.Data;
    /// <summary>
    /// information for table column.
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// column name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// database native type from sql(i.e. 'varchar')
        /// </summary>
        public string RawSqlDataType { get; set; }
        /// <summary>
        /// CLR database type
        /// </summary>
        public DbType DatabaseType { get; set; }
        /// <summary>
        /// .NET mapped type
        /// </summary>
        public Type CSType { get; set; }
        /// <summary>
        /// column size
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// primary key flag
        /// </summary>
        public bool IsPrimary { get; set; }
        /// <summary>
        /// autoincrement flag
        /// </summary>
        public bool IsAutoIncrement => Sequence != null;
        /// <summary>
        /// is nullable flag
        /// </summary>
        public bool IsNullable { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public int? NumericPrecisionRadix { get; set; }
        /// <summary>
        /// identity info for autoincrement
        /// </summary>
        public SequenceInfo Sequence { get; set; }
    }
    /// <summary>
    /// sql sequence info
    /// </summary>
    public class SequenceInfo
    {
        /// <summary>
        /// sequence name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// name of sequence owner schema
        /// </summary>
        public string Schema { get; set; }
    }
    /// <summary>
    /// table info
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// name of table owner schema
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// table name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// columns of table
        /// </summary>
        public ColumnInfo[] Columns { get; set; }
    }
}
