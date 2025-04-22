using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BuiKuVPN.Classes
{
    internal class HelperUtilities
    {
        private static readonly string _username = "Administrator";
        private static readonly string _encodedPassword = "encodedPassword"; 

        public static string Username => _username;

        public static string GetDecodedPassword()
        {
            var decodedBytes = Convert.FromBase64String(_encodedPassword);
            return Encoding.UTF8.GetString(decodedBytes);
        }

        public static SecureString GetSecurePassword()
        {
            string decodedPassword = GetDecodedPassword();
            SecureString securePassword = new SecureString();
            foreach (char c in decodedPassword)
            {
                securePassword.AppendChar(c);
            }
            return securePassword;
        }
    }
}
