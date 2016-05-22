using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Generator
{
    /// <summary>
    /// generating classes for DeclarativeSQL.
    /// </summary>
    public partial class ClassGeneratorTemplate
    {
        IEnumerable<TableInfo> Tables { get; }
        string Namespace { get; }
        bool OutputSchema { get; }
        IEnumerable<string> AdditionalNamespace { get; set; }
        NameConverter Converter { get; set; } = new NameConverter();
        Func<string, string> ConvertClassName;
        Func<string, string> ConvertFieldName;
        Func<string, Type, string, string> ConvertFieldTypeName;
        /// <summary>
        /// default constructor
        /// </summary>
        /// <remarks>if you generate from large number of tables,be careful of memory usage</remarks>
        /// <param name="tableInfo">table information list</param>
        /// <param name="ns">namespace of generated class</param>
        /// <param name="IsOutputSchema">should add the schema attribute</param>
        /// <param name="additionalNamespace">additional namespaces(added head of file)</param>
        /// <param name="converter">if you want to customize generated class name,you must pass the converter instance</param>
        public ClassGeneratorTemplate(IEnumerable<TableInfo> tableInfo, string ns, bool IsOutputSchema, IEnumerable<string> additionalNamespace = null, NameConverter converter = null)
        {
            Tables = tableInfo;
            Namespace = ns;
            this.OutputSchema = IsOutputSchema;
            AdditionalNamespace = additionalNamespace;
            var Converter = converter != null ? converter : new NameConverter();
            if (Converter.ConvertClassName != null)
            {
                ConvertClassName = Converter.ConvertClassName;
            }
            else
            {
                ConvertClassName = x => x;
            }
            if (Converter.ConvertMemberName != null)
            {
                ConvertFieldName = Converter.ConvertMemberName;
            }
            else
            {
                ConvertFieldName = x => x;
            }
            if (Converter.ConvertFieldTypeName != null)
            {
                ConvertFieldTypeName = Converter.ConvertFieldTypeName;
            }
            else
            {
                ConvertFieldTypeName = (sn, t, x) => x;
            }
        }
    }
}
