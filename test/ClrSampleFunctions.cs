using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;

// Compile this file::
//   csc.exe ClrSampleFunctions.cs /target:library /out:ClrSampleFunctions.dll
// 
// Deploy to your database:
//   sqlclrdeployment ClrSampleFunctions.dll -cs="server=(localdb)\ProjectsV12;Database=MyDB;Trusted_Connection=True;"
//
// Use in SQL Server:
//   SELECT dbo.HelloWorld(1)
//   SELECT dbo.StringJoin(name) FROM sys.objects
//   SELECT * FROM dbo.GetTable(1)

public class MyClass
{
    [SqlFunction(DataAccess = DataAccessKind.Read)]
    public static string HelloWorld(int value)
    {
        return "Hello World " + Convert.ToString(value);
    }

    [SqlFunction(DataAccess = DataAccessKind.Read,
             FillRowMethodName = "FillRow_Table",
             TableDefinition = "MyCol NVARCHAR(128)")]
    public static System.Collections.IEnumerable GetTable(int id)
    {
        return new string[] { "Hello", "World" };
    }

    public static void FillRow_Table(Object obj, out string myCol)
    {
        var item = (string)obj;

        myCol = item;
    }
}

namespace MyNamespace
{
    public class MySecondClass
    {
        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static string ByeTheWorld(int value)
        {
            return "Hello World " + Convert.ToString(value);
        }
    }
}

[Serializable]
[SqlUserDefinedAggregate(Format.UserDefined, MaxByteSize = -1)]
public struct StringJoin : IBinarySerialize
{
    private List<string> values;

    public void Init()
    {
        values = new List<string>();
    }

    public void Accumulate(SqlString Value)
    {
        values.Add(Value.IsNull ? string.Empty : Value.Value);
    }

    public void Merge(StringJoin Group)
    {
        values.AddRange(Group.values);
    }

    public SqlString Terminate()
    {
        values.Sort();
        return new SqlString(string.Join("; ", values.ToArray()));
    }

    public void Read(BinaryReader r)
    {
        int count = r.ReadInt32();
        values = new List<string>(count);
        for (int i = 0; i <= count - 1; i++)
        {
            values.Add(r.ReadString());
        }
    }

    public void Write(BinaryWriter w)
    {
        w.Write(values.Count);
        foreach (string s in values)
        {
            w.Write(s);
        }
    }
}