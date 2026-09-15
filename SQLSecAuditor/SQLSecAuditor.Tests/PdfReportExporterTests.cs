using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Data;

namespace SQLSecAuditor.Tests;

public sealed class PdfReportExporterTests
{
    [Theory]
    [InlineData(PdfReportType.Audit)]
    [InlineData(PdfReportType.Scoring)]
    [InlineData(PdfReportType.Combined)]
    public void Export_CreatesValidPdfForEveryReportType(PdfReportType reportType)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"sqlsecauditor-{Guid.NewGuid():N}.pdf");
        try
        {
            var instance = CreateInstance();

            PdfReportExporter.Export(filePath, instance, reportType, new[] { "network_connectivity" });

            var bytes = File.ReadAllBytes(filePath);
            Assert.True(bytes.Length > 1_000);
            Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static SqlInstance CreateInstance()
    {
        var instance = new SqlInstance
        {
            ServerName = "TEST-SQL",
            DatabaseName = "master",
            ScoringRawPoints = 1,
            ScoringPoints = 1,
            ScoringMinPoints = -1,
            ScoringMaxPoints = 1,
            ScoringGreenCount = 1
        };

        var table = new DataTable("Settings");
        table.Columns.Add("Status");
        table.Rows.Add("Disabled");
        var result = new ScriptExecutionResult { ScriptName = "Check_Remote_Admin_Connections" };
        result.Tables.Add(table);
        instance.NetworkConnectivityResults.Add(result);
        return instance;
    }
}
