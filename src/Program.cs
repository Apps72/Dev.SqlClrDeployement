using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            try
#endif
            {
                var config = new Configuration(args);

                if (config.Action == TypeOfAction.ShowHelp || config.AssemblyFiles.Count() <= 0)
                {
                    config.ShowHelp();
                }
                else
                {
                    Logger.WriteInfo("Tool to generate a SQL Server CLR deployment script.");
                    Logger.WriteInfo("(c) Denis Voituron - v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
                    Logger.WriteInfo(String.Empty);

                    Logger.WriteInfo("Started...");

                    var sql = new StringBuilder();
                    var assemblies = new List<SqlAssembly>();

                    foreach (var item in config.AssemblyFiles)
                    {
                        Logger.WriteInfo($"  Reading assembly '{item.Name}'.");
                        assemblies.Add(new SqlAssembly(item.FullName));
                    }

                    sql.AppendLine("PRINT ' >>> ASSEMBLIES DEPLOYING.'");

                    sql.AppendLine(assemblies.First().ScriptClrEnabled());
                    sql.AppendLine(assemblies.First().ScriptSetTrustworthy(config.IsTrustworthy));

                    foreach (var item in assemblies.Reverse<SqlAssembly>())
                    {
                        sql.AppendLine(item.ScriptDropMethods());
                    }

                    foreach (var item in assemblies.Reverse<SqlAssembly>())
                    {
                        sql.AppendLine(item.ScriptDropAssembly());
                    }

                    foreach (var item in assemblies)
                    {
                        sql.AppendLine(item.ScriptCreateAssembly());
                    }

                    foreach (var item in assemblies)
                    {
                        sql.AppendLine(item.ScriptCreateMethods());
                    }

                    sql.AppendLine("PRINT ' >>> ASSEMBLIES DEPLOYED.'");

                    // Output
                    if (config.DeploymentFile != null)
                    {
                        System.IO.File.WriteAllText(config.DeploymentFile.FullName, sql.ToString());
                        Logger.WriteInfo($"File '{config.DeploymentFile.Name}' generated.");
                    }

                    // PreScripts
                    if (config.PreScripts != null)
                    {
                        var conn = new SqlConnection(config);
                        foreach (var file in config.PreScripts)
                        {
                            Logger.WriteInfo($"Executing pre-script '{file.Name}'.");
                            conn.Execute(System.IO.File.ReadAllText(file.FullName));
                        }
                    }

                    // ConnectionString
                    if (!String.IsNullOrEmpty(config.ConnectionString))
                    {
                        Logger.WriteInfo($"Executing CLR scripts.");
                        new SqlConnection(config).Execute(sql.ToString());
                    }

                    // PostScripts
                    if (config.PostScripts != null)
                    {
                        var conn = new SqlConnection(config);
                        foreach (var file in config.PostScripts)
                        {
                            Logger.WriteInfo($"Executing post-script '{file.Name}'.");
                            conn.Execute(System.IO.File.ReadAllText(file.FullName));
                        }
                    }
                }

            }
#if !DEBUG
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
                if (ex.InnerException != null)
                    Logger.WriteError(ex.InnerException.Message);

                Console.ResetColor();
                Environment.Exit(1359);     // INTERNAL ERROR
            }
#endif

#if DEBUG
            Console.WriteLine();
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
#endif
        }
    }
}
