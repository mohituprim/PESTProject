// Do not modify this file

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.AccessControl;

namespace FactSet.Partners.Lib_EDP
{
    // To control dependancies, EDP dlls are only callable through this wrapper
    // EDP conventions:
    //   - EDP dlls must have the same name as their project
    //   - EDP modules must be at the same base directory
    // this way the wrapper only needs to know the short name of the dll to wrap
    public class DllWrapper
    {
        public DllWrapper(string dllShortNameWithoutExt)
        {
            AppDomainSetup adSetup = new AppDomainSetup();
            string globalBaseFolder = AppDomain.CurrentDomain.BaseDirectory + "../../..";
            adSetup.ApplicationBase = globalBaseFolder;
            adSetup.PrivateBinPath = new Uri(globalBaseFolder).MakeRelativeUri(new Uri(AppDomain.CurrentDomain.BaseDirectory)).ToString();

            EvidenceBase[] hostEvidence = { new Zone(SecurityZone.Intranet) };
            Evidence evidence = new Evidence(hostEvidence, null);
            PermissionSet permSet = SecurityManager.GetStandardSandbox(evidence);

            FileIOPermission fileRights = new FileIOPermission(PermissionState.None)
            {
                AllLocalFiles = FileIOPermissionAccess.AllAccess,
                AllFiles = FileIOPermissionAccess.AllAccess,
            };
            permSet.AddPermission(fileRights);
            permSet.AddPermission(new SecurityPermission(PermissionState.Unrestricted));

            AppDomain newDomain = AppDomain.CreateDomain("Sandbox", evidence, adSetup, permSet);

            m_dllcallerRestricted = (DllCaller)newDomain.CreateInstanceAndUnwrap(
                typeof(DllCaller).Assembly.FullName, // assemblyName
                typeof(DllCaller).FullName, // typeName
                false, // ignoreCase
                BindingFlags.Default, // bindingAttr
                null, // binder
                new object[] { dllShortNameWithoutExt }, // constructor arguments
                null, // culture
                null // activationAttributes
            );
        }

        public string FctDataQuery(string shortQuery)
        {
            if (m_dllcallerRestricted == null)
                return "";
            return m_dllcallerRestricted.ExecuteUntrustedCode("FctDataQuery", new object[] { Checker + shortQuery });
        }

        public string FctInvoke(string xmlquery)
        {
            if (m_dllcallerRestricted == null)
                return "";
            return m_dllcallerRestricted.ExecuteUntrustedCode("FctInvoke", new object[] { Checker + xmlquery });
        }

        private const string Checker =
            "7XZHtm+iQOmM3WvAVt43mwNL21hFpU7LpA72IruwT0BAS6hH6KJLGoWvEU623WoiHZXgGAuSRjq/SJlKrB9j8nOB5XCp+UIYm3eQ/fOexK1g8GOPXnVFiaV5nvBGyM/rs/Ist2yLQ4GqdqGZq39OkFaV9jelUElzplum86QmA8A+I34yT/JEybi+m+sTIV78pQC5ND2uSm+6S4V01HIIFESU3mPWKU51pt6eoqihTZJ5WJK1LOlLD7HLQiRIuzcwb7LiZ9TzTVir5xhTi0Gr+XR94KAggEWFuiU8/Ge5MdHgG3myod1IpJHs7EBkCzhTnCJulN5aR3W0m431nd3i39PZKj1sPUnSonJNExaSn6a3U5Y3XIFL8r3kRuRfi2aeaAT2fMA1RMyXUgGYbrZieD54OfC5hEiisCMqqnsftsc=";

        private readonly DllCaller m_dllcallerRestricted;

        private class DllCaller : MarshalByRefObject
        {
            public DllCaller(string dllShortNameWithoutExt)
            {
                string pathCaller = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
                string pathDllToLoad = pathCaller + "../../../" + dllShortNameWithoutExt + "/bin/";
#if DEBUG
                pathDllToLoad += "Debug/";
#else
                pathDllToLoad += "Release/";
#endif
                string dllCompleteFileName = Path.Combine(pathDllToLoad, dllShortNameWithoutExt + ".dll");

                var dllToLoad = Assembly.LoadFrom(dllCompleteFileName);
                var dllBaseTypes =
                    dllToLoad.GetExportedTypes().Where(type =>
                        type.GetMethod("FctDataQuery") != null
                        && type.GetMethod("FctInvoke") != null
                        && !type.IsAbstract).ToList();
                if (dllBaseTypes.Count() != 1)
                {
                    throw new ArgumentException(
                        dllBaseTypes.Count() + " class" + (dllBaseTypes.Count() > 1 ? "es" : "") + " implement IDLLBASE in " + dllShortNameWithoutExt
                        + ".\nThere should be only 1.");
                }

                m_dllbaseTypeRestricted = dllBaseTypes.First();
                var constructorInfo = m_dllbaseTypeRestricted.GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                    m_dllbaseRestricted = constructorInfo.Invoke(new object[] { });
            }

            public string ExecuteUntrustedCode(string methodName, Object[] parameters)
            {
                return (string)m_dllbaseTypeRestricted.GetMethod(methodName).Invoke(m_dllbaseRestricted, parameters);
            }

            private readonly object m_dllbaseRestricted;
            private readonly Type m_dllbaseTypeRestricted;
        }
    }
}