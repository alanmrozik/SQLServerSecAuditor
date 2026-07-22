using System.Collections.ObjectModel;
using SqlSecAuditor.Models;

namespace SqlSecAuditor.ViewModels
{
    public class MainViewModel
    {
        // Dynamiczna lista, którą obserwuje XAML
        public ObservableCollection<SqlInstance> Instances { get; set; }

        public MainViewModel()
        {
            Instances = new ObservableCollection<SqlInstance>();

            // Testowe dodanie elementów na start (symulacja połączenia)
            AddNewInstance("LOCALHOST\\SQLEXPRESS", "Wersja: SQL Server 2022. Status: Online.", "Znaleziono 3 podatności w rolach sysadmin.");
            AddNewInstance("192.168.1.50,1433", "Wersja: SQL Server 2019. Połączenie szyfrowane.", "Brak krytycznych błędów uprawnień.");
        }

        // Tę metodę wywołasz np. po kliknięciu w menu "Połącz z bazą" i podaniu danych przez użytkownika
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