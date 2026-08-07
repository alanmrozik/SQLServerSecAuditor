using SqlSecAuditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SqlSecAuditor.Infrastructure
{
    public static class ReportSnapshotService
    {
        public static void SaveSnapshot(string filePath, SqlInstance instance)
        {
            var snapshot = BuildSnapshot(instance);
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static ReportSnapshot LoadSnapshot(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var snapshot = JsonSerializer.Deserialize<ReportSnapshot>(json);
            if (snapshot == null)
                throw new InvalidDataException("Invalid snapshot file.");
            return snapshot;
        }

        public static ReportSnapshot BuildSnapshot(SqlInstance instance)
        {
            var snapshot = new ReportSnapshot
            {
                CreatedAtUtc = DateTime.UtcNow,
                ServerName = instance.ServerName,
                DatabaseName = instance.DatabaseName
            };

            if (instance.IsGeneralInfoLoaded)
            {
                var general = new SnapshotCategory { Name = "Informacje Ogólne" };
                var script = new SnapshotScript { Name = "GeneralInfoAboutServer" };
                var table = new SnapshotTable { Name = "General" };
                table.Columns.Add("Label");
                table.Columns.Add("Value");
                foreach (var entry in instance.GeneralInfoEntries)
                    table.Rows.Add(new List<string> { entry.Label ?? string.Empty, entry.Value ?? string.Empty });
                script.Tables.Add(table);
                general.Scripts.Add(script);
                snapshot.Categories.Add(general);
            }

            AddCategory(snapshot, "Utrzymanie i integralność", instance.MaintenanceIntegrityResults, instance.MaintenanceIntegrityError);
            AddCategory(snapshot, "Sieć i łączność", instance.NetworkConnectivityResults, instance.NetworkConnectivityError);
            AddCategory(snapshot, "Redukcja powierzchni ataku", instance.SurfaceAreaReductionResults, instance.SurfaceAreaReductionError);
            AddCategory(snapshot, "Audyt i monitoring", instance.AuditingMonitoringResults, instance.AuditingMonitoringError);
            AddCategory(snapshot, "Uwierzytelnianie i kontrola dostępu", instance.AuthenticationAccessControlResults, instance.AuthenticationAccessControlError);
            AddCategory(snapshot, "Autoryzacja i uprawnienia", instance.AuthorizationPermissionsResults, instance.AuthorizationPermissionsError);
            AddCategory(snapshot, "Bezpieczeństwo baz danych", instance.DatabaseSecurityResults, instance.DatabaseSecurityError);
            AddCategory(snapshot, "Wysoka dostępność i odzyskiwanie po awarii", instance.HighAvailabilityDisasterRecoveryResults, instance.HighAvailabilityDisasterRecoveryError);

            return snapshot;
        }

        public static string Compare(ReportSnapshot current, ReportSnapshot other)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Snapshot Compare");
            sb.AppendLine($"Current: {current.ServerName} [{current.DatabaseName}] @ {current.CreatedAtUtc:u}");
            sb.AppendLine($"Other  : {other.ServerName} [{other.DatabaseName}] @ {other.CreatedAtUtc:u}");
            sb.AppendLine();

            var currentCategories = current.Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var otherCategories = other.Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var allCategoryNames = currentCategories.Keys.Union(otherCategories.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var categoryName in allCategoryNames)
            {
                var hasCurrent = currentCategories.TryGetValue(categoryName, out var currentCategory);
                var hasOther = otherCategories.TryGetValue(categoryName, out var otherCategory);

                if (!hasOther)
                {
                    sb.AppendLine($"[+] Category added: {categoryName}");
                    continue;
                }

                if (!hasCurrent)
                {
                    sb.AppendLine($"[-] Category removed: {categoryName}");
                    continue;
                }

                CompareCategory(sb, currentCategory!, otherCategory!);
            }

            if (sb.ToString().Trim().Split('\n').Length <= 4)
            {
                sb.AppendLine("No differences.");
            }

            return sb.ToString();
        }

        private static void CompareCategory(StringBuilder sb, SnapshotCategory current, SnapshotCategory other)
        {
            var currentScripts = current.Scripts.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var otherScripts = other.Scripts.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var allScriptNames = currentScripts.Keys.Union(otherScripts.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var scriptName in allScriptNames)
            {
                var hasCurrent = currentScripts.TryGetValue(scriptName, out var currentScript);
                var hasOther = otherScripts.TryGetValue(scriptName, out var otherScript);

                if (!hasOther)
                {
                    sb.AppendLine($"[+] Script added: {current.Name} / {scriptName}");
                    continue;
                }

                if (!hasCurrent)
                {
                    sb.AppendLine($"[-] Script removed: {current.Name} / {scriptName}");
                    continue;
                }

                CompareScript(sb, current.Name, currentScript!, otherScript!);
            }
        }

        private static void CompareScript(StringBuilder sb, string categoryName, SnapshotScript current, SnapshotScript other)
        {
            var currentTables = current.Tables.ToDictionary(t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var otherTables = other.Tables.ToDictionary(t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var allTableNames = currentTables.Keys.Union(otherTables.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var tableName in allTableNames)
            {
                var hasCurrent = currentTables.TryGetValue(tableName, out var currentTable);
                var hasOther = otherTables.TryGetValue(tableName, out var otherTable);

                if (!hasOther)
                {
                    sb.AppendLine($"[+] Table added: {categoryName} / {current.Name} / {tableName}");
                    continue;
                }

                if (!hasCurrent)
                {
                    sb.AppendLine($"[-] Table removed: {categoryName} / {current.Name} / {tableName}");
                    continue;
                }

                CompareTable(sb, categoryName, current.Name, tableName, currentTable!, otherTable!);
            }
        }

        private static void CompareTable(StringBuilder sb, string categoryName, string scriptName, string tableName, SnapshotTable current, SnapshotTable other)
        {
            var currentRows = current.Rows.Select(r => string.Join("|", r)).ToList();
            var otherRows = other.Rows.Select(r => string.Join("|", r)).ToList();

            var added = currentRows.Except(otherRows, StringComparer.Ordinal).ToList();
            var removed = otherRows.Except(currentRows, StringComparer.Ordinal).ToList();

            if (added.Count == 0 && removed.Count == 0)
                return;

            sb.AppendLine($"[*] Changed table: {categoryName} / {scriptName} / {tableName}");

            foreach (var row in added.Take(20))
                sb.AppendLine($"    + {row}");
            if (added.Count > 20)
                sb.AppendLine($"    + ... {added.Count - 20} more rows");

            foreach (var row in removed.Take(20))
                sb.AppendLine($"    - {row}");
            if (removed.Count > 20)
                sb.AppendLine($"    - ... {removed.Count - 20} more rows");
        }

        public static List<SnapshotComparisonRow> CompareRows(ReportSnapshot current, ReportSnapshot other)
        {
            var rows = new List<SnapshotComparisonRow>();

            var currentCategories = current.Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var otherCategories = other.Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var categoryNames = currentCategories.Keys.Union(otherCategories.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var categoryName in categoryNames)
            {
                var hasCurrent = currentCategories.TryGetValue(categoryName, out var currentCategory);
                var hasOther = otherCategories.TryGetValue(categoryName, out var otherCategory);

                if (!hasOther)
                {
                    rows.Add(CreateDiffRow($"Category/{categoryName}", "exists", "", "Added"));
                    continue;
                }

                if (!hasCurrent)
                {
                    rows.Add(CreateDiffRow($"Category/{categoryName}", "", "exists", "Removed"));
                    continue;
                }

                CompareScripts(rows, categoryName, currentCategory!, otherCategory!);
            }

            return rows;
        }

        public static string BuildComparisonSummary(IReadOnlyCollection<SnapshotComparisonRow> rows)
        {
            if (rows.Count == 0)
                return "Brak różnic.";

            var added = rows.Count(r => r.ChangeType == "Added");
            var removed = rows.Count(r => r.ChangeType == "Removed");
            var changed = rows.Count(r => r.ChangeType == "Changed");

            return $"Różnice: +{added} / -{removed} / ~{changed} (razem: {rows.Count})";
        }

        private static void CompareScripts(List<SnapshotComparisonRow> rows, string categoryName, SnapshotCategory current, SnapshotCategory other)
        {
            var currentScripts = current.Scripts.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var otherScripts = other.Scripts.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var scriptNames = currentScripts.Keys.Union(otherScripts.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var scriptName in scriptNames)
            {
                var hasCurrent = currentScripts.TryGetValue(scriptName, out var currentScript);
                var hasOther = otherScripts.TryGetValue(scriptName, out var otherScript);

                if (!hasOther)
                {
                    rows.Add(CreateDiffRow($"{categoryName}/{scriptName}", "exists", "", "Added"));
                    continue;
                }

                if (!hasCurrent)
                {
                    rows.Add(CreateDiffRow($"{categoryName}/{scriptName}", "", "exists", "Removed"));
                    continue;
                }

                CompareTables(rows, categoryName, currentScript!, otherScript!);
            }
        }

        private static void CompareTables(List<SnapshotComparisonRow> rows, string categoryName, SnapshotScript current, SnapshotScript other)
        {
            var currentTables = current.Tables.ToDictionary(t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var otherTables = other.Tables.ToDictionary(t => t.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            var tableNames = currentTables.Keys.Union(otherTables.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var tableName in tableNames)
            {
                var hasCurrent = currentTables.TryGetValue(tableName, out var currentTable);
                var hasOther = otherTables.TryGetValue(tableName, out var otherTable);
                var path = $"{categoryName}/{current.Name}/{tableName}";

                if (!hasOther)
                {
                    rows.Add(CreateDiffRow(path, "exists", "", "Added"));
                    continue;
                }

                if (!hasCurrent)
                {
                    rows.Add(CreateDiffRow(path, "", "exists", "Removed"));
                    continue;
                }

                CompareTableRows(rows, path, currentTable!, otherTable!);
            }
        }

        private static void CompareTableRows(List<SnapshotComparisonRow> rows, string path, SnapshotTable current, SnapshotTable other)
        {
            var currentRows = current.Rows.Select(r => string.Join(" | ", r)).ToList();
            var otherRows = other.Rows.Select(r => string.Join(" | ", r)).ToList();

            var added = currentRows.Except(otherRows, StringComparer.Ordinal).ToList();
            var removed = otherRows.Except(currentRows, StringComparer.Ordinal).ToList();

            // Pair one added row with one removed row as a single "Changed" line (side-by-side)
            var pairedCount = Math.Min(added.Count, removed.Count);
            for (var i = 0; i < pairedCount; i++)
            {
                rows.Add(CreateDiffRow(path, added[i], removed[i], "Changed"));
            }

            // Remaining unmatched rows are pure additions/removals
            for (var i = pairedCount; i < added.Count; i++)
            {
                rows.Add(CreateDiffRow(path, added[i], "", "Added"));
            }

            for (var i = pairedCount; i < removed.Count; i++)
            {
                rows.Add(CreateDiffRow(path, "", removed[i], "Removed"));
            }
        }

        private static SnapshotComparisonRow CreateDiffRow(string path, string currentValue, string snapshotValue, string changeType)
        {
            var marker = changeType switch
            {
                "Added" => "→",
                "Removed" => "←",
                "Changed" => "↔",
                _ => "="
            };

            return new SnapshotComparisonRow
            {
                Path = path,
                CurrentValue = currentValue,
                SnapshotValue = snapshotValue,
                Marker = marker,
                ChangeType = changeType
            };
        }

        public static List<SnapshotViewerCategory> BuildViewerCategories(ReportSnapshot snapshot)
        {
            var result = new List<SnapshotViewerCategory>();

            foreach (var category in snapshot.Categories)
            {
                var viewerCategory = new SnapshotViewerCategory
                {
                    Name = category.Name,
                    Error = category.Error
                };

                foreach (var script in category.Scripts)
                {
                    var execution = new ScriptExecutionResult
                    {
                        ScriptName = script.Name,
                        Error = script.Error
                    };

                    foreach (var sourceTable in script.Tables)
                    {
                        var table = new DataTable { TableName = sourceTable.Name ?? string.Empty };

                        foreach (var colName in sourceTable.Columns)
                        {
                            table.Columns.Add(colName);
                        }

                        foreach (var rowValues in sourceTable.Rows)
                        {
                            var values = rowValues.Cast<object>().ToArray();
                            table.Rows.Add(values);
                        }

                        execution.Tables.Add(table);
                    }

                    ApplyHighAvailabilityContextForViewer(execution);
                    viewerCategory.Scripts.Add(execution);
                }

                result.Add(viewerCategory);
            }

            return result;
        }

        public static string BuildViewerSummary(ReportSnapshot snapshot)
        {
            var categories = snapshot.Categories.Count;
            var scripts = snapshot.Categories.Sum(c => c.Scripts.Count);
            var tables = snapshot.Categories.Sum(c => c.Scripts.Sum(s => s.Tables.Count));
            return $"Snapshot: {snapshot.ServerName} [{snapshot.DatabaseName}] | Kategorie: {categories}, Skrypty: {scripts}, Tabele: {tables}";
        }

        private static void ApplyHighAvailabilityContextForViewer(ScriptExecutionResult script)
        {
            var allTables = script.Tables.Cast<DataTable>().ToList();
            var hasAnyEnabled = allTables.Any(table =>
                table.Rows.Cast<DataRow>().Any(row =>
                    row.ItemArray.Any(cell =>
                        cell != null &&
                        cell != DBNull.Value &&
                        string.Equals(cell.ToString()?.Trim(), "1", StringComparison.OrdinalIgnoreCase))));

            foreach (var table in allTables)
            {
                table.ExtendedProperties["HaDrAnyEnabled"] = hasAnyEnabled;
            }
        }

        private static void AddCategory(ReportSnapshot snapshot, string categoryName, IEnumerable<ScriptExecutionResult> results, string? error)
        {
            var resultList = results.ToList();
            if (resultList.Count == 0)
                return;

            var category = new SnapshotCategory
            {
                Name = categoryName,
                Error = error
            };

            foreach (var result in resultList)
            {
                var script = new SnapshotScript
                {
                    Name = result.ScriptName,
                    Error = result.Error
                };

                foreach (DataTable dt in result.Tables)
                {
                    var table = new SnapshotTable
                    {
                        Name = dt.TableName
                    };

                    foreach (DataColumn col in dt.Columns)
                        table.Columns.Add(col.ColumnName);

                    foreach (DataRow row in dt.Rows)
                    {
                        var values = new List<string>(dt.Columns.Count);
                        foreach (var value in row.ItemArray)
                            values.Add(value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty);
                        table.Rows.Add(values);
                    }

                    script.Tables.Add(table);
                }

                category.Scripts.Add(script);
            }

            snapshot.Categories.Add(category);
        }
    }

    public sealed class ReportSnapshot
    {
        public DateTime CreatedAtUtc { get; set; }
        public string ServerName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public List<SnapshotCategory> Categories { get; set; } = new();
    }

    public sealed class SnapshotCategory
    {
        public string Name { get; set; } = string.Empty;
        public string? Error { get; set; }
        public List<SnapshotScript> Scripts { get; set; } = new();
    }

    public sealed class SnapshotScript
    {
        public string Name { get; set; } = string.Empty;
        public string? Error { get; set; }
        public List<SnapshotTable> Tables { get; set; } = new();
    }

    public sealed class SnapshotTable
    {
        public string? Name { get; set; }
        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}
