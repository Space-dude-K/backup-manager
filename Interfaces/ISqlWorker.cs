using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backup_manager.Interfaces
{
    internal interface ISqlWorker
    {
        void BackupDatabase(SqlConnection con, string databaseName, string backupName, string backupDescription, string backupFilename);
        Task<bool> BackupDatabaseAsync(SqlConnection con, string databaseName, string backupPath, string backupDescription, string backupFilename);
        Task<bool> VerifyDatabaseAsync(SqlConnection con, string backupPath, string databaseName);
    }
}
