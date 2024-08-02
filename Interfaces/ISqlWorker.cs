using System.Data.SqlClient;

namespace backup_manager.Interfaces
{
    internal interface ISqlWorker
    {
        Task<bool> BackupDatabaseAsync(SqlConnection con, string databaseName, string backupPath, string backupDescription, string backupFilename);
        Task<bool> CheckDatabaseAsync(SqlConnection con, string databaseName);
        Task<bool> DatabaseExistAsync(SqlConnection con, string databaseName, bool closeConn = true);
        Task<bool> DeleteDatabaseAsync(SqlConnection con, string databaseName, bool closeConn = true);
        Task<bool> RestoreDatabaseAsync(SqlConnection con, string backupPath, string databaseName);
        Task<bool> RestoreDatabaseWithMoveAsync(SqlConnection con, string backupPath, string databaseName, string dbRestoreDataFolder);
        Task<bool> VerifyDatabaseAsync(SqlConnection con, string backupPath, string databaseName);
    }
}
