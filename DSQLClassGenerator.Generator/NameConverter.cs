using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Generator
{
    public class NameConverter
    {
        /// <summary>
        /// for customizing output class name.
        /// argument is table name,and you must return the new class name you want to generate
        /// </summary>
        public Func<string, string> ConvertClassName { get; set; }
        /// <summary>
        /// for customizing output field name.
        /// argument is sql field name,and you must return the new field name you want to generate
        /// </summary>
        public Func<string, string> ConvertMemberName { get; set; }
        /// <summary>
        /// for customizing output field type.
        /// arguments are following.
        /// <ol>
        ///   <li>raw sql datatype(i.e. varchar)</li>
        ///   <li>mapped type for CLR</li>
        ///   <li>field name(even if you also set ConvertMemberName,passed the before converted name)</li>
        /// </ol>
        /// you must return the new typename of field.
        /// </summary>
        public Func<string, Type, string, string> ConvertFieldTypeName { get; set; }
    }
}
