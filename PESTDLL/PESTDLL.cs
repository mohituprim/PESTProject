// Do not modify this file

using System.Runtime.InteropServices;
using FactSet.Partners.Lib_EDP;

namespace FactSet.Partners.PESTDLL
{
    // to call that from powershell: $myComObject = New-Object -ComObject FactSet.Partners.DllBase.DLLWRAPPER
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("fc22a512-69fd-485c-be51-bf6286cfbf96")]
    public class PESTDLL : DllBase
    {
        protected override IDllBase CreateDllBase()
        {
            return new PESTDLLPImp();
        }

        protected override string ModuleName()
        {
            return "PESTDLL";
        }
    }
}