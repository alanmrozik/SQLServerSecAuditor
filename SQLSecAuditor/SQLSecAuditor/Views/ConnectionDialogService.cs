using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Windows;

namespace SqlSecAuditor.Views
{
    public sealed class ConnectionDialogService(Window owner) : IConnectionDialogService
    {
        public SqlInstance? ShowConnectionDialog(SavedConnection? savedConnection = null)
        {
            var dialog = new ConnectionWindow(savedConnection) { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.ResultInstance : null;
        }
    }
}
