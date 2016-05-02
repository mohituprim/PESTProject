// Do not modify this file

using System.Xml.Linq;

namespace FactSet.Partners.Lib_EDP
{
    public interface IDllBase
    {
        string FctInvoke(XDocument xmlquery);
        string FctDataQuery(string query);
    }
}
