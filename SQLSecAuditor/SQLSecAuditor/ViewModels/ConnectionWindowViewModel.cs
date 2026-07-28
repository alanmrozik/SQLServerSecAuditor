using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlSecAuditor.ViewModels
{
    public class ConnectionWindowViewModel : INotifyPropertyChanged
    {
        private string _serverName = string.Empty;
        private string _port = "1433";
        private string _databaseName = string.Empty;
        private bool _useWindowsAuthentication = true;
        private bool _useSqlAuthentication;
        private string _sqlUserName = string.Empty;
        private string _password = string.Empty;
        private bool _encryptConnection = true;
        private bool _trustServerCertificate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ServerName
        {
            get => _serverName;
            set => SetProperty(ref _serverName, value);
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string DatabaseName
        {
            get => _databaseName;
            set => SetProperty(ref _databaseName, value);
        }

        public bool UseWindowsAuthentication
        {
            get => _useWindowsAuthentication;
            set
            {
                if (SetProperty(ref _useWindowsAuthentication, value) && value)
                {
                    SetProperty(ref _useSqlAuthentication, false, nameof(UseSqlAuthentication));
                }
            }
        }

        public bool UseSqlAuthentication
        {
            get => _useSqlAuthentication;
            set
            {
                if (SetProperty(ref _useSqlAuthentication, value) && value)
                {
                    SetProperty(ref _useWindowsAuthentication, false, nameof(UseWindowsAuthentication));
                }
            }
        }

        public string SqlUserName
        {
            get => _sqlUserName;
            set => SetProperty(ref _sqlUserName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool EncryptConnection
        {
            get => _encryptConnection;
            set => SetProperty(ref _encryptConnection, value);
        }

        public bool TrustServerCertificate
        {
            get => _trustServerCertificate;
            set => SetProperty(ref _trustServerCertificate, value);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
