using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    public class SqlParameter
    {
        public SqlParameter(ParameterInfo parameter)
        {
            this.Name = parameter.Name;
            this.IsOutput = parameter.IsOut;

            if (parameter.IsOut)
                this.Type = Convertor.DataTypedConvertor.ToSqlDbType(parameter.ParameterType.GetElementType());
            else
                this.Type = Convertor.DataTypedConvertor.ToSqlDbType(parameter.ParameterType);
        }

        public string Name { get; private set; }
        public SqlDbType Type { get; private set; }
        public bool IsOutput { get; private set; }
        public string TypeAsString
        {
            get
            {
                if (Type == SqlDbType.Char || Type == SqlDbType.NChar ||
                    Type == SqlDbType.VarChar || Type == SqlDbType.NVarChar)
                {
                    return $"{this.Type.ToString().ToUpper()}(MAX)";
                }
                else
                {
                    return this.Type.ToString().ToUpper();
                }
            }
        }
    }
}
