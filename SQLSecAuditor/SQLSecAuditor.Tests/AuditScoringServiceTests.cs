using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Data;

namespace SQLSecAuditor.Tests;

public sealed class AuditScoringServiceTests
{
    [Fact]
    public void Recalculate_UsesGreenAndRedResultsForScore()
    {
        var instance = new SqlInstance();
        instance.NetworkConnectivityResults.Add(CreateResult("Check_Remote_Access", "Status", "Disabled"));
        instance.NetworkConnectivityResults.Add(CreateResult("Check_Remote_Admin_Connections", "Status", "Enabled"));

        AuditScoringService.Recalculate(instance);

        Assert.Equal(1, instance.ScoringGreenCount);
        Assert.Equal(1, instance.ScoringRedCount);
        Assert.Equal(0, instance.ScoringPoints);
        Assert.Equal("1 / 2", instance.ScoringDisplay);
    }

    [Fact]
    public void ApplyHighAvailabilityContext_SharesEnabledStateAcrossResults()
    {
        var enabled = CreateResult("IsAGEnabled", "Status", "1");
        var disabled = CreateResult("IsClustered", "Status", "0");

        AuditScoringService.ApplyHighAvailabilityContext(new[] { enabled, disabled });

        Assert.True(enabled.Tables[0].ExtendedProperties["HaDrAnyEnabled"] is true);
        Assert.True(disabled.Tables[0].ExtendedProperties["HaDrAnyEnabled"] is true);
    }

    private static ScriptExecutionResult CreateResult(string scriptName, string columnName, string value)
    {
        var table = new DataTable();
        table.Columns.Add(columnName);
        table.Rows.Add(value);
        var result = new ScriptExecutionResult { ScriptName = scriptName };
        result.Tables.Add(table);
        return result;
    }
}
