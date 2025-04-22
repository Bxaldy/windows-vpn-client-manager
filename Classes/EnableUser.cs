using Newtonsoft.Json;
using System;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Configuration;

namespace BuiKuVPN.Classes
{
    public class EnableUser
    {
        private static string _username = HelperUtilities.Username;
        private static string _password = HelperUtilities.GetDecodedPassword();
        private static readonly string _vpnLDAP = ConfigurationManager.AppSettings["vpnLDAP"];


        public static string Enable(string userLogonName)
        {
            using (var runspace = RunspaceFactory.CreateRunspace(CreateWSManConnectionInfo()))
            {
                runspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    string script = @"
                    Enable-ADAccount -Identity '" + userLogonName + @"'
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
                        return $"User '{user.SamAccountName}' enabled successfully.";
                    }
                }
            }
        }

        private static WSManConnectionInfo CreateWSManConnectionInfo()
        {
            SecureString securePassword = HelperUtilities.GetSecurePassword();
            PSCredential cred = new PSCredential(_username, securePassword);

            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(new Uri("_vpnLDAP"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", cred);

            //daca expira certificatul server-ului nu se mai pot administra userii
            //trebuie reinnoit certificatul pentru wsman cu noul thumbprint al noului certificat :
            // se intra in powershell se sterge vechiul listener :
            // # winrm delete winrm / config / Listener ? Address = *+Transport = HTTPS
            // recream listener-ul cu noul thumbprint al certificatului
            // # New-Item -Path WSMan:\Localhost\Listener -Transport HTTPS -CertificateThumbprint "2a4641cb44c6a4e72276287a3169b7cf62d3038e" -Address * -Force
            // verificam daca e ok cu : # winrm enumerate winrm/config/Listener

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
