using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    public class SqlAssembly
    {
        private Assembly _clrAssembly = null;

        public SqlAssembly(Assembly clrAssembly, string schema)
        {
            _clrAssembly = clrAssembly;
            this.Schema = schema.TrimStart('[').TrimEnd(']');
            this.Name = clrAssembly.GetName().Name;
        }

        public SqlAssembly(Assembly clrAssembly) : this(clrAssembly, "dbo")
        {
        }

        public SqlAssembly(FileInfo file) : this(Assembly.LoadFile(file.FullName))
        {

        }

        public SqlAssembly(string file) : this(Assembly.LoadFile(file))
        {

        }

        public string Name { get; private set; }

        public string Schema { get; private set; }

        public string ScriptClrEnabled()
        {
            var sql = new StringBuilder();

            sql.AppendLine("IF NOT EXISTS(SELECT value_in_use from sys.configurations where name = 'clr enabled' AND value_in_use = 1) ");
            sql.AppendLine("BEGIN ");
            sql.AppendLine("  EXEC sp_configure 'clr enabled', 1 ");
            sql.AppendLine("  RECONFIGURE ");
            sql.AppendLine("END; ");

            return sql.ToString();
        }

        public string ScriptSetTrustworthy(bool? value)
        {
            if (!value.HasValue) return string.Empty;

            var sql = new StringBuilder();

            sql.AppendLine($"IF NOT EXISTS(SELECT is_trustworthy_on FROM sys.databases WHERE name = db_name() AND is_trustworthy_on = {(value == true ? "1" : "0")}) ");
            sql.AppendLine($"BEGIN ");
            sql.AppendLine($"  DECLARE @sqlAlter NVARCHAR(512) ");
            sql.AppendLine($"  SET @sqlAlter = 'ALTER DATABASE ' + db_name() + ' SET TRUSTWORTHY {(value == true ? "ON" : "OFF")}' ");
            sql.AppendLine($"  EXEC sp_executesql @sqlAlter ");
            sql.AppendLine($"END; ");

            return sql.ToString();
        }

        public string ScriptDropMethods()
        {
            var sql = new StringBuilder();
            var methods = this.GetSqlMethods();

            sql.AppendLine($"-- DROP METHODS OF '{this.Name}'.");
            foreach (var method in methods)
            {
                sql.AppendLine(method.ScriptDropMethod());
            }

            return sql.ToString();
        }

        public string ScriptDropAssembly()
        {
            var sql = new StringBuilder();

            sql.AppendLine($"-- DROP ASSEMBLY '{this.Name}'.");
            sql.AppendLine($"IF EXISTS (SELECT [name] FROM sys.assemblies WHERE [name] = '{this.Name}') ");
            sql.AppendLine($"  DROP ASSEMBLY [{this.Name}]; ");

            return sql.ToString();
        }

        public string ScriptCreateAssembly()
        {
            var sql = new StringBuilder();

            var bytes = new StringBuilder();
            using (var dll = File.OpenRead(_clrAssembly.Location))
            {
                int @byte;
                while ((@byte = dll.ReadByte()) >= 0)
                    bytes.AppendFormat("{0:X2}", @byte);
            }

            // _clrAssembly.GetName().Name
            sql.AppendLine($"-- CREATE ASSEMBLY '{this.Name}'.");
            sql.AppendLine($"CREATE ASSEMBLY [{this.Name}] AUTHORIZATION [{this.Schema}] ");
            sql.AppendLine($"FROM 0x{bytes};");

            return sql.ToString();
        }

        public string ScriptCreateMethods()
        {
            var sql = new StringBuilder();
            var methods = this.GetSqlMethods();

            if (methods.Any())
            {
                sql.AppendLine($"-- CREATE METHODS OF '{this.Name}'.");
                foreach (var method in methods)
                {
                    Logger.WriteInfo($"    Create method '{method.Name}'.");
                    sql.AppendLine(method.ScriptCreateMethod());
                }
            }

            return sql.ToString();
        }

        public IEnumerable<SqlMethod> GetSqlMethods()
        {
            // Methods with SqlServer attribute
            var methods = _clrAssembly.GetTypes()
                                      .SelectMany(t => t.GetMethods())
                                      .Where(m => m.GetCustomAttributes(true)
                                                   .Any(a => a.GetType().FullName.Contains("Microsoft.SqlServer.Server")))
                                      .Select(m => new SqlMethod(this, m));

            // Methods (Accumulate or Terminate) include in class with SqlUserDefinedAggregate attribute
            var aggregates = _clrAssembly.GetTypes()
                                         .Where(t => t.GetCustomAttributes(true)
                                                      .Any(a => a.GetType().FullName.Contains("Microsoft.SqlServer.Server.SqlUserDefinedAggregateAttribute")))
                                         .Select(t => new { Accumulate = t.GetMethod("Accumulate"), Terminate = t.GetMethod("Terminate") })
                                         .Select(m => new SqlMethod(this, m.Accumulate, m.Terminate));

            return methods.Union(aggregates)
                          .Where(m => Convertor.DataTypedConvertor.IsPrimitive(m.SourceWithReturns.ReturnType) ||
                                      Convertor.DataTypedConvertor.IsEnumerable(m.SourceWithReturns.ReturnType));
        }

    }
}
