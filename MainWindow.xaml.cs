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
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using BuiKuVPN.Classes;
using System.Configuration;


namespace BuiKuVPN
{
    public partial class MainWindow : Window
    {

        private readonly string _dataSource = ConfigurationManager.ConnectionStrings["DataSource"].ConnectionString;
        private readonly string _database = "Initial Catalog=vpn_clienti";
        private readonly string _userId = "User Id=sa";
        private readonly string _password = "Password=Password";
        private readonly string _connectionString;
        //
        //
        private DataTable dataTable;
        private DispatcherTimer refreshTimer;

        public string SearchText { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _connectionString = $"{_dataSource};{_database};{_userId};{_password}";
            this.Title = AppInfo.Version;
            dataGrid.AutoGenerateColumns = false;
            LoadData();
            SearchBox.TextChanged += SearchBox_TextChanged;

            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(30);
            refreshTimer.Tick += (sender, e) => RefreshData();
            refreshTimer.Start();
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

        private void Terminal_Click(object sender, RoutedEventArgs e)
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
                    try
                    {
                        // Launch SSH session with PowerShell
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
            else
            {
                MessageBox.Show("Please select a row to open a terminal.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
    }
}