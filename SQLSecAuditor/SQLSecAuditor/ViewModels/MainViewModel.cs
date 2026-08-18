using Microsoft.Data.SqlClient;
using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlSecAuditor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private SqlInstance? _selectedInstance;
        private string? _snapshotComparisonSummary;
        private string? _snapshotViewerSummary;
        private readonly IConnectionDialogService _connectionDialogService;

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

        public MainViewModel(IConnectionDialogService connectionDialogService)
        {
            _connectionDialogService = connectionDialogService ?? throw new ArgumentNullException(nameof(connectionDialogService));
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

        private void ExecuteConnectNewDatabase(object? obj)
        {
            var instance = _connectionDialogService.ShowConnectionDialog();
            if (instance is not null)
            {
                Instances.Add(instance);
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
                var commandText = SqlScriptText.RemoveBatchSeparators(script);

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
                        perDbResult.Description = SqlScriptText.ExtractDescription(sql);
                        try
                        {
                            var databases = await SqlDatabaseCatalog.GetOnlineUserDatabasesAsync(instance.ConnectionString);
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
                                    cmd.CommandText = SqlScriptText.RemoveBatchSeparators(sql);
                                    await using var rdr = await cmd.ExecuteReaderAsync();
                                        do
                                        {
                                            var tbl = await SqlDataReaderTableReader.ReadAsync(rdr);
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
                        perDbResult.FixScript = SqlScriptText.ExtractFixScript(sql);
                        results.Add(perDbResult);
                    }
                    else
                    {
                        var result = new ScriptExecutionResult { ScriptName = scriptName };
                        result.Description = SqlScriptText.ExtractDescription(sql);
                        result.FixScript = SqlScriptText.ExtractFixScript(sql);
                        try
                        {
                            var commandText = SqlScriptText.RemoveBatchSeparators(sql);
                            await using var command = connection.CreateCommand();
                            command.CommandText = commandText;
                            await using var reader = await command.ExecuteReaderAsync();
                            do
                            {
                                var table = await SqlDataReaderTableReader.ReadAsync(reader);
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
                    AuditScoringService.ApplyHighAvailabilityContext(results);
                }

                AuditScoringService.Recalculate(instance);
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
                        command.CommandText = SqlScriptText.RemoveBatchSeparators(query.Sql);
                        await using var reader = await command.ExecuteReaderAsync();
                        do { result.Tables.Add(await SqlDataReaderTableReader.ReadAsync(reader)); }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex) { result.Error = ex.Message; }
                    instance.CustomQueryResults.Add(result);
                }
            }
            catch (Exception ex) { instance.CustomQueriesError = ex.Message; }
            finally { instance.IsCustomQueriesRunning = false; }
        }
    }
}
