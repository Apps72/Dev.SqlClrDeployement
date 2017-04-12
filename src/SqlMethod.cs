using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    [DebuggerDisplay("{Name}")]
    public class SqlMethod
    {
        private SqlAssembly _sqlAssembly;

        public SqlMethod(SqlAssembly assembly, MethodInfo method) : this(assembly, method, method)
        {
        }

        public SqlMethod(SqlAssembly assembly, MethodInfo methodWithParams, MethodInfo methodWithReturns)
        {
            _sqlAssembly = assembly;
            this.SourceWithParams = methodWithParams;
            this.SourceWithReturns = methodWithReturns;
            this.Name = methodWithParams.Name;
            this.Namespace = methodWithParams.DeclaringType.Namespace;
            this.ClassName = methodWithParams.DeclaringType.Name;

            // Type
            if (methodWithParams != methodWithReturns)
            {
                // AGGREGATE
                this.Name = this.ClassName;
                var attributes = methodWithParams.DeclaringType.GetCustomAttributes(true).Select(a => a.GetType().Name);
                if (attributes.Contains("SqlUserDefinedAggregateAttribute")) this.Type = this.Type = SqlMethodType.SqlAgregate;
            }
            else
            {
                // PROCEDURE, FUNCTION, TRIGGER
                var attributes = methodWithParams.GetCustomAttributes(true).Select(a => a.GetType().Name);
                if (attributes.Contains("SqlFunctionAttribute")) this.Type = SqlMethodType.SqlFunction;
                if (attributes.Contains("SqlProcedureAttribute")) this.Type = SqlMethodType.SqlProcedure;
                if (attributes.Contains("SqlTriggerAttribute")) this.Type = SqlMethodType.SqlTrigger;
            }

            // Trigger
            if (this.Type == SqlMethodType.SqlTrigger)
            {
                dynamic attribute = methodWithParams.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name == "SqlTriggerAttribute");
                this.TriggerTarget = attribute.Target;
                this.TriggerEvent = attribute.Event;

                var target = this.TriggerTarget.Split('.');
                if (target.Length >= 2)
                    this.TriggerSchema = target[0].TrimStart('[').TrimEnd(']');
                else
                    this.TriggerSchema = _sqlAssembly.Schema;
            }
        }

        public string Name { get; private set; }

        public string Namespace { get; set; }

        public string ClassName { get; private set; }

        public SqlMethodType Type { get; private set; } = SqlMethodType.Unknown;

        public IEnumerable<SqlParameter> Parameters
        {
            get
            {
                return this.SourceWithParams.GetParameters().Select(p => new SqlParameter(p));
            }
        }

        public SqlParameter ReturnsScalar
        {
            get
            {
                if (Convertor.DataTypedConvertor.IsPrimitive(this.SourceWithReturns.ReturnType))
                    return new SqlParameter(this.SourceWithReturns.ReturnParameter);
                else
                    return null;
            }
        }

        public string TriggerTarget { get; private set; }

        public string TriggerEvent { get; private set; }

        public string TriggerSchema { get; private set; }

        public string ReturnsTableDefinition
        {
            get
            {
                if (Convertor.DataTypedConvertor.IsEnumerable(this.SourceWithReturns.ReturnType))
                {
                    dynamic attribute = this.SourceWithReturns.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name == "SqlFunctionAttribute");
                    return attribute.TableDefinition;
                }
                else
                    return String.Empty;
            }
        }

        public MethodInfo SourceWithParams { get; private set; }
        public MethodInfo SourceWithReturns { get; private set; }

        public string ScriptCreateMethod()
        {
            var sql = new StringBuilder();

            if (this.Type != SqlMethodType.Unknown)
            {
                sql.Append($"PRINT 'Creating [{this.Name}]...'");
                sql.AppendLine();

                // CREATE FUNCTION [dbo].[Mymethod]                
                sql.Append($"EXEC sp_executesql N'");
                sql.Append($"CREATE {GetSqlMethodType()}  [{this.Name}] ");

                // Parameters list
                if (this.Parameters.Any() || this.Type == SqlMethodType.SqlFunction)
                {
                    sql.Append($"(");
                    sql.Append(String.Join(",", this.Parameters.Select(p => $"@{p.Name} {p.TypeAsString}{(p.IsOutput ? " OUTPUT" : String.Empty)}")));
                    sql.Append($")");
                }

                // TRIGGER
                if (this.Type == SqlMethodType.SqlTrigger)
                {
                    sql.Append($" ON {this.TriggerTarget} {this.TriggerEvent}");
                }

                // RETURNS SQLTYPE
                if (this.Type == SqlMethodType.SqlFunction || this.Type == SqlMethodType.SqlAgregate)
                {
                    // SCALAR
                    if (this.ReturnsScalar != null)
                    {
                        sql.Append($" RETURNS {this.ReturnsScalar.TypeAsString}");
                    }

                    // TABLE
                    else
                    {
                        sql.Append($" RETURNS TABLE({this.ReturnsTableDefinition})");
                    }
                }

                // Namespace.
                string namspace = String.IsNullOrEmpty(this.Namespace) ? String.Empty : $"{this.Namespace}.";

                // AS EXTERNAL NAME [AssemblyName].[Namespace.ClassName].[MethodName]
                if (this.Type == SqlMethodType.SqlAgregate)
                    sql.Append($" EXTERNAL NAME [{_sqlAssembly.Name}].[{namspace}{this.Name}]; ");
                else
                    sql.Append($" AS EXTERNAL NAME [{_sqlAssembly.Name}].[{namspace}{this.ClassName}].[{this.Name}]; ");

                sql.Append($"'");
            }

            return sql.ToString();
        }

        public string ScriptDropMethod()
        {
            var sql = new StringBuilder();

            if (this.Type != SqlMethodType.Unknown)
            {
                // IF EXISTS
                sql.Append($"IF EXISTS(SELECT name FROM sysobjects");
                sql.Append($"  WHERE name = '{this.Name}' ");
                sql.Append($"    AND type IN ({this.GetSqlMethodCodesInClause()})) ");

                // DROP FUNCTION [MyMethod]
                if (this.Type == SqlMethodType.SqlTrigger)
                    sql.Append($" DROP {GetSqlMethodType()}  [{this.TriggerSchema}].[{this.Name}]; ");
                else
                    sql.Append($" DROP {GetSqlMethodType()}  [{this.Name}]; ");
            }

            return sql.ToString();
        }

        private string GetSqlMethodType()
        {
            switch (this.Type)
            {
                case SqlMethodType.SqlFunction:
                    return "FUNCTION";

                case SqlMethodType.SqlProcedure:
                    return "PROCEDURE";

                case SqlMethodType.SqlTrigger:
                    return "TRIGGER";

                case SqlMethodType.SqlAgregate:
                    return "AGGREGATE";

                default:
                    return string.Empty;
            }
        }

        private string[] GetSqlMethodCodes()
        {
            // TODO: to complete list of codes

            switch (this.Type)
            {
                case SqlMethodType.SqlFunction:
                    return new string[] { "FS", "FT" };

                case SqlMethodType.SqlProcedure:
                    return new string[] { "PC" };

                case SqlMethodType.SqlTrigger:
                    return new string[] { "TA" };

                case SqlMethodType.SqlAgregate:
                    return new string[] { "AF" };

                default:
                    return new string[] { };
            }
        }

        private string GetSqlMethodCodesInClause()
        {
            return String.Join(",", this.GetSqlMethodCodes().Select(i => $"'{i}'"));
        }


    }

    public enum SqlMethodType
    {
        Unknown,
        SqlFunction,
        SqlProcedure,
        SqlTrigger,
        SqlAgregate
    }
}
