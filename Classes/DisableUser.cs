using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace BuiKuVPN.Classes
{
    public class DisableUser
    {
        private static readonly string _vpnLDAP = ConfigurationManager.AppSettings["vpnLDAP"];

        public static string Disable(string userLogonName)
        {

            using (var runspace = RunspaceFactory.CreateRunspace(CreateWSManConnectionInfo()))
            {
                runspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    string script = @"
                    Disable-ADAccount -Identity '" + userLogonName + @"'
                    Get-ADUser -Identity '" + userLogonName + @"' | ConvertTo-Json -Depth 1
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
                        var userJson = result[0].BaseObject.ToString();
                        var user = JsonConvert.DeserializeObject<CreatedUser>(userJson);
                        return $"User '{user.SamAccountName}' disabled successfully.";
                    }
                }
            }
        }

        private static WSManConnectionInfo CreateWSManConnectionInfo()
        {
            SecureString securePassword = HelperUtilities.GetSecurePassword();
            PSCredential cred = new PSCredential(HelperUtilities.Username, securePassword);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(
                new Uri(_vpnLDAP),
                "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                cred
            );
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            return connectionInfo;
        }
    }
}
