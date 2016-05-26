using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.SqlServer
{
    using System.Data;
    using DSQLClassGenerator.Util;
    public class SqlServerSchemaGetter : ISchemaGetter
    {
        IEnumerable<Tuple<string, string>> GetTables(IEnumerable<string> targetSchemas, DbConnection con)
        {
            using (var dt = con.GetSchema("Tables"))
            {
                foreach (var dr in dt.Rows.Cast<DataRow>())
                {
                    var tname = (string)dr["table_name"];
                    if (targetSchemas != null && targetSchemas.Any() && targetSchemas.Any(x => tname == x))
                    {
                        continue;
                    }
                    yield return Tuple.Create((string)dr["table_schema"], tname);
                }
            }
        }
        IDictionary<string, IDictionary<string, object>> GetColmnsFromSchemaTable(string schema, string tableName, DbConnection con)
        {
            using (var cmd = con.CreateCommand())
            {
                // all I want is only column information,not row info
                cmd.CommandText = $"select * from {schema}.{tableName} where 1 = 2";
                using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    using (var dt = reader.GetSchemaTable())
                    {
                        return dt.ToDictionary().ToDictionary(x => x["ColumnName"].ToString());
                    }
                }
            }
        }
        IDictionary<string, string> GetColumnSqlTypes(string schema, string tableName, DbConnection con)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = string.Format("select COLUMN_NAME,DATA_TYPE from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA=@TABLE_SCHEMA and TABLE_NAME=@TABLE_NAME");
                cmd.AddParameter("TABLE_SCHEMA", schema);
                cmd.AddParameter("TABLE_NAME", tableName);
                using (var reader = cmd.ExecuteReader())
                {
                    var ret = new Dictionary<string, string>();
                    while (reader.Read())
                    {
                        ret[reader.GetString(0)] = reader.GetString(1);
                    }
                    return ret;
                }
            }
        }
        DbType SqlTypeToDbType(SqlDbType t)
        {
            switch(t)
            {
                case SqlDbType.BigInt:
                    return DbType.Int64;
                case SqlDbType.Binary:
                    return DbType.Binary;
                case SqlDbType.Bit:
                    return DbType.Boolean;
                case SqlDbType.Char:
                    return DbType.AnsiStringFixedLength;
                case SqlDbType.Date:
                    return DbType.Date;
                case SqlDbType.DateTime:
                    return DbType.DateTime;
                case SqlDbType.DateTime2:
                    return DbType.DateTime2;
                case SqlDbType.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDbType.Decimal:
                    return DbType.Decimal;
                case SqlDbType.Float:
                    return DbType.Double;
                case SqlDbType.Image:
                    return DbType.Binary;
                case SqlDbType.Int:
                    return DbType.Int32;
                case SqlDbType.Money:
                    return DbType.VarNumeric;
                case SqlDbType.NChar:
                    return DbType.StringFixedLength;
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                    return DbType.String;
                case SqlDbType.Real:
                    return DbType.Double;
                case SqlDbType.SmallDateTime:
                    return DbType.DateTime;
                case SqlDbType.SmallInt:
                    return DbType.Int16;
                case SqlDbType.SmallMoney:
                    return DbType.VarNumeric;
                case SqlDbType.Text:
                    return DbType.AnsiString;
                case SqlDbType.Time:
                    return DbType.Time;
                case SqlDbType.Timestamp:
                    return DbType.DateTimeOffset;
                case SqlDbType.TinyInt:
                    return DbType.SByte;
                case SqlDbType.UniqueIdentifier:
                    return DbType.Guid;
                case SqlDbType.VarBinary:
                    return DbType.Binary;
                case SqlDbType.VarChar:
                    return DbType.AnsiString;
                default:
                    return DbType.String;
            }
        }
        IEnumerable<ColumnInfo> GetColumns(string schema, string tableName, DbConnection con)
        {
            var schemaTables = GetColmnsFromSchemaTable(schema, tableName, con);
            var typeNames = GetColumnSqlTypes(schema, tableName, con);
            foreach (var column in schemaTables)
            {
                foreach (var row in column.Value)
                {
                    System.Diagnostics.Trace.WriteLine($"{row.Key} = {row.Value}");
                }
                var ci = new ColumnInfo();
                ci.CSType = column.Value["DataType"] as Type;
                ci.RawSqlDataType = column.Value["DataTypeName"].ToString();
                ci.IsPrimary = (bool)column.Value["IsKey"];
                ci.IsNullable = (bool)column.Value["AllowDBNull"];
                if ((bool)column.Value["IsAutoIncrement"])
                {
                    ci.Sequence = new SequenceInfo();
                }
                ci.DatabaseType = SqlTypeToDbType((SqlDbType)column.Value["NonVersionedProviderType"]);
                ci.Name = column.Key;
                if (column.Value["NumericPrecision"] != null)
                {
                    var v = (short)column.Value["NumericPrecision"];
                    ci.NumericPrecision = v == 255 ? -1 : v;
                }
                if (column.Value["NumericScale"] != null)
                {
                    short v = (short)column.Value["NumericScale"];
                    ci.NumericScale = v == 255 ? -1 : v;
                }
                if (column.Value["ColumnSize"] != null)
                {
                    ci.Size = (int)column.Value["ColumnSize"];
                }
                yield return ci;
            }
        }
        public IEnumerable<TableInfo> Get(IEnumerable<string> targetSchemas, DbConnection con)
        {
            foreach (var table in GetTables(targetSchemas, con).ToArray())
            {
                var ti = new TableInfo();
                ti.Schema = table.Item1;
                ti.Name = table.Item2;
                ti.Columns = GetColumns(table.Item1, table.Item2, con).ToArray();
                yield return ti;
            }
        }
    }
}
