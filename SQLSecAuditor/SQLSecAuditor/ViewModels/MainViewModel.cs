using Microsoft.Data.SqlClient;
using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using SqlSecAuditor.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SqlSecAuditor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private SqlInstance? _selectedInstance;
        private string? _snapshotComparisonSummary;
        private string? _snapshotViewerSummary;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SqlInstance> Instances { get; set; }
        public ObservableCollection<CustomQuery> CustomQueries { get; } = new();

        public ObservableCollection<SnapshotComparisonRow> SnapshotComparisonRows { get; } = new();
        public ObservableCollection<SnapshotViewerCategory> SnapshotViewerCategories { get; } = new();

        public SqlInstance? SelectedInstance
        {
            get => _selectedInstance;
            set => SetProperty(ref _selectedInstance, value);
        }

        public string? SnapshotComparisonSummary
        {
            get => _snapshotComparisonSummary;
            set => SetProperty(ref _snapshotComparisonSummary, value);
        }

        public string? SnapshotViewerSummary
        {
            get => _snapshotViewerSummary;
            set => SetProperty(ref _snapshotViewerSummary, value);
        }

        public ICommand ConnectNewDatabaseCommand { get; }

        public MainViewModel()
        {
            Instances = new ObservableCollection<SqlInstance>();
            foreach (var query in CustomQueriesStore.Load()) CustomQueries.Add(query);

            ConnectNewDatabaseCommand = new RelayCommand(ExecuteConnectNewDatabase);
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        // Extract a fix script from a SQL file: a block comment starting with 'Fix:' (/*Fix: ... */)
        private static string? ExtractFixScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script)) return null;

            var lower = script.ToLowerInvariant();
            var token = "fix:";
            var idx = lower.IndexOf(token, StringComparison.Ordinal);
            if (idx >= 0)
            {
                // find enclosing block comment
                var start = lower.LastIndexOf("/*", idx, StringComparison.Ordinal);
                var end = lower.IndexOf("*/", idx, StringComparison.Ordinal);
                if (start >= 0 && end > start)
                {
                    var block = script.Substring(start + 2, end - (start + 2));
                    var rel = block.ToLowerInvariant().IndexOf(token, StringComparison.Ordinal);
                    if (rel >= 0)
                    {
                        var after = block.Substring(rel + token.Length).Trim();
                        // Return as-is (preserve SQL formatting)
                        return after.Trim();
                    }
                }
            }

            return null;
        }

        private void ExecuteConnectNewDatabase(object? obj)
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

        public async Task LoadGeneralInfoAsync(SqlInstance instance)
        {
            if (instance.IsGeneralInfoLoaded || instance.IsGeneralInfoLoading)
            {
                return;
            }

            instance.IsGeneralInfoLoading = true;
            instance.GeneralInfoError = null;

            try
            {
                var script = await SqlScriptLoader.LoadScriptAsync("General", "GeneralInfoAboutServer.sql");
                var commandText = RemoveBatchSeparators(script);

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = commandText;

                await using var reader = await command.ExecuteReaderAsync();

                instance.GeneralInfoEntries.Clear();

                if (await reader.ReadAsync())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? "N/A" : reader.GetValue(i)?.ToString() ?? "N/A";
                        instance.GeneralInfoEntries.Add(new GeneralInfoEntry
                        {
                            Label = reader.GetName(i),
                            Value = value
                        });
                    }
                }

                if (instance.GeneralInfoEntries.Count == 0)
                {
                    instance.GeneralInfoError = "Brak danych dla kategorii Informacje Ogólne.";
                }

                instance.IsGeneralInfoLoaded = true;
            }
            catch (Exception ex)
            {
                instance.GeneralInfoError = $"Nie udało się pobrać informacji ogólnych: {ex.Message}";
            }
            finally
            {
                instance.IsGeneralInfoLoading = false;
            }
        }

        public async Task RunMaintenanceIntegrityAsync(SqlInstance instance)
        {
            if (instance.IsMaintenanceIntegrityRunning) return;
            await RunCategoryAsync(instance, "Maintenance&Integrity",
                instance.MaintenanceIntegrityResults,
                r => instance.IsMaintenanceIntegrityRunning = r,
                e => instance.MaintenanceIntegrityError = e);
        }

        public async Task RunNetworkConnectivityAsync(SqlInstance instance)
        {
            if (instance.IsNetworkConnectivityRunning) return;
            await RunCategoryAsync(instance, "Network&Connectivity",
                instance.NetworkConnectivityResults,
                r => instance.IsNetworkConnectivityRunning = r,
                e => instance.NetworkConnectivityError = e);
        }

        public async Task RunSurfaceAreaReductionAsync(SqlInstance instance)
        {
            if (instance.IsSurfaceAreaReductionRunning) return;
            await RunCategoryAsync(instance, "SurfaceAreaReduction",
                instance.SurfaceAreaReductionResults,
                r => instance.IsSurfaceAreaReductionRunning = r,
                e => instance.SurfaceAreaReductionError = e);
        }

        // Scripts that must be executed per-database (they query database-level objects)
        private static readonly HashSet<string> PerDatabaseScripts = new(StringComparer.OrdinalIgnoreCase)
        {
            "Check_CLR_Enabled",
            "Check_Orphaned_Users",
            "Check_GUEST_permissions"
        };

        private async Task RunCategoryAsync(
            SqlInstance instance,
            string category,
            ObservableCollection<ScriptExecutionResult> results,
            Action<bool> setRunning,
            Action<string?> setError)
        {
            setRunning(true);
            setError(null);
            results.Clear();

            try
            {
                var scripts = await SqlScriptLoader.LoadCategoryScriptsAsync(category);

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var (fileName, sql) in scripts)
                {
                    var scriptName = Path.GetFileNameWithoutExtension(fileName);
                    if (string.IsNullOrWhiteSpace(scriptName))
                        scriptName = fileName;

                    if (PerDatabaseScripts.Contains(scriptName))
                    {
                        // Run per-database: collect all database names then execute on each
                        var perDbResult = new ScriptExecutionResult { ScriptName = scriptName };
                        perDbResult.Description = ExtractScriptDescription(sql);
                        try
                        {
                            var databases = await GetDatabaseNamesAsync(instance.ConnectionString);
                            foreach (var dbName in databases)
                            {
                                var csBuilder = new SqlConnectionStringBuilder(instance.ConnectionString)
                                {
                                    InitialCatalog = dbName
                                };
                                await using var dbConnection = new SqlConnection(csBuilder.ConnectionString);
                                try
                                {
                                    await dbConnection.OpenAsync();
                                    await using var cmd = dbConnection.CreateCommand();
                                    cmd.CommandText = RemoveBatchSeparators(sql);
                                    await using var rdr = await cmd.ExecuteReaderAsync();
                                        do
                                        {
                                            var tbl = await ReadDataTableAsync(rdr);
                                            tbl.TableName = dbName;
                                            perDbResult.Tables.Add(tbl);

                                            // Evaluate rows for red status
                                            foreach (DataRow r in tbl.Rows)
                                            {
                                                var ev = RowEvaluationService.Evaluate(perDbResult.ScriptName, tbl, r);
                                                if (ev == RowEvaluation.Red)
                                                {
                                                    perDbResult.HasAnyRed = true;
                                                }
                                            }
                                        }
                                        while (await rdr.NextResultAsync());
                                }
                                catch (Exception dbEx)
                                {
                                    // Add an error table for this database
                                    var errTable = new DataTable { TableName = dbName };
                                    errTable.Columns.Add("Błąd");
                                    errTable.Rows.Add(dbEx.Message);
                                    perDbResult.Tables.Add(errTable);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            perDbResult.Error = ex.Message;
                        }
                        // also extract fix script if present
                        perDbResult.FixScript = ExtractFixScript(sql);
                        results.Add(perDbResult);
                    }
                    else
                    {
                        var result = new ScriptExecutionResult { ScriptName = scriptName };
                        result.Description = ExtractScriptDescription(sql);
                        result.FixScript = ExtractFixScript(sql);
                        try
                        {
                            var commandText = RemoveBatchSeparators(sql);
                            await using var command = connection.CreateCommand();
                            command.CommandText = commandText;
                            await using var reader = await command.ExecuteReaderAsync();
                            do
                            {
                                var table = await ReadDataTableAsync(reader);
                                result.Tables.Add(table);

                                // Evaluate rows for red status
                                foreach (DataRow r in table.Rows)
                                {
                                    var ev = RowEvaluationService.Evaluate(result.ScriptName, table, r);
                                    if (ev == RowEvaluation.Red)
                                    {
                                        result.HasAnyRed = true;
                                    }
                                }
                            }
                            while (await reader.NextResultAsync());
                        }
                        catch (Exception ex)
                        {
                            result.Error = ex.Message;
                        }
                        results.Add(result);
                    }
                }

                if (string.Equals(category, "HighAvailability&DisasterRecovery", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyHighAvailabilityContext(results);
                }

                RecalculateScoring(instance);
            }
            catch (Exception ex)
            {
                setError(ex.Message);
            }
            finally
            {
                setRunning(false);
            }
        }


        public async Task RunAuditingMonitoringAsync(SqlInstance instance)
        {
            if (instance.IsAuditingMonitoringRunning) return;
            await RunCategoryAsync(instance, "Auditing&Monitoring",
                instance.AuditingMonitoringResults,
                r => instance.IsAuditingMonitoringRunning = r,
                e => instance.AuditingMonitoringError = e);
        }

        public async Task RunAuthenticationAccessControlAsync(SqlInstance instance)
        {
            if (instance.IsAuthenticationAccessControlRunning) return;
            await RunCategoryAsync(instance, "Authentication&AccessControl",
                instance.AuthenticationAccessControlResults,
                r => instance.IsAuthenticationAccessControlRunning = r,
                e => instance.AuthenticationAccessControlError = e);
        }

        public async Task RunAuthorizationPermissionsAsync(SqlInstance instance)
        {
            if (instance.IsAuthorizationPermissionsRunning) return;
            await RunCategoryAsync(instance, "Authorization&Permissions",
                instance.AuthorizationPermissionsResults,
                r => instance.IsAuthorizationPermissionsRunning = r,
                e => instance.AuthorizationPermissionsError = e);
        }

        public async Task RunDatabaseSecurityAsync(SqlInstance instance)
        {
            if (instance.IsDatabaseSecurityRunning) return;
            await RunCategoryAsync(instance, "DatabaseSecurity",
                instance.DatabaseSecurityResults,
                r => instance.IsDatabaseSecurityRunning = r,
                e => instance.DatabaseSecurityError = e);
        }

        public async Task RunHighAvailabilityDisasterRecoveryAsync(SqlInstance instance)
        {
            if (instance.IsHighAvailabilityDisasterRecoveryRunning) return;
            await RunCategoryAsync(instance, "HighAvailability&DisasterRecovery",
                instance.HighAvailabilityDisasterRecoveryResults,
                r => instance.IsHighAvailabilityDisasterRecoveryRunning = r,
                e => instance.HighAvailabilityDisasterRecoveryError = e);
        }

        public void AddCustomQuery(CustomQuery query)
        {
            CustomQueries.Add(query);
            CustomQueriesStore.Save(CustomQueries);
        }

        public void DeleteCustomQuery(CustomQuery query)
        {
            CustomQueries.Remove(query);
            foreach (var instance in Instances)
            {
                foreach (var result in instance.CustomQueryResults.Where(r => r.CustomQueryId == query.Id).ToList())
                    instance.CustomQueryResults.Remove(result);
            }
            CustomQueriesStore.Save(CustomQueries);
        }

        public async Task RunCustomQueriesAsync(SqlInstance instance)
        {
            if (instance.IsCustomQueriesRunning) return;
            instance.CustomQueryResults.Clear();
            await RunCustomQueriesCoreAsync(instance, CustomQueries);
        }

        public async Task RunCustomQueryAsync(SqlInstance instance, CustomQuery query)
        {
            if (instance.IsCustomQueriesRunning) return;
            foreach (var result in instance.CustomQueryResults.Where(r => r.CustomQueryId == query.Id).ToList())
                instance.CustomQueryResults.Remove(result);
            await RunCustomQueriesCoreAsync(instance, new[] { query });
        }

        private async Task RunCustomQueriesCoreAsync(SqlInstance instance, IEnumerable<CustomQuery> queries)
        {
            instance.IsCustomQueriesRunning = true;
            instance.CustomQueriesError = null;
            try
            {
                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();
                foreach (var query in queries)
                {
                    var result = new ScriptExecutionResult { ScriptName = query.Name, CustomQueryId = query.Id };
                    try
                    {
                        await using var command = connection.CreateCommand();
                        command.CommandText = RemoveBatchSeparators(query.Sql);
                        await using var reader = await command.ExecuteReaderAsync();
                        do { result.Tables.Add(await ReadDataTableAsync(reader)); }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex) { result.Error = ex.Message; }
                    instance.CustomQueryResults.Add(result);
                }
            }
            catch (Exception ex) { instance.CustomQueriesError = ex.Message; }
            finally { instance.IsCustomQueriesRunning = false; }
        }


        private static void ApplyHighAvailabilityContext(IEnumerable<ScriptExecutionResult> results)
        {
            var allTables = results.SelectMany(r => r.Tables).ToList();
            var hasAnyEnabled = allTables.Any(TableHasExactOneValue);

            foreach (var table in allTables)
            {
                table.ExtendedProperties["HaDrAnyEnabled"] = hasAnyEnabled;
            }
        }

        private static bool TableHasExactOneValue(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    if (cell != null
                        && cell != DBNull.Value
                        && string.Equals(cell.ToString()?.Trim(), "1", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void RecalculateScoring(SqlInstance instance)
        {
            var allResults = EnumerateAllScoredResults(instance).ToList();

            var green = 0;
            var yellow = 0;
            var red = 0;

            foreach (var result in allResults)
            {
                foreach (DataTable table in result.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var eval = RowEvaluationService.Evaluate(result.ScriptName, table, row);
                        switch (eval)
                        {
                            case RowEvaluation.Green:
                                green++;
                                break;
                            case RowEvaluation.Yellow:
                                yellow++;
                                break;
                            case RowEvaluation.Red:
                                red++;
                                break;
                        }
                    }
                }
            }

            // Yellow evaluations do not affect scoring (per new rule)
            var rawPoints = green - red;
            var points = rawPoints < 0 ? 0 : rawPoints; // display value (non-negative)
            var maxPoints = green; // only greens count toward positive max
            var minPoints = -red;  // only reds count toward negative min

            instance.ScoringGreenCount = green;
            instance.ScoringYellowCount = yellow;
            instance.ScoringRedCount = red;
            instance.ScoringRawPoints = rawPoints;
            instance.ScoringPoints = points;
            instance.ScoringMaxPoints = maxPoints <= 0 ? 1 : maxPoints;
            instance.ScoringMinPoints = minPoints;
        }

        private static IEnumerable<ScriptExecutionResult> EnumerateAllScoredResults(SqlInstance instance)
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

        private static async Task<DataTable> ReadDataTableAsync(SqlDataReader reader)
        {
            var table = new DataTable();

            // Define columns
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnType = typeof(object);
                try
                {
                    columnType = reader.GetFieldType(i) ?? typeof(object);
                }
                catch
                {
                    // ignore and use object
                }

                var columnName = GetUniqueColumnName(table, reader.GetName(i), i);
                table.Columns.Add(columnName, columnType);
            }

            // Read rows asynchronously
            while (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }

                table.Rows.Add(values);
            }

            return table;
        }

        private static string GetUniqueColumnName(DataTable table, string? proposedName, int ordinal)
        {
            var baseName = string.IsNullOrWhiteSpace(proposedName) ? $"Column{ordinal + 1}" : proposedName;
            var name = baseName;
            var suffix = 2;

            while (table.Columns.Contains(name))
            {
                name = $"{baseName}_{suffix++}";
            }

            return name;
        }

        private static string RemoveBatchSeparators(string script)
        {
            var builder = new StringBuilder();
            using var reader = new StringReader(script);
            string? line;

            while ((line = reader.ReadLine()) is not null)
            {
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        // Extract a short description from the top of a SQL script.
        // Supports a leading block comment (/* ... */) or consecutive leading line comments (-- ...)
        private static string? ExtractScriptDescription(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return null;
            // Prefer a block comment that contains the token "Description:" (case-insensitive).
            var lower = script.ToLowerInvariant();
            var descToken = "description:";
            var descIdx = lower.IndexOf(descToken, StringComparison.Ordinal);
            if (descIdx >= 0)
            {
                // Find the start of the containing block comment
                var blockStart = lower.LastIndexOf("/*", descIdx, StringComparison.Ordinal);
                var blockEnd = lower.IndexOf("*/", descIdx + descToken.Length, StringComparison.Ordinal);
                if (blockStart >= 0 && blockEnd > blockStart)
                {
                    var block = script.Substring(blockStart + 2, blockEnd - (blockStart + 2));
                    // Extract text after the Description: token
                    var relIdx = block.ToLowerInvariant().IndexOf(descToken, StringComparison.Ordinal);
                    if (relIdx >= 0)
                    {
                        var after = block.Substring(relIdx + descToken.Length).Trim();
                        // stop at 'Rationale:' if present
                        var rationaleIdx = after.IndexOf("Rationale:", StringComparison.OrdinalIgnoreCase);
                        if (rationaleIdx >= 0)
                            after = after.Substring(0, rationaleIdx).Trim();
                        return NormalizeWhitespace(after);
                    }
                }
            }

            // Fallback: keep original behavior (leading block or leading -- lines)
            using var reader = new StringReader(script);
            string? line;

            // Skip leading blank lines
            while ((line = reader.ReadLine()) is not null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    break;
            }

            if (line is null)
                return null;

            var sb = new StringBuilder();
            var trimmed = line.TrimStart();

            // Block comment /* ... */
            if (trimmed.StartsWith("/*"))
            {
                // Inline end on same line
                var startIdx = trimmed.IndexOf("/*");
                var endIdx = trimmed.IndexOf("*/", startIdx + 2);
                if (endIdx >= 0)
                {
                    return NormalizeWhitespace(trimmed.Substring(startIdx + 2, endIdx - (startIdx + 2)).Trim());
                }

                // Multi-line block comment
                sb.AppendLine(trimmed.Substring(startIdx + 2));
                while ((line = reader.ReadLine()) is not null)
                {
                    var idx = line.IndexOf("*/");
                    if (idx >= 0)
                    {
                        sb.Append(line.Substring(0, idx));
                        break;
                    }
                    sb.AppendLine(line);
                }

                return NormalizeWhitespace(sb.ToString().Trim());
            }

            // Consecutive -- comment lines
            if (trimmed.StartsWith("--"))
            {
                sb.AppendLine(trimmed.Length > 2 ? trimmed.Substring(2).TrimStart() : string.Empty);
                while ((line = reader.ReadLine()) is not null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        break;
                    var t = line.TrimStart();
                    if (t.StartsWith("--"))
                        sb.AppendLine(t.Length > 2 ? t.Substring(2).TrimStart() : string.Empty);
                    else
                        break;
                }

                return NormalizeWhitespace(sb.ToString().Trim());
            }

            return null;
        }

        private static string NormalizeWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var parts = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
            return string.Join(" ", parts).Trim();
        }

        private static async Task<List<string>> GetDatabaseNamesAsync(string connectionString)
        {
            var names = new List<string>();
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sys.databases WHERE state_desc = 'ONLINE' AND name NOT IN ('master','tempdb','model','msdb') ORDER BY name";
            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
                names.Add(rdr.GetString(0));
            return names;
        }
    }
}
