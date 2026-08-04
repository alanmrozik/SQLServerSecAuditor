namespace SqlSecAuditor.Models
{
    public sealed class SavedConnection
    {
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
    }
}
