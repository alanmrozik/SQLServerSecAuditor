using System.Text.RegularExpressions;

namespace SqlSecAuditor.Infrastructure
{
    public static class DangerousSqlDetector
    {
        private static readonly (Regex Pattern, string Description)[] Rules =
        {
            (new Regex(@"\b(INSERT|UPDATE|DELETE|MERGE|TRUNCATE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "operację modyfikacji danych (DML)"),
            (new Regex(@"\b(CREATE|ALTER|DROP)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "operację zmiany struktury bazy (DDL)"),
            (new Regex(@"\b(GRANT|REVOKE|DENY)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "operację zmiany uprawnień"),
            (new Regex(@"\b(EXEC|EXECUTE|SP_CONFIGURE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "wywołanie procedury lub polecenia administracyjnego"),
            (new Regex(@"\b(XP_CMDSHELL|POWERSHELL|PWSH|CMD(?:\.EXE)?|START-PROCESS)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "element powiązany z uruchamianiem poleceń systemowych")
        };

        public static IReadOnlyList<string> FindRisks(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return Array.Empty<string>();
            }

            return Rules
                .Where(rule => rule.Pattern.IsMatch(sql))
                .Select(rule => rule.Description)
                .ToArray();
        }
    }
}
