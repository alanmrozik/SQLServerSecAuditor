using System.Collections.ObjectModel;
using System.Windows;

namespace SqlSecAuditor
{
    // 1. Klasa reprezentująca dane pojedynczego SQL Servera
    public class SqlInstance
    {
        public string ServerName { get; set; }
        public string GeneralInfo { get; set; }
        public string PermissionsInfo { get; set; }
    }

    public partial class MainWindow : Window
    {
        // 2. Dynamiczna lista, którą obserwuje nasz XAML
        public ObservableCollection<SqlInstance> Instances { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Inicjalizujemy listę
            Instances = new ObservableCollection<SqlInstance>();

            // 3. Mówimy oknu, skąd ma brać dane do bindowania ({Binding ...})
            this.DataContext = this;

            // Testowe dodanie elementów na start (symulacja połączenia)
            DodajNowaInstancje("LOCALHOST\\SQLEXPRESS", "Wersja: SQL Server 2022. Status: Online.", "Znaleziono 3 podatności w rolach sysadmin.");
            DodajNowaInstancje("192.168.1.50,1433", "Wersja: SQL Server 2019. Połączenie szyfrowane.", "Brak krytycznych błędów uprawnień.");
        }

        // 4. Tę metodę wywołasz np. po kliknięciu w menu "Połącz z bazą" i podaniu danych przez użytkownika
        public void DodajNowaInstancje(string nazwa, string infoOgolne, string uprawnienia)
        {
            Instances.Add(new SqlInstance
            {
                ServerName = nazwa,
                GeneralInfo = infoOgolne,
                PermissionsInfo = uprawnienia
            });
        }
    }
}