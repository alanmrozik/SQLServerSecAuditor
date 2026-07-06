using System.Collections.ObjectModel;
using System.Windows;

namespace SqlSecAuditor
{
    // Klasa reprezentująca pełne dane audytowe pojedynczego SQL Servera
    public class SqlInstance
    {
        public string ServerName { get; set; }
        public string GenMachineName { get; set; }
        public string GenInstanceName { get; set; }
        public string GenEdition { get; set; }
        public string GenProductVersion { get; set; }
        public string GenProductLevel { get; set; }
        public string GenUptime { get; set; }
        public string GenLastUpdate { get; set; }
        public string AuthPermissionsInfo { get; set; }
        public string AuthenAccessControlInfo { get; set; }
        public string DatabaseSecurityInfo { get; set; }
        public string SurfaceAreaReductionInfo { get; set; }
        public string NetworkConnectivityInfo { get; set; }
        public string HighAvailabilityInfo { get; set; }
        public string MaintenanceIntegrityInfo { get; set; }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<SqlInstance> Instances { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Instances = new ObservableCollection<SqlInstance>();
            this.DataContext = this;

            // Dodanie przykładowych danych na podstawie Twojego skryptu T-SQL
            Instances.Add(new SqlInstance
            {
                ServerName = "LOCALHOST\\SQLEXPRESS",

                // Dane do ładnej listy w sekcji General
                GenMachineName = "DESKTOP-PRO-01",
                GenInstanceName = "SQLEXPRESS",
                GenEdition = "Express Edition (64-bit)",
                GenProductVersion = "16.0.1000.6",
                GenProductLevel = "RTM",
                GenUptime = "2026-07-01 08:34:12", // Symulacja daty utworzenia tempdb
                GenLastUpdate = "2026-05-15 14:20:00",

                AuthPermissionsInfo = "SELECT * FROM sys.server_principals WHERE is_disabled = 0;\nZnaleziono 3 konta z uprawnieniami sysadmin.",
                AuthenAccessControlInfo = "SELECT name, is_policy_checked FROM sys.sql_logins;",
                DatabaseSecurityInfo = "SELECT name, containment FROM sys.databases;",
                SurfaceAreaReductionInfo = "EXEC sp_configure 'show advanced options', 1;",
                NetworkConnectivityInfo = "SELECT protocol_name FROM sys.endpoints;",
                HighAvailabilityInfo = "SELECT recovery_model_desc FROM sys.databases;",
                MaintenanceIntegrityInfo = "DBCC CHECKDB WITH NO_INFOMSGS;"
            });
            Instances.Add(new SqlInstance
            {
                ServerName = "VM2-TEST",

                // Dane do ładnej listy w sekcji General
                GenMachineName = "VM2-TEST",
                GenInstanceName = "MSSQLSERVER",
                GenEdition = "Standard Edition (64-bit)",
                GenProductVersion = "16.0.1000.6",
                GenProductLevel = "RTM-CU25",
                GenUptime = "2026-07-01 08:34:12", // Symulacja daty utworzenia tempdb
                GenLastUpdate = "2026-05-15 14:20:00",

                AuthPermissionsInfo = "SELECT * FROM sys.server_principals WHERE is_disabled = 0;\nZnaleziono 3 konta z uprawnieniami sysadmin.",
                AuthenAccessControlInfo = "SELECT name, is_policy_checked FROM sys.sql_logins;",
                DatabaseSecurityInfo = "SELECT name, containment FROM sys.databases;",
                SurfaceAreaReductionInfo = "EXEC sp_configure 'show advanced options', 1;",
                NetworkConnectivityInfo = "SELECT protocol_name FROM sys.endpoints;",
                HighAvailabilityInfo = "SELECT recovery_model_desc FROM sys.databases;",
                MaintenanceIntegrityInfo = "DBCC CHECKDB WITH NO_INFOMSGS;"
            });
        }
    }
}