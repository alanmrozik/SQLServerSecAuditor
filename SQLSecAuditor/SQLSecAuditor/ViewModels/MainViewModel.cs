using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SqlSecAuditor.Models;
using SqlSecAuditor.Views;

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
        }

        private void ExecuteConnectNewDatabase(object obj)
        {
            var dialog = new ConnectionWindow
            {
                Owner = Application.Current?.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.ResultInstance is not null)
            {
                Instances.Add(dialog.ResultInstance);
            }
        }

        public void AddNewInstance(string name, string generalInfo)
        {
            Instances.Add(new SqlInstance
            {
                ServerName = name,
                GeneralInfo = generalInfo
            });
        }
    }
}
