using System.ComponentModel;

public class Client : INotifyPropertyChanged
{
    private string _onlineStatus;

    public string IPAddress { get; set; }
    public string Username { get; set; }

    public string OnlineStatus
    {
        get => _onlineStatus;
        set
        {
            if (_onlineStatus != value)
            {
                _onlineStatus = value;
                OnPropertyChanged(nameof(OnlineStatus));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
