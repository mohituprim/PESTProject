using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactSet.Partners.PESTDLL;
using FactSet.Partners.Lib_EDP;
using System.IO;

namespace PartnersEST
{
    class Program
    {
        static void Main(string[] args)
        {
            DllWrapper dll5 = null;
            try
            {
                dll5 = new DllWrapper("PESTDLL");
            }
            catch (ArgumentException argumentException)
            {
                Console.WriteLine("{0} Exception caught.", argumentException);
                return;
            }

            try
            {
                string query = File.ReadAllText(Path.Combine((Directory.GetCurrentDirectory()), "PESTConfiguration.xml"));
                dll5.FctInvoke(query);
            }
            catch (Exception argumentException)
            {
                Console.WriteLine("{0} Exception caught.", argumentException);
                return;
            }

            //string query = @"<?xml version='1.0' encoding='utf-8' ?>
            //                    <start>
            //                      <File>
            //                        <SourceFolder>C:\Users\mokumar\Desktop\a_appli\Kepler\FactSetPartners_Client</SourceFolder>
            //                       <Destination>C:\a_appli\</Destination>
            //                        <LocalDBServerName>MOKUMARPC\SQLEXPRESS</LocalDBServerName>
            //                      </File>
            //                    </start>";
            //<SourceFolder>C:\Users\mokumar\Desktop\a_appli\Kepler\FactSetPartners_Client</SourceFolder>
            //<SourceFolder>\\prod.factset.com\dfs\Partners\HYDEngineering\Sup\Kepler\FactSetPartners_Client</SourceFolder>
        }
    }
}