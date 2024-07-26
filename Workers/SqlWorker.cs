using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace backup_manager.Workers
{
    internal class SqlWorker : ISqlWorker
    {
        private bool isBackupVerified = false;
        private bool isBackupChecked = false;

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
        public async Task<bool> RestoreDatabaseWithMoveAsync(SqlConnection con, 
            string backupPath, string databaseName, string dbRestoreDataFolder)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            var testPath = dbRestoreDataFolder;
            var testDbMdfName = databaseName;
            var testDbLdfName = databaseName + "_Log";
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
                    cmd.CommandTimeout = 360;
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
        public async Task<bool> CheckDatabaseAsync(SqlConnection con, string databaseName)
        {
            bool isSucceeded = false;

            con.FireInfoMessageEventOnUserErrors = true;
            con.InfoMessage += OnInfoMessage;
            con.Open();

            using (var cmd = new SqlCommand(string.Format(
                "dbcc checkdb {0} with EXTENDED_LOGICAL_CHECKS",
                Brackets(databaseName)), con))
            {
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Check error for {databaseName}.");
                }

                isSucceeded = true;
            }

            con.Close();
            con.InfoMessage -= OnInfoMessage;
            con.FireInfoMessageEventOnUserErrors = false;

            return isSucceeded && isBackupChecked;
        }
        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError info in e.Errors)
            {
                logger.LogDebug($"Class: {info.Class}, state: {info.State}, number: {info.Number}");

                if (info.Class > 10)
                {
                    // TODO: treat this as a genuine error
                    logger.LogError($"Msg source {e.Source}, Error: {e.Message}");
                }
                else
                {
                    // TODO: treat this as a progress message
                    logger.LogDebug($"Msg source {e.Source}, Current progress: {e.Message}");
                }

                // Success verification
                if(info.Number == 3262)
                {
                    isBackupVerified = true;
                }
                // Success check
                if (info.Number == 8989)
                {
                    // Exclude db name part
                    var msg = info.Message.Substring(0, info.Message.IndexOf("\""));
                    isBackupChecked = !msg.Any(c => char.IsNumber(c) && (int)Char.GetNumericValue(c) > 0);

                    logger.LogDebug($"Backup checkd status: {isBackupChecked}");
                }
            }
        }
        private string QuoteIdentifier(string name)
        {
            return "[" + name.Replace("]", "]]") + "]";
        }
        private string Brackets(string name)
        {
            return "(" + name + ")";
        }

        private string QuoteString(string text)
        {
            return "'" + text.Replace("'", "''") + "'";
        }
    }
}