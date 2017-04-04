using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlTypes;

namespace Apps72.Dev.SqlClr.Convertor
{
    /// <summary>
    /// DbType Mapping
    /// See https://gist.github.com/abrahamjp
    /// </summary>
    internal static class DbTypeMap
    {
        private static readonly List<DbTypeMapEntry> _dbTypeList = new List<DbTypeMapEntry>();

        /// <summary>
        /// Initialize the DbTypeMap
        /// </summary>
        static DbTypeMap()
        {
            FillDbTypeList();
        }

        /// <summary>
        /// Fill all dbTypeList entries
        /// </summary>
        public static void FillDbTypeList()
        {           
            // https://msdn.microsoft.com/en-us/library/cc716729.aspx
            _dbTypeList.Add(new DbTypeMapEntry(typeof(Int16), DbType.Int16, SqlDbType.SmallInt, typeof(SqlInt16)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(Int32), DbType.Int32, SqlDbType.Int, typeof(SqlInt32)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(Int64), DbType.Int64, SqlDbType.BigInt, typeof(SqlInt64)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(UInt16), DbType.UInt16, SqlDbType.SmallInt, typeof(SqlInt16)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(UInt32), DbType.UInt32, SqlDbType.Int, typeof(SqlInt32)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(UInt64), DbType.UInt64, SqlDbType.BigInt, typeof(SqlInt64)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit, typeof(SqlBoolean)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(byte), DbType.Byte, SqlDbType.TinyInt, typeof(SqlByte)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(sbyte), DbType.SByte, SqlDbType.SmallInt, typeof(SqlByte)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(Decimal), DbType.Decimal, SqlDbType.Decimal, typeof(SqlDecimal)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(Decimal), DbType.Single, SqlDbType.Decimal, typeof(SqlSingle)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float, typeof(SqlDouble)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(decimal), DbType.Currency, SqlDbType.Money, typeof(SqlMoney)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(decimal), DbType.Currency, SqlDbType.SmallMoney, typeof(SqlMoney)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Real, typeof(SqlDouble)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(double), DbType.VarNumeric, SqlDbType.Real, typeof(SqlDouble)));

            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.NVarChar, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.NChar, typeof(SqlChars)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.Char, typeof(SqlChars)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.NText, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.Text, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.AnsiString, SqlDbType.VarChar, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.AnsiStringFixedLength, SqlDbType.VarChar, typeof(SqlString)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(string), DbType.Xml, SqlDbType.Xml, typeof(SqlXml)));

            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime, typeof(SqlDateTime)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.Date, SqlDbType.Date, typeof(SqlDateTime)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.DateTime2, SqlDbType.DateTime2, typeof(SqlDateTime)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.SmallDateTime, typeof(SqlDateTime)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.DateTimeOffset, SqlDbType.DateTimeOffset, typeof(SqlDateTime)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(DateTime), DbType.Time, SqlDbType.Time, typeof(SqlDateTime)));

            _dbTypeList.Add(new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier, typeof(SqlGuid)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant, typeof(SqlBinary)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image, typeof(SqlBinary)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Binary, typeof(SqlBinary)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Udt, typeof(SqlBinary)));
            _dbTypeList.Add(new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Structured, typeof(SqlBinary)));

        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static DbTypeMapEntry First(Func<DbTypeMapEntry, bool> predicate)
        {
            return _dbTypeList.First(predicate);
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static DbTypeMapEntry FirstOrDefault(Func<DbTypeMapEntry, bool> predicate)
        {
            return _dbTypeList.FirstOrDefault(predicate);
        }
    }


    /// <summary>
    /// Mapping type structure to convert C# type to DbType, or to SqlDbType
    /// </summary>
    internal struct DbTypeMapEntry
    {
        public DbTypeMapEntry(Type type, DbType dbType, SqlDbType sqlDbType, Type sqlType)
        {
            this.Type = type;
            this.DbType = dbType;
            this.SqlDbType = sqlDbType;
            this.SqlType = sqlType;

        }
        public SqlDbType SqlDbType;
        public Type Type;
        public DbType DbType;
        public Type SqlType;
    }
}