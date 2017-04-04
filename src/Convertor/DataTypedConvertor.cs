using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Apps72.Dev.SqlClr.Convertor
{
    public static partial class DataTypedConvertor
    {
        /// <summary>
        /// Convert db type to .Net data type
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static Type ToNetType(System.Data.SqlDbType dbType)
        {
            DbTypeMapEntry entry = DbTypeMap.First(t => t.SqlDbType == dbType);
            return entry.Type;
        }

        /// <summary>
        /// Convert .Net type to Db type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Data.SqlDbType ToSqlDbType(Type type)
        {
            if (type.FullName.StartsWith("System.Data.SqlTypes.Sql"))
            {
                DbTypeMapEntry entry = DbTypeMap.First(t => t.SqlType == type);
                return entry.SqlDbType;
            }
            else
            {
                DbTypeMapEntry entry = DbTypeMap.First(t => t.Type == type);
                return entry.SqlDbType;
            }
        }

        /// <summary>
        /// Returns True if the specified type is Primitive (int, string, void, ...)
        /// Or false if it's an array (for example)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPrimitive(Type type)
        {
            return (type.IsPrimitive || 
                    type == typeof(string) ||                     
                    type == typeof(void) ||
                    type.FullName.StartsWith("System.Data.SqlTypes.Sql"));
        }

        /// <summary>
        /// Returns True if the specified type is IEnumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(Type type)
        {
            return (type == typeof(IEnumerable));
        }
    }
}