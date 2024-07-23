using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Workers
{
    internal class SqlWorker : ISqlWorker
    {
        private readonly ILogger<SqlWorker> logger;

        public SqlWorker(ILogger<SqlWorker> logger)
        {
            this.logger = logger;
        }
        public void BackupDatabase(SqlConnection con, string databaseName, string backupName, string backupDescription, string backupFilename)
        {
            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "backup database {0} to disk = {1} with description = {2}, name = {3}, stats = 1",
                QuoteIdentifier(databaseName),
                QuoteString(backupFilename),
                QuoteString(backupDescription),
                QuoteString(backupName)), con))
            {
                cmd.ExecuteNonQuery();
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError info in e.Errors)
            {
                if (info.Class > 10)
                {
                    // TODO: treat this as a genuine error
                }
                else
                {
                    // TODO: treat this as a progress message
                    logger.LogInformation($"Current progress: {e.Message}");
                }
            }
        }

        private string QuoteIdentifier(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }

        private string QuoteString(string text)
        {
            return "'" + text.Replace("'", "''") + "'";
        }
    }
}