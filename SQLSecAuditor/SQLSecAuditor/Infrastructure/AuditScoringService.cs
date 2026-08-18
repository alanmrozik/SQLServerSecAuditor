using SqlSecAuditor.Models;
using System.Data;

namespace SqlSecAuditor.Infrastructure
{
    /// <summary>Applies audit-specific context and calculates the security score for an instance.</summary>
    public static class AuditScoringService
    {
        public static void ApplyHighAvailabilityContext(IEnumerable<ScriptExecutionResult> results)
        {
            var tables = results.SelectMany(result => result.Tables).ToList();
            var hasAnyEnabled = tables.Any(TableHasExactOneValue);

            foreach (var table in tables)
            {
                table.ExtendedProperties["HaDrAnyEnabled"] = hasAnyEnabled;
            }
        }

        public static void Recalculate(SqlInstance instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            var green = 0;
            var yellow = 0;
            var red = 0;
            foreach (var result in EnumerateScoredResults(instance))
            {
                foreach (DataTable table in result.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        switch (RowEvaluationService.Evaluate(result.ScriptName, table, row))
                        {
                            case RowEvaluation.Green: green++; break;
                            case RowEvaluation.Yellow: yellow++; break;
                            case RowEvaluation.Red: red++; break;
                        }
                    }
                }
            }

            var rawPoints = green - red;
            instance.ScoringGreenCount = green;
            instance.ScoringYellowCount = yellow;
            instance.ScoringRedCount = red;
            instance.ScoringRawPoints = rawPoints;
            instance.ScoringPoints = Math.Max(0, rawPoints);
            instance.ScoringMaxPoints = Math.Max(1, green);
            instance.ScoringMinPoints = -red;
        }

        private static IEnumerable<ScriptExecutionResult> EnumerateScoredResults(SqlInstance instance)
        {
            foreach (var result in instance.MaintenanceIntegrityResults) yield return result;
            foreach (var result in instance.NetworkConnectivityResults) yield return result;
            foreach (var result in instance.SurfaceAreaReductionResults) yield return result;
            foreach (var result in instance.AuditingMonitoringResults) yield return result;
            foreach (var result in instance.AuthenticationAccessControlResults) yield return result;
            foreach (var result in instance.AuthorizationPermissionsResults) yield return result;
            foreach (var result in instance.DatabaseSecurityResults) yield return result;
            foreach (var result in instance.HighAvailabilityDisasterRecoveryResults) yield return result;
        }

        private static bool TableHasExactOneValue(DataTable table) => table.Rows.Cast<DataRow>()
            .SelectMany(row => row.ItemArray)
            .Any(value => value is not null
                && value != DBNull.Value
                && string.Equals(value.ToString()?.Trim(), "1", StringComparison.OrdinalIgnoreCase));
    }
}
