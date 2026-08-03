using Microsoft.Data.SqlClient;
using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using SqlSecAuditor.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SqlSecAuditor.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<SqlInstance> Instances { get; set; }

        public ICommand ConnectNewDatabaseCommand { get; }

        public MainViewModel()
        {
            Instances = new ObservableCollection<SqlInstance>();

            ConnectNewDatabaseCommand = new RelayCommand(ExecuteConnectNewDatabase);
        }

        private void ExecuteConnectNewDatabase(object obj)
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
                        results.Add(perDbResult);
                    }
                    else
                    {
                        var result = new ScriptExecutionResult { ScriptName = scriptName };
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


        private static string FormatReaderRow(SqlDataReader reader)
        {
            var values = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? "N/A" : reader.GetValue(i)?.ToString() ?? "N/A";
                values[i] = $"{columnName}: {value}";
            }

            return string.Join(" | ", values);
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

                var columnName = reader.GetName(i);
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Column{ i + 1 }";
                }
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
