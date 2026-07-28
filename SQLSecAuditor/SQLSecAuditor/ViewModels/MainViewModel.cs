using System.Collections.ObjectModel;
using System.Windows.Input;
using SqlSecAuditor.Models;

namespace SqlSecAuditor.ViewModels
{
    public class MainViewModel
    {
        // ...
        public ObservableCollection<SqlInstance> Instances { get; set; }

        public ICommand ConnectNewDatabaseCommand { get; }

        public MainViewModel()
        {
            Instances = new ObservableCollection<SqlInstance>();

            ConnectNewDatabaseCommand = new RelayCommand(ExecuteConnectNewDatabase);

            // Testowe dodanie elementów na start
            AddNewInstance("LOCALHOST\\SQLEXPRESS", "Wersja: SQL Server 2022. Status: Online.", "Znaleziono 3 podatności w rolach sysadmin.");
            AddNewInstance("192.168.1.50,1433", "Wersja: SQL Server 2019. Połączenie szyfrowane.", "Brak krytycznych błędów uprawnień.");
        }

        private void ExecuteConnectNewDatabase(object obj)
        {
            // Placeholder: po kliknięciu symulujemy "połączenie"
            // Tutaj docelowo wywołasz np. okienko typu "ConnectWindow" by podać poświadczenia
            AddNewInstance($"NEW_SERVER_{Instances.Count + 1}", "Wersja: SQL Server 2022. Status: Oczekujący.", "Brak danych, rozpocznij skanowanie.");
        }

        public void AddNewInstance(string name, string generalInfo, string permissionsInfo)
        {
            Instances.Add(new SqlInstance
            {
                ServerName = name,
                GeneralInfo = generalInfo,
                PermissionsInfo = permissionsInfo
            });
        }
    }
}