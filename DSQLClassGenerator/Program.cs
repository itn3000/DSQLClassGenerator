using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DSQLClassGenerator
{
    using DSQLClassGenerator.Generator;
    using System.Data.Common;
    using System.Data;
    using System.Configuration;
    using Util;
    using Mono.Options;
    using System.IO;
    class Program
    {
        static string OutputPath = "out";
        static string OutputFilePath = "TableDefinitions.cs";
        static string Namespace = "Example";
        static bool ShowHelp = false;
        static bool OutputSchema = true;
        static string ExternalAppConfig = null;
        static bool IsAmalgamation = false;
        static OptionSet CommandOptions = new Mono.Options.OptionSet()
            .Add("a|amalgamation", "Creating classes into one .cs file", v => IsAmalgamation = v != null)
            .Add("d|outputdir=", "Output directory(default: 'out',ignored when amalgamation)", (string x) => OutputPath = x)
            .Add("f|outputfile=", "Output file path(default: 'TablesDefinitions.cs',ignored when no amalgamation)", (string x) => OutputFilePath = x)
            .Add("n|namespace=", "Namespace(default: 'Example')", (string x) => Namespace = x)
            .Add("s|noschema=", "generate class without Schema attribute(default: output with schema attribute)", v => OutputSchema = !(v != null))
            .Add("c|config", "Specify external configuration file path(configuration name must be 'Target')", x => ExternalAppConfig = x)
            .Add("h|help", "Output help message", v => ShowHelp = v != null)
            ;

        static IEnumerable<string> ProcessCommandLine(string[] args)
        {
            return CommandOptions.Parse(args);
        }
        static void WriteCsFile(TableInfo tableInfo)
        {
            var generator = new ClassGeneratorTemplate(new[] { tableInfo }, Namespace, OutputSchema);
            var filePath = Path.Combine(OutputPath != null ? OutputPath : "", $"{tableInfo.Name}.cs");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            File.WriteAllText(filePath, generator.TransformText(), Encoding.UTF8);
        }
        static void WriteAmalgamatedCsFile(IEnumerable<TableInfo> tableInfo)
        {
            var filePath = OutputFilePath;
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var generator = new ClassGeneratorTemplate(tableInfo, Namespace, OutputSchema);
            File.WriteAllText(filePath, generator.TransformText(), Encoding.UTF8);
        }
        static DbConnection CreateConnection()
        {
            const string keyName = "Target";
            string providerName = "";
            string connectionString = "";
            if (!string.IsNullOrEmpty(ExternalAppConfig))
            {
                var confMap = new ExeConfigurationFileMap();
                confMap.ExeConfigFilename = "";
                var conf = ConfigurationManager.OpenMappedExeConfiguration(confMap, ConfigurationUserLevel.None);
                providerName = conf.ConnectionStrings.ConnectionStrings[keyName].ProviderName;
                connectionString = conf.ConnectionStrings.ConnectionStrings[keyName].ConnectionString;
            }
            else
            {
                providerName = ConfigurationManager.ConnectionStrings[keyName].ProviderName;
                connectionString = ConfigurationManager.ConnectionStrings[keyName].ConnectionString;
            }
            var factory = DbProviderFactories.GetFactory(providerName);
            var ret = factory.CreateConnection();
            ret.ConnectionString = connectionString;
            return ret;
        }
        static void Main(string[] args)
        {
            try
            {
                ProcessCommandLine(args);
                if (ShowHelp)
                {
                    CommandOptions.WriteOptionDescriptions(Console.Out);
                    return;
                }
                using (var con = CreateConnection())
                {
                    con.Open();
                    var schemaGetter = SchemaGetterFactory.Create(con);
                    var schemas = schemaGetter.Get(null, con);
                    if (IsAmalgamation)
                    {
                        WriteAmalgamatedCsFile(schemas);
                    }
                    else
                    {
                        foreach (var mappingInfo in schemaGetter.Get(null, con))
                        {
                            WriteCsFile(mappingInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"error: {e}");
                Environment.ExitCode = -1;
            }
        }
    }
}
