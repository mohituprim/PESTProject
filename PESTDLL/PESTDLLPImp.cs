using System;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;
using FactSet.Partners.Lib_EDP;

namespace FactSet.Partners.PESTDLL
{
    // Put your code here or in other classes, note that new public classes are forbidden
    internal class PESTDLLPImp : IDllBase
    {
        // Some request are generic to all EDP projects, they are managed in IDllBasePImp

        // From a short simply formatted request (ie not XML) return a simple answer
        //   - It cannot modify anything (no database modification, no file creation, ...)
        //   - It must let the memory exactly in the same state (no cache, no repository refresh, ...)
        public string FctDataQuery(string query)
        {
            // This is a sample, replace it with your actual code
            if (string.Equals(query, "NOW", StringComparison.OrdinalIgnoreCase))
                return DateTime.Now.ToString();

            return "FctDataQuery: " + query;
            // End of the sample code
        }
        // From an XML request return a more generic answer
        public string FctInvoke(XDocument xmlquery)
        {
            try
            {
                EnvironmentSetup environmentSetup = new EnvironmentSetup();
                environmentSetup.RunEnvironmentSetup(xmlquery);
                return "SUCCEED";
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught.", ex);
                return "Failed";
            }
        }
    }
}