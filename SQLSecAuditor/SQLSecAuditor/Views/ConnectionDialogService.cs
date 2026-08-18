using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Windows;

namespace SqlSecAuditor.Views
{
    public sealed class ConnectionDialogService(Window owner) : IConnectionDialogService
    {
        public SqlInstance? ShowConnectionDialog()
        {
            var dialog = new ConnectionWindow { Owner = owner };
            return dialog.ShowDialog() == true ? dialog.ResultInstance : null;
        }
    }
}
