using System;

namespace SqlSecAuditor.Models
{
    public sealed class SavedConnection
    {
        public Guid Id { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public string Port { get; set; } = "1433";
        public string DatabaseName { get; set; } = string.Empty;
        public bool UseWindowsAuthentication { get; set; } = true;
        public string SqlUserName { get; set; } = string.Empty;
        public bool EncryptConnection { get; set; } = true;
        public bool TrustServerCertificate { get; set; }

        public string DisplayLabel =>
            string.IsNullOrWhiteSpace(DatabaseName)
                ? $"{ServerName}:{Port}"
                : $"{ServerName}:{Port} [{DatabaseName}]";

        public string AuthenticationLabel => UseWindowsAuthentication
            ? "Windows authentication"
            : $"SQL Server authentication ({SqlUserName})";

        public SavedConnection Copy() => new()
        {
            Id = Id,
            ServerName = ServerName,
            Port = Port,
            DatabaseName = DatabaseName,
            UseWindowsAuthentication = UseWindowsAuthentication,
            SqlUserName = SqlUserName,
            EncryptConnection = EncryptConnection,
            TrustServerCertificate = TrustServerCertificate
        };
    }
}
