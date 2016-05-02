// Do not modify this file

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace FactSet.Partners.Lib_EDP
{
    public abstract class DllBase
    {
        protected DllBase()
        {
            m_edpdll = new Lazy<IDllBase>(CreateDllBase);
        }

        public string FctDataQuery(string query)
        {
            if (!ValidateStringEntry(ref query))
                return "";

            if (string.Equals(query, "VERSION", StringComparison.OrdinalIgnoreCase))
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (string.Equals(query, "LONGVERSION", StringComparison.OrdinalIgnoreCase))
                return LongVersion();

            if (!query.StartsWith(ModuleName() + "\\", StringComparison.OrdinalIgnoreCase))
                return "Unrecognized query";
            query = query.Remove(0, ModuleName().Length + 1);

            return m_edpdll.Value.FctDataQuery(query);
        }

        public string FctInvoke(string query)
        {
            if (!ValidateStringEntry(ref query))
                return "";

            XDocument xmldocument = null;
            try
            {
                xmldocument = XDocument.Parse(query);
            }
            catch (XmlException xmlexception)
            {
                return xmlexception.Message; // should we wrap it into some xml?
            }
            return m_edpdll.Value.FctInvoke(xmldocument);
        }

        protected abstract IDllBase CreateDllBase();

        protected abstract string ModuleName();

        private bool ValidateStringEntry(ref string entry)
        {
            if (!entry.StartsWith(Checker, StringComparison.Ordinal))
                return false;
            entry = entry.Remove(0, Checker.Length);
            return true;
        }

        private string LongVersion()
        {
            string sResult = Assembly.GetExecutingAssembly().Location;
            sResult += " " + Assembly.GetExecutingAssembly().GetName().Version;
#if DEBUG
            sResult += " [Debug Unicode] ";
#else
            sResult += " [Release Unicode] ";
#endif
            sResult += "[pid=" + Process.GetCurrentProcess().Id + "," + Thread.CurrentThread.ManagedThreadId + "]";
            return sResult;
        }

        private const string Checker =
            "7XZHtm+iQOmM3WvAVt43mwNL21hFpU7LpA72IruwT0BAS6hH6KJLGoWvEU623WoiHZXgGAuSRjq/SJlKrB9j8nOB5XCp+UIYm3eQ/fOexK1g8GOPXnVFiaV5nvBGyM/rs/Ist2yLQ4GqdqGZq39OkFaV9jelUElzplum86QmA8A+I34yT/JEybi+m+sTIV78pQC5ND2uSm+6S4V01HIIFESU3mPWKU51pt6eoqihTZJ5WJK1LOlLD7HLQiRIuzcwb7LiZ9TzTVir5xhTi0Gr+XR94KAggEWFuiU8/Ge5MdHgG3myod1IpJHs7EBkCzhTnCJulN5aR3W0m431nd3i39PZKj1sPUnSonJNExaSn6a3U5Y3XIFL8r3kRuRfi2aeaAT2fMA1RMyXUgGYbrZieD54OfC5hEiisCMqqnsftsc=";

        private readonly Lazy<IDllBase> m_edpdll;
    }
}
