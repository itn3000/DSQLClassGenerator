﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator
{
    using System.Data.Common;
    static class SchemaGetterFactory
    {
        public static ISchemaGetter Create(DbConnection con)
        {
            var t = con.GetType();
            if (t.Name.Contains("Npgsql"))
            {
                return new Postgres.NpgsqlSchemaGetter();
            }
            else
            {
                return null;
            }
        }
    }
}
