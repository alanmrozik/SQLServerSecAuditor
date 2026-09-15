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
            Export(filePath, instance, PdfReportType.Audit, null);
        }

        public static void Export(string filePath, SqlInstance instance, IReadOnlyCollection<string>? selectedCategoryKeys)
        {
            Export(filePath, instance, PdfReportType.Audit, selectedCategoryKeys);
        }

        public static void Export(
            string filePath,
            SqlInstance instance,
            PdfReportType reportType,
            IReadOnlyCollection<string>? selectedCategoryKeys)
        {
            var categories = BuildCategories(instance, selectedCategoryKeys).ToList();
            var reportDate = DateTime.Now;

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
                        header.Item().Text(GetReportTitle(reportType)).FontSize(20).SemiBold();
                        header.Item().Text($"Instance: {instance.ServerName} | Database: {instance.DatabaseName}")
                            .FontSize(11)
                            .FontColor(Colors.Blue.Darken2);
                        header.Item().Text($"Report date: {reportDate:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().Column(content =>
                    {
                        content.Spacing(12);

                        if (reportType is PdfReportType.Combined or PdfReportType.Scoring)
                        {
                            content.Item().Element(scoring => RenderScoring(scoring, instance));
                        }

                        if (reportType is PdfReportType.Combined or PdfReportType.Audit)
                        {
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

                                            if (!string.IsNullOrWhiteSpace(script.Description))
                                            {
                                                scriptCol.Item().Text(script.Description)
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
                                                foreach (var columns in SplitColumnsForPdf(table))
                                                {
                                                    if (table.Columns.Count > MaxColumnsPerTable)
                                                    {
                                                        var first = table.Columns.IndexOf(columns[0]) + 1;
                                                        var last = table.Columns.IndexOf(columns[^1]) + 1;
                                                        scriptCol.Item().Text($"Columns {first}-{last} of {table.Columns.Count}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                                    }

                                                    scriptCol.Item().Table(tableDescriptor => RenderDataTable(tableDescriptor, table, script.ScriptName, columns));
                                                }
                                            }
                                        });
                                    }
                                });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static string GetReportTitle(PdfReportType reportType) => reportType switch
        {
            PdfReportType.Audit => "Security Audit Report",
            PdfReportType.Scoring => "Security Scoring Report",
            _ => "Security Audit and Scoring Report"
        };

        private static void RenderScoring(IContainer container, SqlInstance instance)
        {
            container.Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(12).Column(column =>
            {
                column.Spacing(6);
                column.Item().Text("Security Score").FontSize(15).SemiBold();
                column.Item().Text($"{instance.ScoringPercentDisplay} ({instance.ScoringDisplay})")
                    .FontSize(20)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Raw score: {instance.ScoringRawPoints:0.##} | Minimum: {instance.ScoringMinPoints:0.##} | Maximum: {instance.ScoringMaxPoints:0.##}");
                column.Item().Text($"Green results: {instance.ScoringGreenCount}   Yellow results: {instance.ScoringYellowCount}   Red results: {instance.ScoringRedCount}");
                column.Item().Text("Green results add one point, red results subtract one point, and yellow results require administrator review without changing the score.")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
            });
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
                    Title = "General Information",
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

            AddCategoryIfExecuted(categories, "maintenance_integrity", "Maintenance and Integrity", instance.MaintenanceIntegrityResults, instance.MaintenanceIntegrityError);
            AddCategoryIfExecuted(categories, "network_connectivity", "Network and Connectivity", instance.NetworkConnectivityResults, instance.NetworkConnectivityError);
            AddCategoryIfExecuted(categories, "surface_area_reduction", "Surface Area Reduction", instance.SurfaceAreaReductionResults, instance.SurfaceAreaReductionError);
            AddCategoryIfExecuted(categories, "auditing_monitoring", "Auditing and Monitoring", instance.AuditingMonitoringResults, instance.AuditingMonitoringError);
            AddCategoryIfExecuted(categories, "authentication_access_control", "Authentication and Access Control", instance.AuthenticationAccessControlResults, instance.AuthenticationAccessControlError);
            AddCategoryIfExecuted(categories, "authorization_permissions", "Authorization and Permissions", instance.AuthorizationPermissionsResults, instance.AuthorizationPermissionsError);
            AddCategoryIfExecuted(categories, "database_security", "Database Security", instance.DatabaseSecurityResults, instance.DatabaseSecurityError);
            AddCategoryIfExecuted(categories, "high_availability_disaster_recovery", "High Availability and Disaster Recovery", instance.HighAvailabilityDisasterRecoveryResults, instance.HighAvailabilityDisasterRecoveryError);

            AddCategoryIfExecuted(categories, "custom_queries", "Custom Queries", instance.CustomQueryResults, instance.CustomQueriesError);

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

        private const int MaxColumnsPerTable = 5;

        private static IEnumerable<IReadOnlyList<DataColumn>> SplitColumnsForPdf(DataTable dataTable)
        {
            return dataTable.Columns.Cast<DataColumn>()
                .Select((column, index) => new { column, index })
                .GroupBy(item => item.index / MaxColumnsPerTable)
                .Select(group => (IReadOnlyList<DataColumn>)group.Select(item => item.column).ToArray());
        }

        private static void RenderDataTable(TableDescriptor table, DataTable dataTable, string scriptName, IReadOnlyList<DataColumn> columns)
        {
            table.ColumnsDefinition(descriptor =>
            {
                descriptor.ConstantColumn(28);
                foreach (var column in columns)
                {
                    descriptor.RelativeColumn();
                }
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").SemiBold();
                foreach (var column in columns)
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(column.ColumnName).SemiBold();
                }
            });

            if (dataTable.Rows.Count == 0)
            {
                table.Cell().ColumnSpan((uint)(columns.Count + 1)).Padding(4).Text("No rows.").FontColor(Colors.Grey.Darken1);
                return;
            }

            for (var rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                var row = dataTable.Rows[rowIndex];
                var rowBackground = EvaluateRowColorHex(scriptName, dataTable, row);
                var indexCell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4);
                if (!string.IsNullOrWhiteSpace(rowBackground)) indexCell = indexCell.Background(rowBackground);
                indexCell.Text((rowIndex + 1).ToString(CultureInfo.InvariantCulture));

                foreach (var column in columns)
                {
                    var cell = table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4);
                    if (!string.IsNullOrWhiteSpace(rowBackground))
                    {
                        cell = cell.Background(rowBackground);
                    }

                    cell.Text(FormatValue(row[column]));
                }
            }
        }

        private static string? EvaluateRowColorHex(string scriptName, DataTable table, DataRow row)
        {
            var evaluation = RowEvaluationService.Evaluate(scriptName, table, row);
            return RowEvaluationService.ToColorHex(evaluation);
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
