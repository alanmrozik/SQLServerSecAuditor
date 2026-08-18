using SqlSecAuditor.Infrastructure;

namespace SQLSecAuditor.Tests;

public sealed class DangerousSqlDetectorTests
{
    [Theory]
    [InlineData("DELETE FROM dbo.AuditLog")]
    [InlineData("ALTER TABLE dbo.Users ADD IsActive bit")]
    [InlineData("EXEC xp_cmdshell 'powershell -Command whoami'")]
    [InlineData("GRANT CONTROL SERVER TO [user]")]
    public void FindRisks_DetectsPotentiallyDangerousSql(string sql)
    {
        Assert.NotEmpty(DangerousSqlDetector.FindRisks(sql));
    }

    [Fact]
    public void FindRisks_AllowsSimpleReadOnlyQuery()
    {
        Assert.Empty(DangerousSqlDetector.FindRisks("SELECT name FROM sys.databases"));
    }
}
