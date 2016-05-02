using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Security;
using System.Data;
using System;
using System.Security.Permissions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace FactSet.Partners.DBOperations
{
    public class SQLServerOperations
    {
        [SecuritySafeCritical]
        public string ExportSqlDatabaseBackup(string serverName, string databaseName)
        {
            try
            {
                ServerConnection connection = new ServerConnection(serverName);
                connection.LoginSecure = true;
                Server sqlServer = new Server(connection);

                string backupDirectoryPath = sqlServer.Settings.BackupDirectory;
                DateTime currentTime = DateTime.Now;
                int year = currentTime.Year;
                int month = currentTime.Month;
                int day = currentTime.Day;
                int hour = currentTime.Hour;
                int min = currentTime.Minute;
                string backUpFileName = databaseName + "_" + day + "_" + month + "_" + year + "_" + hour + "_" + min + ".bak";
                string backUpFilePath = Path.Combine(backupDirectoryPath, backUpFileName);

                BackupDeviceItem deviceItem = new BackupDeviceItem(backUpFilePath, DeviceType.File);
                Database db = sqlServer.Databases[databaseName];
                Backup sqlBackup = new Backup()
                {
                    Action = BackupActionType.Database,
                    BackupSetDescription = "ArchiveDataBase:" + DateTime.Now.ToShortDateString(),
                    BackupSetName = "Archive",
                    Database = databaseName,
                    Initialize = true,
                    Checksum = true,
                    ContinueAfterError = true,
                    Incremental = false,
                    FormatMedia = false
                };

                sqlBackup.Devices.Add(deviceItem);
                sqlBackup.SqlBackup(sqlServer);

                return backUpFileName;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("{0} Backup of database failed.", ex);
            }
        }

        [SecuritySafeCritical]
        public void ImportSqlDatabase(string serverName, string dataBaseName, string backUpFileName)
        {
            try
            {
                ServerConnection connection = new ServerConnection(serverName);
                connection.LoginSecure = true;
                DropSqlDatabase(serverName, dataBaseName);
                Server sqlServer = new Server(connection);
                string backUpFilePath = Path.Combine(sqlServer.Settings.BackupDirectory, backUpFileName);
                // Get the default file and log locations
                // (If DefaultFile and DefaultLog are empty, use the MasterDBPath and MasterDBLogPath values)
                string defaultFilePath = sqlServer.Settings.DefaultFile;
                if (defaultFilePath.Equals(string.Empty))
                    defaultFilePath = sqlServer.Information.MasterDBPath;
                string defaultLogPath = sqlServer.Settings.DefaultLog;
                if (defaultLogPath.Equals(string.Empty))
                    defaultLogPath = sqlServer.Information.MasterDBLogPath;
                // Build the physical file names for the database copy
                string dataFile = defaultFilePath + "/" + dataBaseName + "_Data.mdf";
                string logFile = defaultLogPath + "/" + dataBaseName + "_Log.ldf";
                // Use the backup file name to create the backup device
                BackupDeviceItem bkpDevice = new BackupDeviceItem(backUpFilePath, DeviceType.File);

                // Create the new restore object, set the database name and add the backup device
                Restore restoreDatabase = new Restore()
                {
                    Database = dataBaseName
                };
                restoreDatabase.Devices.Add(bkpDevice);
                // Get the file list info from the backup file
                DataTable logicalRestoreFiles = restoreDatabase.ReadFileList(sqlServer);
                restoreDatabase.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[0][0].ToString(), dataFile));
                restoreDatabase.RelocateFiles.Add(new RelocateFile(logicalRestoreFiles.Rows[1][0].ToString(), logFile));
                // Restore the database
                restoreDatabase.SqlRestore(sqlServer);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("{0} Restoration of database failed.", ex);
            }
        }

        [SecuritySafeCritical]
        public void DropSqlDatabase(string serverName, string databaseName)
        {
            try
            {
                var server = new Server(serverName); // Can use overload that specifies 
                foreach (Database db in server.Databases)
                {
                    if (db.Name.ToLower().Equals(databaseName.ToLower()))
                    {
                        db.Refresh();
                        db.Drop();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("{0} Database Drop operation failed", ex);
            }
        }
    }
}
