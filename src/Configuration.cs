using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    public class Configuration
    {
        public Configuration(string[] args)
        {
            foreach (var arg in this.GetArguments(args))
            {
                switch (arg.Key.ToLower())
                {
                    // Output
                    case "-output":
                    case "-o":
                        this.Action = TypeOfAction.Generate;
                        this.DeploymentFile = new FileInfo(arg.Value);
                        break;

                    // ConnectionString
                    case "-connectionstring":
                    case "-cs":
                        this.Action = TypeOfAction.Generate;
                        this.ConnectionString = arg.Value;
                        break;

                    // PreScripts
                    case "-prescripts":
                    case "-pre":
                        var prePathAndFile = GetPathAndFile(arg.Value);                        
                        this.PreScripts = Directory.EnumerateFiles(this.CurrentDirectory(prePathAndFile[0]), prePathAndFile[1])
                                                   .Select(f => new FileInfo(f))
                                                   .OrderBy(f => f.FullName)
                                                   .ToList();
                        break;

                    // PostScripts
                    case "-postscripts":
                    case "-post":
                        var postPathAndFile = GetPathAndFile(arg.Value);
                        this.PostScripts = Directory.EnumerateFiles(this.CurrentDirectory(postPathAndFile[0]), postPathAndFile[1])
                                                    .Select(f => new FileInfo(f))
                                                    .OrderBy(f => f.FullName)
                                                    .ToList();
                        break;

                    // Trustworthy
                    case "-trustworthy":
                        if (String.IsNullOrEmpty(arg.Value))
                            this.IsTrustworthy = true;
                        else
                            switch (arg.Value.ToLower())
                            {
                                case "0":
                                case "false":
                                case "no":
                                    this.IsTrustworthy = false;
                                    break;

                                case "1":
                                case "true":
                                case "yes":
                                    this.IsTrustworthy = true;
                                    break;
                            }
                        break;

                    // AssemblyFiles
                    default:
                        if (String.IsNullOrEmpty(arg.Value) && !arg.Key.StartsWith("-"))
                        {
                            this.AssemblyFiles.Add(new FileInfo(arg.Key));
                        }
                        break;
                }
            }

        }

        public List<FileInfo> AssemblyFiles { get; set; } = new List<FileInfo>();

        public FileInfo DeploymentFile { get; set; }

        public TypeOfAction Action { get; set; } = TypeOfAction.ShowHelp;

        public string ConnectionString { get; set; }

        public List<FileInfo> PreScripts { get; set; }

        public List<FileInfo> PostScripts { get; set; }

        public bool? IsTrustworthy { get; set; }

        private string AssemblyDirectory()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            if (!path.EndsWith("\\") || path.EndsWith("/")) path += "\\";
            return path;
        }

        private string AssemblyDirectory(string subPath)
        {
            var current = this.AssemblyDirectory();
            return new DirectoryInfo(current + subPath).FullName;
        }

        private string CurrentDirectory()
        {
            string path = Directory.GetCurrentDirectory();
            if (!path.EndsWith("\\") || path.EndsWith("/")) path += "\\";
            return path;
        }
        private string CurrentDirectory(string subPath)
        {
            return new DirectoryInfo(String.IsNullOrEmpty(subPath) ? ".\\" + subPath : subPath).FullName;
        }

        private string[] GetPathAndFile(string file)
        {
            var lastSeparator = file.LastIndexOfAny(new char[] { '\\', '/' });
            if (lastSeparator > 0)
            {
                return new string[] { file.Substring(0, lastSeparator + 1), file.Substring(lastSeparator + 1) };
            }
            else
            {
                return new string[] { string.Empty, file };
            }
        }

        public void ShowHelp()
        {
            Logger.WriteHelp("Tool to generate a SQL Server CLR deployment script.");
            Logger.WriteHelp("Copyright (c) Denis Voituron - v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("Usage: SqlClrDeployement Assemblies [options...]");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("  Assemblies                  List of CLR assemblies to use in the generation process.");
            Logger.WriteHelp("                              Ex: MyAssembly1.dll ..\\bin\\MyAssembly2.dll");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("Options:");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp(" -output=[Filename]           SQL file name to generate.");
            Logger.WriteHelp("                              Ex: -output=Script.sql");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp(" -connectionstring=[\"...\"]  Connection string to execute immediately the SQL requests.");
            Logger.WriteHelp("                              Ex: -cs=\"Server=.;Database=MyDb;Trusted_Connection=True;\"");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp(" -pre=[wildcards]             Filename or wildcards to find SQL files to execute before the CLR deployment.");
            Logger.WriteHelp("                              These scripts will be executed in alphabetic order.");
            Logger.WriteHelp("                              Ex: -pre=C:\\Scripts\\before-*.sql");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp(" -post=[wildcards]            Filename or wildcards to find SQL files to execute after the CLR deployment.");
            Logger.WriteHelp("                              These scripts will be executed in alphabetic order.");
            Logger.WriteHelp("                              Ex: -post=C:\\Scripts\\after-*.sql");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp(" -trustworthy=true|false      Set the TRUSTWORTHY database property to ON or OFF.");
            Logger.WriteHelp("                              Ex: -trustworthy=Yes");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("Samples:");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("  SqlClrDeployement MyAssembly1.dll MyAssembly2.dll -output=Script.sql");
            Logger.WriteHelp(String.Empty);
            Logger.WriteHelp("  SqlClrDeployement MyAssembly.dll -cs=\"Server=.;Database=MyDb;Trusted_Connection=True;\" -pre=..\\*.sql");
        }

        private Dictionary<string, string> GetArguments(string[] args)
        {
            var arguments = new Dictionary<string, string>();
            foreach (var item in args)
            {
                int separatorIndex = item.IndexOf('=');
                if (separatorIndex >= 0)
                {
                    string key = item.Substring(0, separatorIndex).Trim();
                    string value = item.Substring(separatorIndex + 1).Trim().Trim('"').Replace("[TODAY]", DateTime.Today.ToString("yyyyMMdd"));

                    if (!arguments.ContainsKey(key))
                    {
                        arguments.Add(key, value);
                    }
                }
                else
                {
                    arguments.Add(item.Trim(), string.Empty);
                }
            }
            return arguments;
        }


    }

    public enum TypeOfAction
    {
        Generate,
        ShowHelp
    }
}
