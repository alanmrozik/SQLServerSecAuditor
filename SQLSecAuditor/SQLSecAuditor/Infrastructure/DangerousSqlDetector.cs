using System.Text.RegularExpressions;

namespace SqlSecAuditor.Infrastructure
{
    public static class DangerousSqlDetector
    {
        private static readonly (Regex Pattern, string Description)[] Rules =
        {
            (new Regex(@"\b(INSERT|UPDATE|DELETE|MERGE|TRUNCATE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "a data modification statement (DML)"),
            (new Regex(@"\b(CREATE|ALTER|DROP)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "a database structure modification statement (DDL)"),
            (new Regex(@"\b(GRANT|REVOKE|DENY)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "a permission modification statement"),
            (new Regex(@"\b(EXEC|EXECUTE|SP_CONFIGURE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "a stored procedure or administrative command"),
            (new Regex(@"\b(XP_CMDSHELL|POWERSHELL|PWSH|CMD(?:\.EXE)?|START-PROCESS)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), "a system command execution element")
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
