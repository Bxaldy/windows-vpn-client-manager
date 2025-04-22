using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Windows.Threading;
using BuiKuVPN.Classes;
using System.Security;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Runtime.Intrinsics.X86;
using System.Management.Automation;
using System.Net.Sockets;


namespace BuiKuVPN
{

    public partial class SysMainWindow : Window
    {

        //Conn string
        //
        private readonly string _dataSource = ConfigurationManager.ConnectionStrings["DataSource"].ConnectionString;
        private readonly string _database = "Initial Catalog=vpn_clienti";
        private readonly string _userId = "User Id=sa";
        private readonly string _password = "Password=26.Februarie!";
        private readonly string _connectionString;
        //
        //
        private DataTable dataTable;
        private DispatcherTimer refreshTimer;
        public string SearchText { get; set; }
        private bool _isPasswordVisible;

        public SysMainWindow()
        {
            InitializeComponent();
            _connectionString = $"{_dataSource};{_database};{_userId};{_password}";
            dataGrid.AutoGenerateColumns = false;
            this.Title = AppInfo.Version;
            LoadData();
            SearchBox.TextChanged += SearchBox_TextChanged;

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(30);
            refreshTimer.Tick += (sender, e) => RefreshData();
            refreshTimer.Start();
            DataContext = this;
            PasswordText = string.Empty;
        }
        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT IPAddress, Username, " +
                                   "CASE WHEN OnlineStatus = 1 THEN 'online' ELSE 'offline' END AS OnlineStatus " +
                                   "FROM clienti " +
                                   "ORDER BY Username ASC;";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGrid.ItemsSource = dataTable.DefaultView;
                    ApplyFilter();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data from the database: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            try
            {
                string filterExpression = "";
                if (!string.IsNullOrEmpty(SearchText))
                {
                    filterExpression = $"IPAddress LIKE '%{SearchText}%' OR Username LIKE '%{SearchText}%'";
                }
                dataTable.DefaultView.RowFilter = filterExpression;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying filter: {ex.Message}");
                MessageBox.Show($"Error applying filter: {ex.Message}");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = SearchBox.Text;
            ApplyFilter();
        }


        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            LoadData();
        }
        private void CopyIP_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();
                Clipboard.Clear();
                try
                {
                    Clipboard.SetDataObject(ipAddress, true);
                }
                catch (COMException)
                {
                    MessageBox.Show("Failed to copy to clipboard after several attempts. Please try again.", "Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //TrySetClipboardData(ipAddress);         // in caz de nefericiri cu cliboard-ul
            }
        }

        // in caz de nefericiri cu cliboard-ul
        //
        //private void TrySetClipboardData(string text)
        //{
        //    const int maxRetries = 5;
        //    const int delayMilliseconds = 100;
        //    int attempts = 0;

        //    while (attempts < maxRetries)
        //    {
        //        try
        //        {
        //            bool success = false;
        //            Thread thread = new Thread(() =>
        //            {
        //                try
        //                {
        //                    Clipboard.SetDataObject(text, true);
        //                    success = true;
        //                }
        //                catch (COMException)
        //                {
        //                    success = false;
        //                }
        //            });
        //            thread.SetApartmentState(ApartmentState.STA);
        //            thread.Start();
        //            thread.Join();

        //            if (success)
        //            {
        //                break;
        //            }
        //        }
        //        catch (COMException ex) when ((uint)ex.HResult == 0x800401D0)
        //        {
        //            attempts++;
        //            Thread.Sleep(delayMilliseconds);
        //        }
        //    }

        //    if (attempts == maxRetries)
        //    {
        //        MessageBox.Show("Failed to copy to clipboard after several attempts. Please try again.", "Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void RemoteDesktop_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();
                System.Diagnostics.Process.Start("mstsc", $"/v:{ipAddress}");
            }
        }

        private void Ping_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();
                System.Diagnostics.Process.Start("ping", $"{ipAddress} -t");
            }
        }
        private void RemoteTerminal_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();

                // Prompt user for SSH username
                string userName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter SSH username:",
                    "Remote Terminal",
                    "Administrator" // Default username
                );

                if (!string.IsNullOrEmpty(userName))
                {
                    // Prompt user to choose action
                    var dialogResult = MessageBox.Show(
                        "Rulezi Script Remote (Yes) sau vrei sa intri in Powershell (No)?",
                        "Choose Action",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question
                    );

                    if (dialogResult == MessageBoxResult.Yes) // Run a script on the server
                    {
                        // Select script file to upload and execute
                        Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "PowerShell Scripts (*.ps1)|*.ps1",
                            Title = "Select PowerShell Script"
                        };

                        bool? fileResult = fileDialog.ShowDialog();
                        if (fileResult == true && !string.IsNullOrEmpty(fileDialog.FileName))
                        {
                            string scriptPath = fileDialog.FileName;

                            try
                            {
                                // Combine scp and ssh into a single PowerShell process
                                string tempFileName = "C:/Temp/remote_script.ps1"; // Path on the server
                                string combinedCommand = $@"
                                                            scp ""{scriptPath}"" {userName}@{ipAddress}:{tempFileName};
                                                            ssh {userName}@{ipAddress} 'powershell -ExecutionPolicy Bypass -File {tempFileName}'";

                                // Run the combined command in a single PowerShell session
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "powershell.exe",
                                    Arguments = $"-NoExit -Command \"{combinedCommand}\"",
                                    CreateNoWindow = false,
                                    UseShellExecute = false
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to transfer or execute script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No script selected. Operation canceled.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (dialogResult == MessageBoxResult.No) // Start interactive SSH session
                    {
                        try
                        {
                            // Launch SSH session
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c start ssh {userName}@{ipAddress} -t powershell",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to open SSH session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to open a terminal.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }





        //private void TerminalIse_Click(object sender, RoutedEventArgs e)
        //{
        //    if (dataGrid.SelectedItem is DataRowView selectedRow)
        //    {
        //        string ipAddress = selectedRow["IPAddress"].ToString();

        //        // Prompt user for SSH username
        //        string userName = Microsoft.VisualBasic.Interaction.InputBox(
        //            "Enter SSH username:",
        //            "Remote Terminal",
        //            "Administrator" // Default username
        //        );

        //        if (!string.IsNullOrEmpty(userName))
        //        {
        //            try
        //            {
        //                // Crearea fișierului temporar PowerShell
        //                string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ssh_session.ps1");
        //                string sshCommand = $"ssh {userName}@{ipAddress}";

        //                // Scrierea comenzii SSH în fișier
        //                System.IO.File.WriteAllText(tempFilePath, sshCommand);

        //                // Deschiderea fișierului în PowerShell ISE
        //                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        //                {
        //                    FileName = "powershell_ise.exe",
        //                    Arguments = $"\"{tempFilePath}\"",
        //                    CreateNoWindow = true,
        //                    UseShellExecute = false
        //                });
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Failed to open SSH session in PowerShell ISE: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Please select a row to open a terminal.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();
                string username = selectedRow["Username"].ToString();

                // Prompt user for confirmation
                MessageBoxResult result = MessageBox.Show("Esti sigur ca vrei sa stergi inregistrarea?", "Confirma", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();
                            string query = "DELETE FROM clienti WHERE IPAddress = @IPAddress AND Username = @Username;";

                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@IPAddress", ipAddress);
                            command.Parameters.AddWithValue("@Username", username);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Inregistrare stearsa! Modificarile vor aparea in cateva secunde.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadData(); // Refresh data after deletion
                            }
                            else
                            {
                                MessageBox.Show("No rows affected. Entry not found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item from the DataGrid
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                string ipAddress = selectedRow["IPAddress"].ToString();

                // Start scanning ports asynchronously
                await ScanPortsAsync(ipAddress, 1443, 65535);
            }
            else
            {
                MessageBox.Show("Please select a row to scan ports.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task ScanPortsAsync(string target, int startPort, int endPort)
        {
            // Show a progress dialog or status message
            MessageBox.Show($"Scanning ports {startPort}-{endPort} on {target}...", "Port Scan", MessageBoxButton.OK, MessageBoxImage.Information);

            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync(target, port);
                        MessageBox.Show($"Port {port} is open on {target}.", "Port Scan", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (SocketException)
                {
                    // Port is closed or filtered
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error scanning port {port}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show($"Port scan completed for {target}.", "Port Scan", MessageBoxButton.OK, MessageBoxImage.Information);
        }
           


        //private bool _isPasswordVisible;
        private string _passwordText;


        //private void CreateUser_Click(object sender, RoutedEventArgs e)
        //{
        //string firstName = FirstNameTextBox.Text;
        //string lastName = LastNameTextBox.Text;
        //string userLogonName = UserLogonNameTextBox.Text;
        //string memberOf = MemberOfTextBox.Text;
        //string staticIPAddress = StaticIPAddressTextBox.Text;

        //try
        //{
        //    // Crearea utilizatorului
        //    CreateUser.CreateNewUser(firstName, lastName, userLogonName, memberOf, staticIPAddress);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Eroare la crearea utilizatorului: {ex.Message}");
        //    // Afișează un mesaj de eroare utilizatorului sau gestionează altfel excepția
        //}
        //}


        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged(nameof(IsPasswordVisible));
            }
        }

        public string PasswordText
        {
            get => _passwordText;
            set
            {
                _passwordText = value;
                OnPropertyChanged(nameof(PasswordText));
            }
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string userLogonName = UserLogonNameTextBox.Text;
            string password = IsPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password; // Get password from the appropriate field
            SecureString securePassword = ConvertToSecureString(password); // Convert the password to SecureString
            string staticIPAddress = StaticIPAddressTextBox.Text;

            // Verificarea dacă toate câmpurile sunt completate
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(userLogonName) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(staticIPAddress))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            // Crearea utilizatorului
            string result = CreateUser.Create(firstName, lastName, userLogonName, securePassword, staticIPAddress);
            MessageBox.Show(result);
        }

        // Metodă pentru a converti un șir în SecureString
        private SecureString ConvertToSecureString(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return null;
            SecureString securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!IsPasswordVisible)
            {
                PasswordText = PasswordBox.Password;
            }
        }
        //private void OnKeyDownHandler(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Return)
        //    {
        //        CreateUser_Click(sender, e);
        //    }
        //}

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Text = PasswordBox.Password;
                IsPasswordVisible = true;
            }
            else
            {
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Password = PasswordTextBox.Text;
                IsPasswordVisible = false;
            }

            (sender as Button).Content = IsPasswordVisible ? "👁" : "👁‍🗨";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
   

    private void Enable_User_Click(object sender, RoutedEventArgs e)
        {
            string userLogonName = UserLogonNameDel.Text;
            if (string.IsNullOrWhiteSpace(userLogonName))
            {
                MessageBox.Show("Please enter a user logon name.");
                return;
            }

            string result = EnableUser.Enable(userLogonName);
            MessageBox.Show(result);
        }

        private void Disable_User_Click(object sender, RoutedEventArgs e)
        {
            string userLogonName = UserLogonNameDel.Text;
            if (string.IsNullOrWhiteSpace(userLogonName))
            {
                MessageBox.Show("Please enter a user logon name.");
                return;
            }

            string result = DisableUser.Disable(userLogonName);
            MessageBox.Show(result);
        }

        private void Delete_User_Click(object sender, RoutedEventArgs e)
        {
            string userLogonName = UserLogonNameDel.Text;
            if (string.IsNullOrWhiteSpace(userLogonName))
            {
                MessageBox.Show("Please enter a user logon name.");
                return;
            }

            string result = DeleteUser.Delete(userLogonName);
            MessageBox.Show(result);
        }



    private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void dataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ControlTemplate_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            // Clear text in all text boxes
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            UserLogonNameTextBox.Text = "";
            StaticIPAddressTextBox.Text = "";
            MemberOfTextBox.Text = "VPN Clients";

            // Clear text in password box
            PasswordBox.Clear();
            PasswordTextBox.Text = "";
        }
    }
}