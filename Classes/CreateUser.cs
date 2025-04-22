using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace BuiKuVPN.Classes
{  
    //cu ajutorul Lui Dumnezeu am reusit sa-l fac sa mearga la 28 Mai ora 01:02 a.m.
    public class CreateUser
    {
        private static string _username = HelperUtilities.Username;
        private static string _password = HelperUtilities.GetDecodedPassword();
        private static readonly string _vpnLDAP = ConfigurationManager.AppSettings["vpnLDAP"];


        public static string Create(string firstName, string lastName, string userLogonName, SecureString password, string staticIPAddress)
        {
            using (var runspace = RunspaceFactory.CreateRunspace(CreateWSManConnectionInfo()))
            {
                runspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    string script = @"
                        try {
                            # Convert the static IP address to its integer representation
                            $ipAddress = [System.Net.IPAddress]::Parse('" + staticIPAddress + @"')
                            $ipBytes = $ipAddress.GetAddressBytes()
                            [System.Array]::Reverse($ipBytes)
                            $ipAddressInt = [System.BitConverter]::ToUInt32($ipBytes, 0)
                        } catch {
                            Write-Host 'Error occurred while parsing IP address: $ipAddressInt' 
                        }

                        # Create the user in Active Directory
                        $newUser = New-ADUser -Name '" + firstName + @" " + lastName + @"' `
                                   -GivenName '" + firstName + @"' `
                                   -Surname '" + lastName + @"' `
                                   -SamAccountName '" + userLogonName + @"' `
                                   -UserPrincipalName '" + userLogonName + @"@integrisoft.hub' `
                                   -AccountPassword (ConvertTo-SecureString -String '" + ConvertToUnsecureString(password) + @"' -AsPlainText -Force) `
                                   -Path 'OU=VPN Accounts,DC=integrisoft,DC=hub' `
                                   -Enabled $true `
                                   -CannotChangePassword $true `
                                   -PasswordNeverExpires $true `
                                   -PassThru

                        # Add the user to the VPN Clients group
                        Add-ADGroupMember -Identity 'VPN Clients' -Members $newUser

                        # Assign the static IP address to the user in Dial-in properties
                        Set-ADUser -Identity $newUser -Server 'INTEGRISOFT.HUB' -Replace @{msRADIUSFramedIPAddress = $ipAddressInt}

                        $newUser | ConvertTo-Json -Depth 1
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
                        var newUserJson = result[0].BaseObject.ToString();
                        var newUser = JsonConvert.DeserializeObject<CreatedUser>(newUserJson);
                        return $"User '{newUser.SamAccountName}' created successfully.";
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

        private static string ConvertToUnsecureString(SecureString securePassword)
        {
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }

    public class CreatedUser
    {
        public string SamAccountName { get; set; }
    }
}
