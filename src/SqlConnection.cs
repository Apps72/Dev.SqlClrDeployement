using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    public class SqlConnection
    {
        private Configuration _configuration;

        public SqlConnection(Configuration configuration)
        {
            _configuration = configuration;
        }

        public int Execute(string query)
        {
            if (!String.IsNullOrEmpty(_configuration.ConnectionString))
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(_configuration.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {                        
                        cmd.CommandText = query;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }

            return 0;
        }
    }
}
