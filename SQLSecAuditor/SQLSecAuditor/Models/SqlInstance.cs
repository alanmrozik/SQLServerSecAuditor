using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlSecAuditor.Models
{
    public sealed class SqlInstance : INotifyPropertyChanged
    {
        private bool _isGeneralInfoLoaded;
        private bool _isGeneralInfoLoading;
        private string? _generalInfoError;
        private bool _isMaintenanceIntegrityRunning;
        private string? _maintenanceIntegrityError;
        private bool _isNetworkConnectivityRunning;
        private string? _networkConnectivityError;
        private bool _isSurfaceAreaReductionRunning;
        private string? _surfaceAreaReductionError;
        private bool _isAuditingMonitoringRunning;
        private string? _auditingMonitoringError;
        private bool _isAuthenticationAccessControlRunning;
        private string? _authenticationAccessControlError;
        private bool _isAuthorizationPermissionsRunning;
        private string? _authorizationPermissionsError;
        private bool _isDatabaseSecurityRunning;
        private string? _databaseSecurityError;
        private bool _isHighAvailabilityDisasterRecoveryRunning;
        private string? _highAvailabilityDisasterRecoveryError;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ServerName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public string DisplayName => $"{ServerName} [{DatabaseName}]";

        public ObservableCollection<GeneralInfoEntry> GeneralInfoEntries { get; } = new();

        public ObservableCollection<ScriptExecutionResult> MaintenanceIntegrityResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> NetworkConnectivityResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> SurfaceAreaReductionResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> AuditingMonitoringResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> AuthenticationAccessControlResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> AuthorizationPermissionsResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> DatabaseSecurityResults { get; } = new();

        public ObservableCollection<ScriptExecutionResult> HighAvailabilityDisasterRecoveryResults { get; } = new();

        public bool IsGeneralInfoLoaded
        {
            get => _isGeneralInfoLoaded;
            set => SetProperty(ref _isGeneralInfoLoaded, value);
        }

        public bool IsGeneralInfoLoading
        {
            get => _isGeneralInfoLoading;
            set => SetProperty(ref _isGeneralInfoLoading, value);
        }

        public string? GeneralInfoError
        {
            get => _generalInfoError;
            set => SetProperty(ref _generalInfoError, value);
        }

        public bool IsMaintenanceIntegrityRunning
        {
            get => _isMaintenanceIntegrityRunning;
            set => SetProperty(ref _isMaintenanceIntegrityRunning, value);
        }

        public string? MaintenanceIntegrityError
        {
            get => _maintenanceIntegrityError;
            set => SetProperty(ref _maintenanceIntegrityError, value);
        }

        public bool IsNetworkConnectivityRunning
        {
            get => _isNetworkConnectivityRunning;
            set => SetProperty(ref _isNetworkConnectivityRunning, value);
        }

        public string? NetworkConnectivityError
        {
            get => _networkConnectivityError;
            set => SetProperty(ref _networkConnectivityError, value);
        }

        public bool IsSurfaceAreaReductionRunning
        {
            get => _isSurfaceAreaReductionRunning;
            set => SetProperty(ref _isSurfaceAreaReductionRunning, value);
        }

        public string? SurfaceAreaReductionError
        {
            get => _surfaceAreaReductionError;
            set => SetProperty(ref _surfaceAreaReductionError, value);
        }

        public bool IsAuditingMonitoringRunning
        {
            get => _isAuditingMonitoringRunning;
            set => SetProperty(ref _isAuditingMonitoringRunning, value);
        }

        public string? AuditingMonitoringError
        {
            get => _auditingMonitoringError;
            set => SetProperty(ref _auditingMonitoringError, value);
        }

        public bool IsAuthenticationAccessControlRunning
        {
            get => _isAuthenticationAccessControlRunning;
            set => SetProperty(ref _isAuthenticationAccessControlRunning, value);
        }

        public string? AuthenticationAccessControlError
        {
            get => _authenticationAccessControlError;
            set => SetProperty(ref _authenticationAccessControlError, value);
        }

        public bool IsAuthorizationPermissionsRunning
        {
            get => _isAuthorizationPermissionsRunning;
            set => SetProperty(ref _isAuthorizationPermissionsRunning, value);
        }

        public string? AuthorizationPermissionsError
        {
            get => _authorizationPermissionsError;
            set => SetProperty(ref _authorizationPermissionsError, value);
        }

        public bool IsDatabaseSecurityRunning
        {
            get => _isDatabaseSecurityRunning;
            set => SetProperty(ref _isDatabaseSecurityRunning, value);
        }

        public string? DatabaseSecurityError
        {
            get => _databaseSecurityError;
            set => SetProperty(ref _databaseSecurityError, value);
        }

        public bool IsHighAvailabilityDisasterRecoveryRunning
        {
            get => _isHighAvailabilityDisasterRecoveryRunning;
            set => SetProperty(ref _isHighAvailabilityDisasterRecoveryRunning, value);
        }

        public string? HighAvailabilityDisasterRecoveryError
        {
            get => _highAvailabilityDisasterRecoveryError;
            set => SetProperty(ref _highAvailabilityDisasterRecoveryError, value);
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
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

    public sealed class GeneralInfoEntry
    {
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public sealed class ScriptExecutionResult
    {
        public string ScriptName { get; set; } = string.Empty;

        public ObservableCollection<string> Rows { get; } = new();

        // Kolekcja DataTable — jeden element na każdy zestaw wynikowy zwrócony przez skrypt
        public ObservableCollection<System.Data.DataTable> Tables { get; } = new();

        // Krótki opis/skrócona notatka wyciągnięta z nagłówka pliku .sql
        public string? Description { get; set; }

        public string? Error { get; set; }
    }
}
