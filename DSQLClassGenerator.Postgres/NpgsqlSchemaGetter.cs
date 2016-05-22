using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSQLClassGenerator.Util;
using System.Data;
using System.Text.RegularExpressions;

namespace DSQLClassGenerator.Postgres
{
    public class NpgsqlSchemaGetter : ISchemaGetter
    {
        IEnumerable<Tuple<string,string>> GetTables(IEnumerable<string> targetSchemas, DbConnection con)
        {
            var isAnyTable = targetSchemas != null && targetSchemas.Count() != 0 ? false : true;
            using (var tableData = con.GetSchema("Tables"))
            {
                var tableDataList = tableData.ToDictionary();
                foreach (var row in tableDataList)
                {
                    System.Diagnostics.Trace.WriteLine($"{row["table_schema"]},{row["table_name"]}");
                    if (row["table_schema"].ToString() == "information_schema" || row["table_schema"].ToString() == "pg_catalog")
                    {
                        // システムテーブルは処理しない
                        continue;
                    }
                    if (isAnyTable || targetSchemas.Any(x => x == row["table_schema"].ToString()))
                    {
                        // スキーマチェックをパスしたら値を返す
                        yield return Tuple.Create(row["table_schema"].ToString(), row["table_name"].ToString());
                    }
                }
            }
        }
        DbType PgsqlTypeToDbType(string pgtype)
        {
            switch (pgtype.ToLower())
            {
                case "character varying":
                case "varchar":
                case "text":
                case "national character varying":
                    return DbType.String;
                case "char":
                case "national character":
                    return DbType.StringFixedLength;
                case "bigint":
                case "int8":
                case "serial8":
                case "bigserial":
                    return DbType.Int64;
                case "int":
                case "integer":
                case "int4":
                case "serial":
                case "serial4":
                    return DbType.Int32;
                case "money":
                    return DbType.Currency;
                case "real":
                case "float4":
                case "double":
                case "double precision":
                case "float8":
                    return DbType.Double;
                case "date":
                    return DbType.Date;
                case "time":
                    return DbType.Time;
                case "timestamp":
                    return DbType.DateTimeOffset;
                case "interval":
                    return DbType.DateTimeOffset;
                case "bytea":
                    return DbType.Binary;
                case "boolean":
                    return DbType.Boolean;
                case "smallint":
                case "int2":
                    return DbType.Int16;
                case "uuid":
                    return DbType.Guid;
                case "numeric":
                case "decimal":
                    return DbType.Decimal;
                default:
                    return DbType.Object;
            }
        }
        IEnumerable<SequenceInfo> GetSequences(DbConnection con, string schema)
        {
            var sql = "select sequence_schema,sequence_name from information_schema.sequences";
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.AddParameter("sequence_schema", schema);
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        yield return new SequenceInfo()
                        {
                            Schema = reader.GetString(0),
                            Name = reader.GetString(1)
                        };
                    }
                }
            }
        }
        ColumnInfo CreateColumnInfo(IDataReader reader, IEnumerable<SequenceInfo> sequences)
        {
            var ci = new ColumnInfo()
            {
                Name = reader.GetString(reader.GetOrdinal("column_name"))
                ,
                RawSqlDataType = reader.GetString(reader.GetOrdinal("data_type"))
            };
            ci.DatabaseType = PgsqlTypeToDbType(ci.RawSqlDataType);
            ci.CSType = ci.DatabaseType.ToType();
            ci.IsNullable = reader.GetString(reader.GetOrdinal("is_nullable")) == "YES";
            if (!reader.IsDBNull(reader.GetOrdinal("column_default")))
            {
                var m = Regex.Match(reader.GetString(reader.GetOrdinal("column_default")), @"^nextval.'([^']+)'.");
                if (m.Success)
                {
                    var seqname = m.Groups[1].Value;
                    ci.Sequence = sequences.FirstOrDefault(x => x.Name == seqname);
                }
            }
            if (!reader.IsDBNull(reader.GetOrdinal("constraint_type")))
            {
                ci.IsPrimary = reader.GetString(reader.GetOrdinal("constraint_type")) == "PRIMARY KEY";
            }
            else
            {
                ci.IsPrimary = false;
            }
            if (!reader.IsDBNull(reader.GetOrdinal("character_maximum_length")))
            {
                ci.Size = reader.GetInt32(reader.GetOrdinal("character_maximum_length"));
            }
            if(!reader.IsDBNull(reader.GetOrdinal("numeric_precision")))
            {
                ci.NumericPrecision = reader.GetInt32(reader.GetOrdinal("numeric_precision"));
            }
            if (!reader.IsDBNull(reader.GetOrdinal("numeric_precision_radix")))
            {
                ci.NumericPrecision = reader.GetInt32(reader.GetOrdinal("numeric_precision_radix"));
            }
            if (!reader.IsDBNull(reader.GetOrdinal("numeric_scale")))
            {
                ci.NumericScale = reader.GetInt32(reader.GetOrdinal("numeric_scale"));
            }
            return ci;
        }
        IEnumerable<ColumnInfo> GetColumnInfo(DbConnection con, string schema, string tableName, IEnumerable<SequenceInfo> sequences)
        {
            var sql = string.Format(@"
select ic.*,ikcu.constraint_name,itc.constraint_type from information_schema.columns as ic
  left join information_schema.key_column_usage as ikcu 
    on ic.table_name=ikcu.table_name and ic.column_name=ikcu.column_name
  left join information_schema.table_constraints as itc
    on itc.constraint_name=ikcu.constraint_name
  where ic.table_schema=@table_schema and ic.table_name=@table_name
");
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.AddParameter("table_schema", schema);
                cmd.AddParameter("table_name", tableName);
                var ret = new List<ColumnInfo>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.Add(CreateColumnInfo(reader, sequences));
                    }
                }
                return ret;
            }
        }
        public IEnumerable<TableInfo> Get(IEnumerable<string> targetSchemas, DbConnection con)
        {
            var isAnyTable = targetSchemas != null && targetSchemas.Count() != 0 ? false : true;
            var tables = GetTables(targetSchemas, con).ToArray();
            var schemas = tables.Select(x => x.Item1).Distinct();
            var sequences = schemas.SelectMany(x => GetSequences(con, x)).ToArray();
            foreach (var table in tables)
            {
                var ti = new TableInfo();
                ti.Name = table.Item2;
                ti.Schema = table.Item1;
                ti.Columns = GetColumnInfo(con, ti.Schema, ti.Name, sequences).ToArray();
                yield return ti;
            }
        }
    }
}
