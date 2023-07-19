using Google.Protobuf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G4.QueryFramework.Controllers {
    public class QueryConnection {

        private string ConnectionString { get; set; }
        internal MySqlConnection? Connection { get; set; }

        internal QueryConnection(string connectionString) {
            this.ConnectionString = connectionString;
        }

        internal void BuildConnection() {

            if (Connection == null) {
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();
                return;
            }

            if (Connection.State == System.Data.ConnectionState.Open)
                return;

        }

        internal void DisposeConnection() {

            if (Connection == null)
                return;

            if (Connection.State == System.Data.ConnectionState.Closed) {
                Connection = null;
                return;
            }

            Connection.Close();
            Connection = null;

        }

        internal MySqlCommand CreateCommand(string sql, object? parameters = null, Dictionary<string, object>? where = null) {

            this.BuildConnection();

            var command = new MySqlCommand(sql, this.Connection);

            if(parameters != null) {
                foreach(var parameter in parameters.GetType().GetProperties()) {
                    command.Parameters.AddWithValue($"@{parameter.Name}", parameter.GetValue(parameters));
                }
            }

            if(where != null) {
                foreach(var parameter in where.Keys) {
                    command.Parameters.AddWithValue(parameter, where[parameter]);
                }
            }

            return command;

        }

    }
}
