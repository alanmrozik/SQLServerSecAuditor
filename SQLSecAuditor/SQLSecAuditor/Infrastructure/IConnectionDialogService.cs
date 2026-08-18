using SqlSecAuditor.Models;

namespace SqlSecAuditor.Infrastructure
{
    /// <summary>Abstraction over the UI flow used to create a SQL connection.</summary>
    public interface IConnectionDialogService
    {
        SqlInstance? ShowConnectionDialog();
    }
}
