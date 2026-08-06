using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SqlSecAuditor.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SqlSecAuditor.Infrastructure
{
    public static class PdfReportExporter
    {
        static PdfReportExporter()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static void Export(string filePath, SqlInstance instance)
        {
            Export(filePath, instance, null);
        }

        public static void Export(string filePath, SqlInstance instance, IReadOnlyCollection<string>? selectedCategoryKeys)
        {
            var categories = BuildCategories(instance, selectedCategoryKeys).ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(header =>
                    {
                        header.Spacing(4);
                        header.Item().Text("Raport Audytu").FontSize(20).SemiBold();
                        header.Item().Text($"Instancja: {instance.ServerName} | Baza: {instance.DatabaseName}")
                            .FontSize(11)
                            .FontColor(Colors.Blue.Darken2);
                    });

                    page.Content().Column(content =>
                    {
                        content.Spacing(12);

                        foreach (var category in categories)
                        {
                            content.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                            {
                                col.Spacing(8);
                                col.Item().Text(category.Title).FontSize(14).SemiBold();

                                if (!string.IsNullOrWhiteSpace(category.Error))
                                {
                                    col.Item().Text(category.Error).FontColor(Colors.Red.Darken2);
                                }

                foreach (var script in category.Scripts)
                                {
                                    col.Item().PaddingTop(4).Column(scriptCol =>
                                    {
                                        scriptCol.Spacing(5);
                        scriptCol.Item().Text(script.ScriptName).SemiBold();

                        if (!string.IsNullOrWhiteSpace(script.Error))
                        {
                            // leave error handling below
                        }

                        // include script description if present
                        if (!string.IsNullOrWhiteSpace(script is ExportScript es ? es.Description : null))
                        {
                            scriptCol.Item().Text((script as ExportScript)?.Description ?? string.Empty)
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken2);
                        }

                                        if (!string.IsNullOrWhiteSpace(script.Error))
                                        {
                                            scriptCol.Item().Text(script.Error).FontColor(Colors.Red.Darken2);
                                        }

                                        foreach (var table in script.Tables)
                                        {
                                            scriptCol.Item().Text(table.TableName ?? string.Empty).Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                                            scriptCol.Item().Table(tableDescriptor => RenderDataTable(tableDescriptor, table, script.ScriptName));
                                        }
                                    });
                                }
                            });
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static IEnumerable<ExportCategory> BuildCategories(SqlInstance instance, IReadOnlyCollection<string>? selectedCategoryKeys)
        {
            var categories = new List<ExportCategory>();
            var selected = selectedCategoryKeys is null
                ? null
                : new HashSet<string>(selectedCategoryKeys, StringComparer.OrdinalIgnoreCase);

            if (instance.IsGeneralInfoLoaded)
            {
                var table = new DataTable();
                table.Columns.Add("Label");
                table.Columns.Add("Value");

                foreach (var entry in instance.GeneralInfoEntries)
                {
                    table.Rows.Add(entry.Label, entry.Value);
                }

                categories.Add(new ExportCategory
                {
                    Key = "general",
                    Title = "Informacje Ogólne",
                    Scripts = new[]
                    {
                        new ExportScript
                        {
                            ScriptName = "GeneralInfoAboutServer",
                            Tables = new[] { table }
                        }
                    }
                });
            }

            AddCategoryIfExecuted(categories, "maintenance_integrity", "Utrzymanie i integralność", instance.MaintenanceIntegrityResults, instance.MaintenanceIntegrityError);
            AddCategoryIfExecuted(categories, "network_connectivity", "Sieć i łączność", instance.NetworkConnectivityResults, instance.NetworkConnectivityError);
            AddCategoryIfExecuted(categories, "surface_area_reduction", "Redukcja powierzchni ataku", instance.SurfaceAreaReductionResults, instance.SurfaceAreaReductionError);
            AddCategoryIfExecuted(categories, "auditing_monitoring", "Audyt i monitoring", instance.AuditingMonitoringResults, instance.AuditingMonitoringError);
            AddCategoryIfExecuted(categories, "authentication_access_control", "Uwierzytelnianie i kontrola dostępu", instance.AuthenticationAccessControlResults, instance.AuthenticationAccessControlError);
            AddCategoryIfExecuted(categories, "authorization_permissions", "Autoryzacja i uprawnienia", instance.AuthorizationPermissionsResults, instance.AuthorizationPermissionsError);
            AddCategoryIfExecuted(categories, "database_security", "Bezpieczeństwo baz danych", instance.DatabaseSecurityResults, instance.DatabaseSecurityError);
            AddCategoryIfExecuted(categories, "high_availability_disaster_recovery", "Wysoka dostępność i odzyskiwanie po awarii", instance.HighAvailabilityDisasterRecoveryResults, instance.HighAvailabilityDisasterRecoveryError);

            return selected is null ? categories : categories.Where(c => selected.Contains(c.Key));
        }

        private static void AddCategoryIfExecuted(List<ExportCategory> categories, string key, string title, IEnumerable<ScriptExecutionResult> results, string? error)
        {
            var scripts = results.ToList();
            if (scripts.Count == 0)
            {
                return;
            }

            categories.Add(new ExportCategory
            {
                Key = key,
                Title = title,
                Error = error,
                Scripts = scripts.Select(script => new ExportScript
                {
                    ScriptName = script.ScriptName,
                    Error = script.Error,
                    Tables = script.Tables.Cast<DataTable>().ToArray(),
                    Description = script.Description
                }).ToArray()
            });
        }

        private static void RenderDataTable(TableDescriptor table, DataTable dataTable, string scriptName)
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    columns.RelativeColumn();
                }
            });

            table.Header(header =>
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(column.ColumnName).SemiBold();
                }
            });

            if (dataTable.Rows.Count == 0)
            {
                table.Cell().ColumnSpan((uint)dataTable.Columns.Count).Padding(4).Text("Brak wierszy.").FontColor(Colors.Grey.Darken1);
                return;
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var rowBackground = EvaluateRowColorHex(scriptName, dataTable, row);

                foreach (var value in row.ItemArray)
                {
                    var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4);
                    if (!string.IsNullOrWhiteSpace(rowBackground))
                    {
                        cell = cell.Background(rowBackground);
                    }

                    cell.Text(FormatValue(value));
                }
            }
        }

        private static string? EvaluateRowColorHex(string scriptName, DataTable table, DataRow row)
        {
            const string green = "#D4EFDF";
            const string red = "#FADBD8";
            const string yellow = "#FCF3CF";

            var normalizedScript = NormalizeToken(scriptName);
            var rowText = string.Join(" | ", row.ItemArray.Select(ToText)).ToLowerInvariant();

            if (normalizedScript.Contains("encryptionchecks") || normalizedScript.Contains("generalinfoaboutserver"))
                return null;

            if (normalizedScript.Contains("builtinlogins")
                || normalizedScript.Contains("expirationforsqlloginsysadmins")
                || normalizedScript.Contains("sysadminlogins")
                || normalizedScript.Contains("guestpermissions")
                || normalizedScript.Contains("permissionsondblevelpoprawiony")
                || normalizedScript.Contains("serviceaccounts")
                || normalizedScript.Contains("sqlserverport"))
                return yellow;

            if (normalizedScript.Contains("orphanedusers")
                || normalizedScript.Contains("publicroleisnotgrantedtoproxies")
                || normalizedScript.Contains("autoclose")
                || normalizedScript.Contains("clrenabled"))
                return red;

            if (normalizedScript.Contains("defaulttraceenabled"))
                return rowText.Contains("enabled") ? green : rowText.Contains("disabled") ? red : null;

            if (normalizedScript.Contains("loginauditing"))
                return rowText.Contains("failed") && rowText.Contains("login") ? green : red;

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
                return rowText.Contains("disabled") ? green : rowText.Contains("enabled") ? red : null;

            if (normalizedScript.Contains("passwordpolicyforsqllogins"))
            {
                if (rowText.Contains("not checked") || rowText.Contains("n/a") || rowText.Contains("0"))
                    return red;
                if (rowText.Contains("checked") || rowText.Contains("1") || rowText.Contains("true"))
                    return green;
                return null;
            }

            if (normalizedScript.Contains("hideinstance"))
            {
                if (HasExactValue(row, "1")) return green;
                if (HasExactValue(row, "0")) return red;
                return null;
            }

            if (normalizedScript.Contains("ifconnectionusekerberos"))
                return rowText.Contains("kerberos") ? green : rowText.Contains("ntlm") ? red : null;

            if (normalizedScript.Contains("isagenabled")
                || normalizedScript.Contains("isclustered")
                || normalizedScript.Contains("islogshipped")
                || normalizedScript.Contains("ismirrored")
                || normalizedScript.Contains("isreplicated"))
            {
                if (HasExactValue(row, "1")) return green;
                if (HasExactValue(row, "0"))
                {
                    var hasAnyEnabled = table.ExtendedProperties["HaDrAnyEnabled"] as bool? == true;
                    return hasAnyEnabled ? yellow : red;
                }

                return null;
            }

            if (normalizedScript.Contains("lastbackupdates"))
            {
                if (TryFindDate(row, out var dt))
                    return dt >= DateTime.Now.AddMonths(-1) ? green : red;
                return red;
            }

            if (normalizedScript.Contains("lastknowgoodcheckdb"))
            {
                if (TryFindDate(row, out var dt))
                {
                    if (dt.Year == 1900) return red;
                    if (dt >= DateTime.Now.AddMonths(-1)) return green;
                }

                return null;
            }

            return null;
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
            if (value is null || value == DBNull.Value)
                return string.Empty;

            return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        }

        private static string NormalizeToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private static string FormatValue(object? value)
        {
            if (value is null || value == DBNull.Value)
                return string.Empty;

            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }

        private sealed class ExportCategory
        {
            public string Key { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Error { get; set; }
            public IReadOnlyList<ExportScript> Scripts { get; set; } = Array.Empty<ExportScript>();
        }

        private sealed class ExportScript
        {
            public string ScriptName { get; set; } = string.Empty;
            public string? Error { get; set; }
            public IReadOnlyList<DataTable> Tables { get; set; } = Array.Empty<DataTable>();
            public string? Description { get; set; }
        }
    }
}
