using BuiKuVPN.Classes;
using System;
using System.Configuration;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BuiKuVPN
{
    public partial class LoginWindow : Window
    {
        private readonly string _dcSource = ConfigurationManager.AppSettings["dcSource"];
        private int _failedLoginAttempts = 0;
        private const int MaxFailedAttempts = 10;

        public LoginWindow()
        {
            InitializeComponent();
            this.Title = AppInfo.Version;

            UsernameTextBox.GotFocus += UsernameTextBox_GotFocus;
            UsernameTextBox.LostFocus += UsernameTextBox_LostFocus;
            PasswordBox.GotFocus += PasswordBox_GotFocus;
            PasswordBox.LostFocus += PasswordBox_LostFocus;

            // Initialize placeholders
            UsernameTextBox_LostFocus(null, null);
            PasswordBox_LostFocus(null, null);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_failedLoginAttempts >= MaxFailedAttempts)
            {
                MessageBox.Show("Too many failed login attempts. Please contact the administrator.");
                return;
            }

            string username = UsernameTextBox.Text.Trim();
            SecureString securePassword = PasswordBox.SecurePassword;

            if (string.IsNullOrEmpty(username) || securePassword.Length == 0)
            {
                MessageBox.Show("Username and password are required.");
                return;
            }

            string userRole = GetUserRole(username, securePassword);
            if (userRole != null)
            {
                _failedLoginAttempts = 0; // Reset on successful login
                if (userRole == "SysAdmin")
                {
                    SysMainWindow sysAdminMainWindow = new SysMainWindow();
                    sysAdminMainWindow.Show();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                this.Close(); // Close the login window
            }
            else
            {
                _failedLoginAttempts++;
                MessageBox.Show("Authentication failed! Please check your username and password. Ensure your password has not expired, otherwise contact SysAdmin.");
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LoginButton_Click(sender, e);
            }
        }

        private string GetUserRole(string username, SecureString securePassword)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry(_dcSource, username, SecureStringToString(securePassword)))
                {
                    object nativeObject = entry.NativeObject;

                    using (DirectorySearcher searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = $"(SAMAccountName={username})";
                        searcher.PropertiesToLoad.Add("memberOf");

                        SearchResult result = searcher.FindOne();

                        if (result != null)
                        {
                            foreach (var group in result.Properties["memberOf"])
                            {
                                if (group.ToString().Contains("SysAdmin"))
                                {
                                    return "SysAdmin";
                                }
                                else if (group.ToString().Contains("vpn_app"))
                                {
                                    return "vpn_app";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}");
            }
            return null;
        }

        private string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "Username")
            {
                UsernameTextBox.Text = "";
                UsernameTextBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameTextBox.Text = "Username";
                UsernameTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == "Password")
            {
                PasswordBox.Password = "";
                PasswordBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordBox.Password = "Password";
                PasswordBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}