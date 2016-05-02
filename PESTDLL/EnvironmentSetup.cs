using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Reflection;

namespace FactSet.Partners.PESTDLL
{
    class EnvironmentSetup
    {
        public void RunEnvironmentSetup(XDocument xmlQuery)
        {
            string sourcePath = string.Empty;
            string targetPath = string.Empty;
            string localDbServerName = string.Empty;
            try
            {
                try
                {
                    sourcePath = xmlQuery.Descendants("SourceFolder").First().Value;
                    targetPath = xmlQuery.Descendants("Destination").First().Value;
                    localDbServerName = xmlQuery.Descendants("LocalDBServerName").First().Value;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("{0} Xml input is not correct", ex);
                }

                sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
                targetPath = targetPath.EndsWith(@"\") ? targetPath : targetPath + @"\";

                // Fetches a client environment and copying as it is in BaseEnvironment
                FileSystemOperation baseEnvironment = new FileSystemOperation();
                string baseEnvironmentName = baseEnvironment.CopyDirectory(sourcePath, targetPath);
                // Read client software.ini file from BaseEnvironment
                string clientSoftwareIniPath = Path.Combine(targetPath, baseEnvironmentName, "software.ini");
                if (File.Exists(clientSoftwareIniPath) == false)
                    throw new FileNotFoundException("Clinet software.ini file does not exist");
                SoftwareIniInformation clientSoftwareIniInfo = new SoftwareIniInformation(clientSoftwareIniPath);
                clientSoftwareIniInfo.ReadSoftwareIniInformation();
                //copy to Partner.61_ModuleName
                string baseEnvironmentPath = Path.Combine(targetPath, "Partners.61_" + clientSoftwareIniInfo.LayerModuleName);
                baseEnvironment.RemoveDirectory(baseEnvironmentPath);
                baseEnvironment.CopyFolderContents(Path.Combine(targetPath, baseEnvironmentName), baseEnvironmentPath);
                baseEnvironment.RemoveDirectory(Path.Combine(targetPath, baseEnvironmentName));

                string clientEnvironmentPath = Path.Combine(targetPath, "Partners.61_" + clientSoftwareIniInfo.LayerModuleName + ".TR.Client");
                FileSystemOperation clientEnvironment = new FileSystemOperation();
                //clientEnvironment.RenameDirectory(targetPath + clientEnvironmentName, newClientEnvironmentPath);
                clientEnvironment.RemoveDirectory(clientEnvironmentPath);
                clientEnvironment.CopyFolderContents(baseEnvironmentPath, clientEnvironmentPath);

                //Check longversion and read server environment directory path
                ComLayerOperation comLayerInterface = new ComLayerOperation();
                string severEnvironmentPath = comLayerInterface.GetRemoteLongVersion(clientSoftwareIniInfo.SrvName, clientSoftwareIniInfo.LayerModuleName);

                string[] serverNamefields = clientSoftwareIniInfo.SrvName.Split('/');
                if (serverNamefields[2] != "localhost")
                {
                    int index = severEnvironmentPath.IndexOf(@"\");
                    severEnvironmentPath = severEnvironmentPath.Remove(0, index + 1);
                    severEnvironmentPath = Path.Combine("\\\\"+serverNamefields[2], severEnvironmentPath);
                }

                //Change the path of client software ini file 
                clientSoftwareIniInfo.SoftwareIniSourcePath = Path.Combine(clientEnvironmentPath, "software.ini");
                // Change the path of SRV NAME
                clientSoftwareIniInfo.SrvName = @"http://localhost/JcfComMonitor/home.aspx";

                // Fetches a server environment and copying it
                FileSystemOperation serverEnvironment = new FileSystemOperation();
                string serverEnvironmentName = serverEnvironment.CopyDirectory(severEnvironmentPath, targetPath);
                severEnvironmentPath = Path.Combine(targetPath, "Partners.61_" + clientSoftwareIniInfo.LayerModuleName + ".TR.Server");
                serverEnvironment.RenameDirectory(Path.Combine(targetPath, serverEnvironmentName), severEnvironmentPath);
                serverEnvironment.RemoveDirectory(Path.Combine(targetPath, serverEnvironmentName));

                // Creating a comlayer module pointing to Partners.dll of server 
                string serverDllPath = Path.Combine(severEnvironmentPath, "Partners.dll");
                comLayerInterface.AddNewComModule(clientSoftwareIniInfo.LayerModuleName, serverDllPath);

                // Read server software.ini file
                string serverSoftwareIniPath = Path.Combine(severEnvironmentPath, "software.ini");
                if (File.Exists(serverSoftwareIniPath) == false)
                    throw new FileNotFoundException("Server software.ini file does not exist");
                //string serverSoftwareIniPath = @"C:\a_appli\Partners.61_KEPLER.TR.Server\software.ini";
                SoftwareIniInformation serverSoftwareIniInfo = new SoftwareIniInformation(serverSoftwareIniPath);
                serverSoftwareIniInfo.ReadSoftwareIniInformation();
                // Fetches model Folder
                string fileServerPath = Path.Combine(serverSoftwareIniInfo.BasePath, "Model");
                serverEnvironment.CopyDirectory(fileServerPath, severEnvironmentPath);
                serverSoftwareIniInfo.BasePath = "#";

                //Start process of databse duplication
                DatabaseOperation dbDuplication = new DatabaseOperation(localDbServerName);
                dbDuplication.RunDuplicateDatabase(serverSoftwareIniInfo);

                ////Update the client and server software.ini
                clientSoftwareIniInfo.UpdateSoftwareIniFile();
                serverSoftwareIniInfo.UpdateSoftwareIniFile();

                // Clear the repository of client and server
                clientEnvironment.ClearRepository(clientEnvironmentPath);
                serverEnvironment.ClearRepository(severEnvironmentPath);

                ////verify by getting longversion
                //string newServerDllPath = comLayerInterface.GetlongVersion(clientSoftwareIniInfo.LayerModuleName);
                //Console.WriteLine("{0} Exception caught.", newServerDllPath);
                //Run the .bat file of client 
                //try
                //{
                //    ProcessStartInfo psi = new ProcessStartInfo();
                //    psi.FileName = newClientEnvironmentPath + @"\" + "_RegComponents.bat";
                //    psi.Verb = "runas";
                //    Process.Start(psi);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("{0} Exception caught.", e);
                //}

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
