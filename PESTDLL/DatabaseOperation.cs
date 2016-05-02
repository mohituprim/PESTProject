using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactSet.Partners.DBOperations;
using System.IO;

namespace FactSet.Partners.PESTDLL
{
    class DatabaseOperation
    {
        public DatabaseOperation(string localDbServerName)
        {
            m_localServerName = localDbServerName;
        }

        public void RunDuplicateDatabase(SoftwareIniInformation serverSoftwareIniInfo)
        {
            if (serverSoftwareIniInfo.AdoDbProvider.Equals("SQLOLEDB.1"))
            {
                SQLServerOperations sqlDb = new SQLServerOperations();
                string dbBackFileName = sqlDb.ExportSqlDatabaseBackup(serverSoftwareIniInfo.DataSource, serverSoftwareIniInfo.Catalog);
                sqlDb.ImportSqlDatabase(m_localServerName, serverSoftwareIniInfo.Catalog, dbBackFileName);

            }
            //else if (serversoftwareiniinfo.AdodbProvider.Equals("OraOLEDB.Oracle.1"))
            //{
            //    string serverName = "localhost";
            //    string dataBaseSource = "XE";
            //    string backUpDirectoryPath = @"C:\a_appli\";
            //    string username = "BNP";
            //    string password = "BNP";
            //    OracleDbOperation oracleDb = new OracleDbOperation();
            //    oracleDb.DuplicateOracleDatabase(serverName, dataBaseSource, backUpDirectoryPath, username, password);
            //}
            else
                throw new InvalidOperationException("Database information is not correct");
        }

        private string m_localServerName;
    }
}
