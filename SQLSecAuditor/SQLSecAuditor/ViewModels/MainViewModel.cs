using Microsoft.Data.SqlClient;
using SqlSecAuditor.Models;
using SqlSecAuditor.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Threading.Tasks;

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
                var scriptPath = ResolveScriptFilePath("General", "GeneralInfoAboutServer.sql");
                var script = await File.ReadAllTextAsync(scriptPath);
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
            if (instance.IsMaintenanceIntegrityRunning)
            {
                return;
            }

            instance.IsMaintenanceIntegrityRunning = true;
            instance.MaintenanceIntegrityError = null;
            instance.MaintenanceIntegrityResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("Maintenance&Integrity");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.MaintenanceIntegrityError = "Brak skryptów SQL w kategorii Utrzymanie i integralność.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            // Load current result set into DataTable using async reader
                            var table = await ReadDataTableAsync(reader);

                            // Always add the table (even if it has zero rows) so UI can present an empty table
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.MaintenanceIntegrityResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.MaintenanceIntegrityError = $"Nie udało się uruchomić kategorii Utrzymanie i integralność: {ex.Message}";
            }
            finally
            {
                instance.IsMaintenanceIntegrityRunning = false;
            }
        }

        public async Task RunNetworkConnectivityAsync(SqlInstance instance)
        {
            if (instance.IsNetworkConnectivityRunning)
            {
                return;
            }

            instance.IsNetworkConnectivityRunning = true;
            instance.NetworkConnectivityError = null;
            instance.NetworkConnectivityResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("Network&Connectivity");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.NetworkConnectivityError = "Brak skryptów SQL w kategorii Sieć i łączność.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.NetworkConnectivityResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.NetworkConnectivityError = $"Nie udało się uruchomić kategorii Sieć i łączność: {ex.Message}";
            }
            finally
            {
                instance.IsNetworkConnectivityRunning = false;
            }
        }

        public async Task RunSurfaceAreaReductionAsync(SqlInstance instance)
        {
            if (instance.IsSurfaceAreaReductionRunning)
            {
                return;
            }

            instance.IsSurfaceAreaReductionRunning = true;
            instance.SurfaceAreaReductionError = null;
            instance.SurfaceAreaReductionResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("SurfaceAreaReduction");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.SurfaceAreaReductionError = "Brak skryptów SQL w kategorii Redukcja powierzchni ataku.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.SurfaceAreaReductionResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.SurfaceAreaReductionError = $"Nie udało się uruchomić kategorii Redukcja powierzchni ataku: {ex.Message}";
            }
            finally
            {
                instance.IsSurfaceAreaReductionRunning = false;
            }
        }

        public async Task RunAuditingMonitoringAsync(SqlInstance instance)
        {
            if (instance.IsAuditingMonitoringRunning)
            {
                return;
            }

            instance.IsAuditingMonitoringRunning = true;
            instance.AuditingMonitoringError = null;
            instance.AuditingMonitoringResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("Auditing&Monitoring");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.AuditingMonitoringError = "Brak skryptów SQL w kategorii Audyt i monitoring.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.AuditingMonitoringResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.AuditingMonitoringError = $"Nie udało się uruchomić kategorii Audyt i monitoring: {ex.Message}";
            }
            finally
            {
                instance.IsAuditingMonitoringRunning = false;
            }
        }

        public async Task RunAuthenticationAccessControlAsync(SqlInstance instance)
        {
            if (instance.IsAuthenticationAccessControlRunning)
            {
                return;
            }

            instance.IsAuthenticationAccessControlRunning = true;
            instance.AuthenticationAccessControlError = null;
            instance.AuthenticationAccessControlResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("Authentication&AccessControl");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.AuthenticationAccessControlError = "Brak skryptów SQL w kategorii Uwierzytelnianie i kontrola dostępu.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.AuthenticationAccessControlResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.AuthenticationAccessControlError = $"Nie udało się uruchomić kategorii Uwierzytelnianie i kontrola dostępu: {ex.Message}";
            }
            finally
            {
                instance.IsAuthenticationAccessControlRunning = false;
            }
        }

        public async Task RunAuthorizationPermissionsAsync(SqlInstance instance)
        {
            if (instance.IsAuthorizationPermissionsRunning)
            {
                return;
            }

            instance.IsAuthorizationPermissionsRunning = true;
            instance.AuthorizationPermissionsError = null;
            instance.AuthorizationPermissionsResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("Authorization&Permissions");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.AuthorizationPermissionsError = "Brak skryptów SQL w kategorii Autoryzacja i uprawnienia.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.AuthorizationPermissionsResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.AuthorizationPermissionsError = $"Nie udało się uruchomić kategorii Autoryzacja i uprawnienia: {ex.Message}";
            }
            finally
            {
                instance.IsAuthorizationPermissionsRunning = false;
            }
        }

        public async Task RunDatabaseSecurityAsync(SqlInstance instance)
        {
            if (instance.IsDatabaseSecurityRunning)
            {
                return;
            }

            instance.IsDatabaseSecurityRunning = true;
            instance.DatabaseSecurityError = null;
            instance.DatabaseSecurityResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("DatabaseSecurity");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.DatabaseSecurityError = "Brak skryptów SQL w kategorii Bezpieczeństwo baz danych.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.DatabaseSecurityResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.DatabaseSecurityError = $"Nie udało się uruchomić kategorii Bezpieczeństwo baz danych: {ex.Message}";
            }
            finally
            {
                instance.IsDatabaseSecurityRunning = false;
            }
        }

        public async Task RunHighAvailabilityDisasterRecoveryAsync(SqlInstance instance)
        {
            if (instance.IsHighAvailabilityDisasterRecoveryRunning)
            {
                return;
            }

            instance.IsHighAvailabilityDisasterRecoveryRunning = true;
            instance.HighAvailabilityDisasterRecoveryError = null;
            instance.HighAvailabilityDisasterRecoveryResults.Clear();

            try
            {
                var scriptsDirectory = ResolveScriptsDirectoryPath("HighAvailability&DisasterRecovery");
                var scriptFiles = Directory.GetFiles(scriptsDirectory, "*.sql")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (scriptFiles.Length == 0)
                {
                    instance.HighAvailabilityDisasterRecoveryError = "Brak skryptów SQL w kategorii Wysoka dostępność i odzyskiwanie po awarii.";
                    return;
                }

                await using var connection = new SqlConnection(instance.ConnectionString);
                await connection.OpenAsync();

                foreach (var scriptFile in scriptFiles)
                {
                    var result = new ScriptExecutionResult
                    {
                        ScriptName = !string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(scriptFile))
                            ? Path.GetFileNameWithoutExtension(scriptFile)
                            : (Path.GetFileName(scriptFile) ?? scriptFile)
                    };

                    try
                    {
                        var script = await File.ReadAllTextAsync(scriptFile);
                        var commandText = RemoveBatchSeparators(script);

                        await using var command = connection.CreateCommand();
                        command.CommandText = commandText;

                        await using var reader = await command.ExecuteReaderAsync();
                        var hasAnyRow = false;

                        do
                        {
                            var table = await ReadDataTableAsync(reader);
                            if (table != null)
                            {
                                if (table.Rows.Count > 0)
                                {
                                    hasAnyRow = true;
                                }

                                result.Tables.Add(table);
                            }
                        }
                        while (await reader.NextResultAsync());
                    }
                    catch (Exception ex)
                    {
                        result.Error = ex.Message;
                    }

                    instance.HighAvailabilityDisasterRecoveryResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                instance.HighAvailabilityDisasterRecoveryError = $"Nie udało się uruchomić kategorii Wysoka dostępność i odzyskiwanie po awarii: {ex.Message}";
            }
            finally
            {
                instance.IsHighAvailabilityDisasterRecoveryRunning = false;
            }
        }


        private static string ResolveScriptFilePath(string category, string scriptFileName)
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, "t-sql-scripts", category, scriptFileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new FileNotFoundException($"Nie znaleziono pliku skryptu: t-sql-scripts/{category}/{scriptFileName}");
        }

        private static string ResolveScriptsDirectoryPath(string category)
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current is not null)
            {
                var candidate = Path.Combine(current.FullName, "t-sql-scripts", category);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException($"Nie znaleziono katalogu skryptów: t-sql-scripts/{category}");
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
    }
}
