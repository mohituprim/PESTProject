using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net;
using JCFSDK4;

namespace FactSet.Partners.PESTDLL
{
    class ComLayerOperation
    {

        public string GetLongVersion(string layerModuleName)
        {
            try
            {
                ComLayer comlayer = new ComLayer();
                string result = comlayer.FctDataQuery("C00001", layerModuleName + "\\" + "LONGVERSION", "EUR");
                result = result.Substring(0, result.LastIndexOf("\\") + 1);
                return result;
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while trying to get longversion", ex);
            }
        }

        public string GetRemoteLongVersion(string serverName, string layerModuleName)
        {
            try
            {
                WebClient obj = new WebClient();
                string longVersionUrl = serverName + "?DN=C00001&TAC=" + layerModuleName + @"\LONGVERSION&CUR=USD";
                string result = obj.DownloadString(longVersionUrl);
                result = result.Substring(0, result.LastIndexOf("\\") + 1);
                return result;
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while trying to get longversion from remote server ", ex);
            }
        }

        public void AddNewComModule(string moduleName, string serverDllPath)
        {
            m_serverconfigObj = new ServerConfig();

            try
            {
                if (m_serverconfigObj == null)
                {
                    throw new System.InvalidOperationException("ServerConfig is not available. Check FactSet ComLayer installation.");
                }
                // Stop Com Module if it is runing
                m_serverconfigObj.Stop(moduleName);
                //Remove module if exist with same name
                RemoveComModdule(moduleName);
                // Add the module
                m_serverconfigObj.AddModule(moduleName, serverDllPath, true, true);
                // And configure its properties
                m_serverconfigObj.PutMaxPool(moduleName, 1);
                m_serverconfigObj.PutMinPool(moduleName, 1);
                m_serverconfigObj.PutInactivityTimeOut(moduleName, 0);
                // Identity needs a special exception trap for the user's comfort
                //    serverconfigObj.ChangePassword(strModuleName, userName,password);
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while creating new comlayer module", ex);
            }
        }
        public void RemoveComModdule(string moduleName)
        {
            try
            {
                // Stop Com Module if it is runing
                m_serverconfigObj.Stop(moduleName);

                Array array = m_serverconfigObj.GetModulesArray();
                foreach (string module in array)
                {
                    if (module.Equals(moduleName))
                    {
                        m_serverconfigObj.RemoveModule(moduleName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while removing comlayer module", ex);
            }
        }

        private ServerConfig m_serverconfigObj;
    }
}