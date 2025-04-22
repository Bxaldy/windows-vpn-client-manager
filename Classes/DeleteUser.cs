using System;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace BuiKuVPN.Classes
{
    public class DeleteUser
    {
        private static string _username = HelperUtilities.Username;
        private static string _password = HelperUtilities.GetDecodedPassword();
        private static readonly string _vpnLDAP = ConfigurationManager.AppSettings["vpnLDAP"];


        public static string Delete(string userLogonName)
        {
            using (var runspace = RunspaceFactory.CreateRunspace(CreateWSManConnectionInfo()))
            {
                runspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    string script = @"
                    Remove-ADUser -Identity '" + userLogonName + @"' -Confirm:$false
                ";

                    ps.AddScript(script);

                    var result = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        var error = ps.Streams.Error[0].Exception.Message;
                        return $"Error: {error}";
                    }
                    else
                    {
                        return $"User '{userLogonName}' deleted successfully.";
                    }
                }
            }
        }

        private static WSManConnectionInfo CreateWSManConnectionInfo()
        {
            SecureString securePassword = HelperUtilities.GetSecurePassword();
            PSCredential cred = new PSCredential(_username, securePassword);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri(_vpnLDAP), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", cred);
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            return connectionInfo;
        }

        private static SecureString ConvertToSecureString(string password)
        {
            SecureString securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            return securePassword;
        }
    }
}
