using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FactSet.Partners.PESTDLL
{
    class SoftwareIniInformation
    {
        public SoftwareIniInformation(string softwareIniPath)
        {
            SoftwareIniSourcePath = softwareIniPath;
        }

        public string SoftwareIniSourcePath { get; set; }
        public string BasePath { get; set; }
        public string LayerModuleName { get; set; }
        public string SrvName { get; set; }
        public string AdoDbProvider { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DataSource { get; set; }
        public string Catalog { get; set; }

        public void ReadSoftwareIniInformation()
        {
            try
            {
                string line;
                StreamReader fileReader = new StreamReader(SoftwareIniSourcePath);
                while ((line = fileReader.ReadLine()) != null)
                {
                    if (line.StartsWith("BASE"))
                    {
                        string[] fields = line.Split('=');
                        BasePath = fields[1];
                    }
                    if (line.StartsWith("LAYER MODULE NAME"))
                    {
                        string[] fields = line.Split('=');
                        LayerModuleName = fields[1];
                    }
                    if (line.StartsWith("SRV NAME"))
                    {
                        string[] fields = line.Split('=');
                        SrvName = fields[1];
                    }
                    if (line.StartsWith("ADODB=Provider"))
                    {
                        line = line.Replace("ADODB=", string.Empty);
                        string[] fields = line.Split(';');
                        foreach (var field in fields)
                        {
                            string[] columnFields = field.Split('=');
                            if (columnFields[0].Equals("Provider"))
                                AdoDbProvider = columnFields[1];
                            else if (columnFields[0].Equals("User ID"))
                                UserName = columnFields[1];
                            else if (columnFields[0].Equals("Password"))
                                Password = columnFields[1];
                            else if (columnFields[0].Equals("Data Source"))
                                DataSource = columnFields[1];
                            else if (columnFields[0].Equals("Initial Catalog"))
                                Catalog = columnFields[1];
                        }
                    }
                }
                fileReader.Close();
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while reading software.ini information.", ex);
            }
        }

        public void UpdateSoftwareIniFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(SoftwareIniSourcePath);
                int index = 0;
                foreach (string line in lines)
                {
                    if (line.StartsWith("BASE") && !BasePath.Equals(string.Empty))
                        lines[index] = line.Replace(line, "BASE=" + BasePath);

                    if (line.StartsWith("SRV NAME") && !SrvName.Equals(string.Empty))
                        lines[index] = line.Replace(line, "SRV NAME ="+ SrvName);

                    if (line.StartsWith("ADODB=Provider") && !AdoDbProvider.Equals(string.Empty))
                    {
                        if (AdoDbProvider.Equals("SQLOLEDB.1"))
                            lines[index] = line.Replace(line, "ADODB=Provider=SQLOLEDB.1;" + "Data Source=" + DataSource + ";" + "Initial Catalog=" + Catalog + ";" + "Integrated Security=SSPI;" + "LANGUAGE=English;");
                        else if (AdoDbProvider.Equals("OraOLEDB.Oracle.1"))
                            lines[index] = line.Replace(line, "ADODB=Provider=OraOLEDB.Oracle.1;" + "Password=" + Password + ";" + "Persist Security Info=True;" + "User ID=" + UserName + ";" + "Data Source=" + DataSource + ";" + "language=english");
                    }
                    index++;
                }
                File.WriteAllLines(SoftwareIniSourcePath, lines);
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while updating software.ini information.", ex);
            }
        }
    }
}
