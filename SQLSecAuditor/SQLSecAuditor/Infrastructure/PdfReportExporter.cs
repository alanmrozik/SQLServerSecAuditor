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
            var categories = BuildCategories(instance).ToList();

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
                                            scriptCol.Item().Text(script.Error).FontColor(Colors.Red.Darken2);
                                        }

                                        foreach (var table in script.Tables)
                                        {
                                            scriptCol.Item().Text(table.TableName ?? string.Empty).Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
                                            scriptCol.Item().Table(tableDescriptor => RenderDataTable(tableDescriptor, table));
                                        }
                                    });
                                }
                            });
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static IEnumerable<ExportCategory> BuildCategories(SqlInstance instance)
        {
            var categories = new List<ExportCategory>();

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

            AddCategoryIfExecuted(categories, "Utrzymanie i integralność", instance.MaintenanceIntegrityResults, instance.MaintenanceIntegrityError);
            AddCategoryIfExecuted(categories, "Sieć i łączność", instance.NetworkConnectivityResults, instance.NetworkConnectivityError);
            AddCategoryIfExecuted(categories, "Redukcja powierzchni ataku", instance.SurfaceAreaReductionResults, instance.SurfaceAreaReductionError);
            AddCategoryIfExecuted(categories, "Audyt i monitoring", instance.AuditingMonitoringResults, instance.AuditingMonitoringError);
            AddCategoryIfExecuted(categories, "Uwierzytelnianie i kontrola dostępu", instance.AuthenticationAccessControlResults, instance.AuthenticationAccessControlError);
            AddCategoryIfExecuted(categories, "Autoryzacja i uprawnienia", instance.AuthorizationPermissionsResults, instance.AuthorizationPermissionsError);
            AddCategoryIfExecuted(categories, "Bezpieczeństwo baz danych", instance.DatabaseSecurityResults, instance.DatabaseSecurityError);
            AddCategoryIfExecuted(categories, "Wysoka dostępność i odzyskiwanie po awarii", instance.HighAvailabilityDisasterRecoveryResults, instance.HighAvailabilityDisasterRecoveryError);

            return categories;
        }

        private static void AddCategoryIfExecuted(List<ExportCategory> categories, string title, IEnumerable<ScriptExecutionResult> results, string? error)
        {
            var scripts = results.ToList();
            if (scripts.Count == 0)
            {
                return;
            }

            categories.Add(new ExportCategory
            {
                Title = title,
                Error = error,
                Scripts = scripts.Select(script => new ExportScript
                {
                    ScriptName = script.ScriptName,
                    Error = script.Error,
                    Tables = script.Tables.Cast<DataTable>().ToArray()
                }).ToArray()
            });
        }

        private static void RenderDataTable(TableDescriptor table, DataTable dataTable)
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
                foreach (var value in row.ItemArray)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(FormatValue(value));
                }
            }
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
            public string Title { get; set; } = string.Empty;
            public string? Error { get; set; }
            public IReadOnlyList<ExportScript> Scripts { get; set; } = Array.Empty<ExportScript>();
        }

        private sealed class ExportScript
        {
            public string ScriptName { get; set; } = string.Empty;
            public string? Error { get; set; }
            public IReadOnlyList<DataTable> Tables { get; set; } = Array.Empty<DataTable>();
        }
    }
}
