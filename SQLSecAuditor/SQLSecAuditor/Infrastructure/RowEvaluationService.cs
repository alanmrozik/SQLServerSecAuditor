using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace SqlSecAuditor.Infrastructure
{
    public enum RowEvaluation
    {
        None,
        Green,
        Yellow,
        Red
    }

    public static class RowEvaluationService
    {
        public static RowEvaluation Evaluate(string? scriptName, DataTable table, DataRow row)
        {
            var normalizedScript = NormalizeToken(scriptName);
            var rowText = string.Join(" | ", row.ItemArray.Select(ToText)).ToLowerInvariant();

            if (normalizedScript.Contains("encryptionchecks") || normalizedScript.Contains("generalinfoaboutserver"))
                return RowEvaluation.None;

            if (normalizedScript.Contains("builtinlogins")
                || normalizedScript.Contains("expirationforsqlloginsysadmins")
                || normalizedScript.Contains("sysadminlogins")
                || normalizedScript.Contains("guestpermissions")
                || normalizedScript.Contains("permissionsondblevelpoprawiony")
                || normalizedScript.Contains("serviceaccounts")
                || normalizedScript.Contains("sqlserverport"))
                return RowEvaluation.Yellow;

            if (normalizedScript.Contains("orphanedusers")
                || normalizedScript.Contains("publicroleisnotgrantedtoproxies")
                || normalizedScript.Contains("autoclose")
                || normalizedScript.Contains("clrenabled"))
                return RowEvaluation.Red;

            if (normalizedScript.Contains("defaulttraceenabled"))
                return rowText.Contains("enabled") ? RowEvaluation.Green : rowText.Contains("disabled") ? RowEvaluation.Red : RowEvaluation.None;

            if (normalizedScript.Contains("loginauditing"))
                return rowText.Contains("failed") && rowText.Contains("login") ? RowEvaluation.Green : RowEvaluation.Red;

            if (normalizedScript.Contains("issadisabled")
                || normalizedScript.Contains("scanforstartupprocs")
                || normalizedScript.Contains("crossdbownershipchaining")
                || normalizedScript.Contains("trustworthyofdatabase")
                || normalizedScript.Contains("adhocdistributedqueries")
                || normalizedScript.Contains("clrstrictsecurity")
                || normalizedScript.Contains("databasemailxps")
                || normalizedScript.Contains("oleautomationprocedures")
                || normalizedScript.Contains("remoteacces")
                || normalizedScript.Contains("remoteadminconnections"))
                return rowText.Contains("disabled") ? RowEvaluation.Green : rowText.Contains("enabled") ? RowEvaluation.Red : RowEvaluation.None;

            if (normalizedScript.Contains("passwordpolicyforsqllogins"))
            {
                if (rowText.Contains("not checked") || rowText.Contains("n/a") || rowText.Contains("0"))
                    return RowEvaluation.Red;

                if (rowText.Contains("checked") || rowText.Contains("1") || rowText.Contains("true"))
                    return RowEvaluation.Green;

                return RowEvaluation.None;
            }

            if (normalizedScript.Contains("hideinstance"))
            {
                if (HasExactValue(row, "1")) return RowEvaluation.Green;
                if (HasExactValue(row, "0")) return RowEvaluation.Red;
                return RowEvaluation.None;
            }

            if (normalizedScript.Contains("ifconnectionusekerberos"))
                return rowText.Contains("kerberos") ? RowEvaluation.Green : rowText.Contains("ntlm") ? RowEvaluation.Red : RowEvaluation.None;

            if (normalizedScript.Contains("isagenabled")
                || normalizedScript.Contains("isclustered")
                || normalizedScript.Contains("islogshipped")
                || normalizedScript.Contains("ismirrored")
                || normalizedScript.Contains("isreplicated"))
            {
                if (HasExactValue(row, "1")) return RowEvaluation.Green;
                if (HasExactValue(row, "0"))
                {
                    var hasAnyEnabled = table.ExtendedProperties["HaDrAnyEnabled"] as bool? == true;
                    return hasAnyEnabled ? RowEvaluation.Yellow : RowEvaluation.Red;
                }

                return RowEvaluation.None;
            }

            if (normalizedScript.Contains("lastbackupdates"))
            {
                if (TryFindDate(row, out var dt))
                    return dt >= DateTime.Now.AddMonths(-1) ? RowEvaluation.Green : RowEvaluation.Red;
                return RowEvaluation.Red;
            }

            if (normalizedScript.Contains("lastknowgoodcheckdb"))
            {
                if (TryFindDate(row, out var dt))
                {
                    if (dt.Year == 1900) return RowEvaluation.Red;
                    if (dt >= DateTime.Now.AddMonths(-1)) return RowEvaluation.Green;
                }

                return RowEvaluation.None;
            }

            return RowEvaluation.None;
        }

        public static string? ToColorHex(RowEvaluation evaluation)
        {
            return evaluation switch
            {
                RowEvaluation.Green => "#D4EFDF",
                RowEvaluation.Red => "#FADBD8",
                RowEvaluation.Yellow => "#FCF3CF",
                _ => null
            };
        }

        private static bool HasExactValue(DataRow row, string expected)
        {
            return row.ItemArray.Select(ToText).Any(v => string.Equals(v, expected, StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryFindDate(DataRow row, out DateTime date)
        {
            foreach (var value in row.ItemArray)
            {
                if (value is DateTime dt)
                {
                    date = dt;
                    return true;
                }

                var text = ToText(value);
                if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt)
                    || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    date = dt;
                    return true;
                }
            }

            date = default;
            return false;
        }

        private static string ToText(object? value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        }

        private static string NormalizeToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
