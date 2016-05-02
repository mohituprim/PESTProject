using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace FactSet.Partners.PESTDLL
{
    class OracleDbOperation
    {

        public void DuplicateOracleDatabase(string serverName, string dataBaseSource, string backUpDirectoryPath, string username, string password)
        {
            ExportOracleDatabase(serverName, dataBaseSource, backUpDirectoryPath, username, password);
            string dumpFilePath = string.Empty;
            string logFilePath = string.Empty;
            string remoteTableSpace = string.Empty;
            string remoteUserList = string.Empty;
            ImportOracleDatabase(serverName, dataBaseSource, dumpFilePath, logFilePath, remoteTableSpace, remoteUserList);
        }

        private void ExportOracleDatabase(string serverName, string dataBaseSource, string backUpDirectoryPath, string username, string password)
        {
            DateTime Time = DateTime.Now;
            int year = Time.Year;
            int month = Time.Month;
            int day = Time.Day;
            int hour = Time.Hour;
            int min = Time.Minute;
            string backupFileName = day + "_" + month + "_" + year + "_" + hour + "_" + min + ".dmp";
            //your ORACLE_HOME enviroment variable must be setted or you need to set the path here:
            string oracleHome = Environment.GetEnvironmentVariable("ORACLE_HOME");
            string oracleSID = "xe";
            ProcessStartInfo psi = new ProcessStartInfo();
            // Exp is the tool used to export data.
            // This tool is inside $ORACLE_HOME\bin directory
            psi.FileName = Path.Combine(oracleHome, "bin", "exp");
            psi.RedirectStandardInput = false;
            psi.RedirectStandardOutput = true;
            string dumpFile = Path.Combine(backUpDirectoryPath, backupFileName);
            //The command line is: exp user/password@database file=backupname.dmp [OPTIONS....]
            psi.Arguments = string.Format(username + "/" + password + "@" + oracleSID + " FULL=y FILE=" + dumpFile);
            psi.UseShellExecute = false;
            Process process = Process.Start(psi);
            process.WaitForExit();
            process.Close();
        }


        private void ImportOracleDatabase(string serverName, string dataBaseSource, string dumpFilePath, string logFilePath, string remoteTableSpace, string remoteUserList)
        {
            throw new NotImplementedException();
        }

        private void CloseRunningSession(string connection, string user)
        { 
        }

        private void CommonOracleImportQuery(string connection, string remoteUserList,string remoteTableSpace,string dumpFilePath, string logFilePath)
        { 
        }

        private void GetOracleConnection(string dataBaseSource, string username, string password)
        { 
        }

        private void ExecuteOracleDmlQuery(string connection, string queryString)
        {

        }

        private void ExecuteOracleDdlQuery(string connection, string queryString)
        {
        }


    }
}
