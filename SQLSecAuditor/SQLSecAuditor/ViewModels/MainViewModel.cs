using Microsoft.Data.SqlClient;
using SqlSecAuditor.Models;
using SqlSecAuditor.Views;
using System.Collections.ObjectModel;
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
                        ScriptName = Path.GetFileName(scriptFile)
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
                            while (await reader.ReadAsync())
                            {
                                hasAnyRow = true;
                                result.Rows.Add(FormatReaderRow(reader));
                            }
                        }
                        while (await reader.NextResultAsync());

                        if (!hasAnyRow)
                        {
                            result.Rows.Add("Brak wierszy wynikowych.");
                        }
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
                        ScriptName = Path.GetFileName(scriptFile)
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
                            while (await reader.ReadAsync())
                            {
                                hasAnyRow = true;
                                result.Rows.Add(FormatReaderRow(reader));
                            }
                        }
                        while (await reader.NextResultAsync());

                        if (!hasAnyRow)
                        {
                            result.Rows.Add("Brak wierszy wynikowych.");
                        }
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
                        ScriptName = Path.GetFileName(scriptFile)
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
                            while (await reader.ReadAsync())
                            {
                                hasAnyRow = true;
                                result.Rows.Add(FormatReaderRow(reader));
                            }
                        }
                        while (await reader.NextResultAsync());

                        if (!hasAnyRow)
                        {
                            result.Rows.Add("Brak wierszy wynikowych.");
                        }
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
