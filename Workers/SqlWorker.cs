using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace backup_manager.Workers
{
    internal class SqlWorker : ISqlWorker
    {
        private bool isBackupVerified = false;
        private readonly ILogger<SqlWorker> logger;

        public SqlWorker(ILogger<SqlWorker> logger)
        {
            this.logger = logger;
        }
        public void BackupDatabase(SqlConnection con, string databaseName, string backupPath, string backupDescription, string backupFilename)
        {
            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "backup database {0} to disk = {1} with description = {2}, name = {3}, stats = 1",
                QuoteIdentifier(databaseName),
                QuoteString(backupPath),
                QuoteString(backupDescription),
                QuoteString(backupFilename)), con))
            {
                cmd.ExecuteNonQuery();
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;
        }
        public async Task<bool> BackupDatabaseAsync(SqlConnection con, string databaseName, string backupPath, string backupDescription, string backupFilename)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "backup database {0} to disk = {1} with description = {2}, name = {3}, stats = 1, checksum",
                QuoteIdentifier(databaseName),
                QuoteString(backupPath),
                QuoteString(backupDescription),
                QuoteString(backupFilename)), con))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Backup error for {databaseName}.");
                }

                isSucceeded = true;
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;

            return isSucceeded;
        }
        public async Task<bool> VerifyDatabaseAsync(SqlConnection con, string backupPath, string databaseName)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "restore verifyonly from disk = {0}",
                QuoteString(backupPath)), con))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Verify error for {databaseName}.");
                }

                isSucceeded = true;
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;

            logger.LogInformation($"Backup verify status: {isBackupVerified}");

            return isSucceeded && isBackupVerified;
        }
        public async Task<bool> RestoreDatabaseAsync(SqlConnection con, string backupPath, string databaseName)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "restore database {0} from disk = {1}",
                QuoteIdentifier(databaseName),
                QuoteString(backupPath)
                ), con))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Restore error for {databaseName}.");
                }

                isSucceeded = true;
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;

            return isSucceeded && isBackupVerified;
        }
        public async Task<bool> RestoreDatabaseWithMoveAsync(SqlConnection con, string backupPath, string databaseName)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            var testPath = @"D:\MSSQL\ADMTEST\Data";

            var testDbMdfName = databaseName;
            var testDbLdfName = databaseName + "_Log";
            //var testDbMdfPath = Path.Combine(testPath, Path.GetFileNameWithoutExtension(databaseName));
            //var testDbLdfPath = Path.Combine(testPath, Path.GetFileNameWithoutExtension(databaseName));

            var fileSuffix = Path.GetFileNameWithoutExtension(backupPath);
            var newFileSuffix = fileSuffix.Substring(fileSuffix.IndexOf("_"));

            var testNewDbMdfPath = Path.Combine(testPath, testDbMdfName + newFileSuffix + ".mdf");
            var testNewDbLdfPath = Path.Combine(testPath, testDbLdfName + newFileSuffix + ".ldf");

            using (var cmd = new SqlCommand(string.Format(
                "restore database {0} from disk = {1} with move {2} to {3}, move {4} to {5}",
                QuoteIdentifier(databaseName),
                QuoteString(backupPath),
                QuoteString(testDbMdfName),
                QuoteString(testNewDbMdfPath),
                QuoteString(testDbLdfName),
                QuoteString(testNewDbLdfPath)
                ), con))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Restore error for {databaseName}.");
                }

                isSucceeded = true;
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;

            return isSucceeded && isBackupVerified;
        }
        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError info in e.Errors)
            {
                logger.LogInformation($"Class: {info.Class}, state: {info.State}, number: {info.Number}");

                if (info.Class > 10)
                {
                    // TODO: treat this as a genuine error
                    logger.LogError($"Msg source {e.Source}, Error: {e.Message}");
                }
                else
                {
                    // TODO: treat this as a progress message
                    logger.LogInformation($"Msg source {e.Source}, Current progress: {e.Message}");
                }

                // Success verification
                if(info.Number == 3262)
                {
                    isBackupVerified = true;
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