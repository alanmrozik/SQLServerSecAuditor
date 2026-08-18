using SqlSecAuditor.Infrastructure;

namespace SQLSecAuditor.Tests;

public sealed class SqlScriptTextTests
{
    [Fact]
    public void RemoveBatchSeparators_RemovesOnlyStandaloneGoLines()
    {
        const string script = "SELECT 1;\nGO\nSELECT 'GO';\n go \n";

        var result = SqlScriptText.RemoveBatchSeparators(script);

        Assert.Equal("SELECT 1;" + Environment.NewLine + "SELECT 'GO';" + Environment.NewLine, result);
    }

    [Fact]
    public void ExtractDescription_ReturnsDescriptionWithoutRationale()
    {
        const string script = "/* Description: Checks remote access. Rationale: Security baseline. */\nSELECT 1;";

        var result = SqlScriptText.ExtractDescription(script);

        Assert.Equal("Checks remote access.", result);
    }

    [Fact]
    public void ExtractFixScript_ReturnsSqlPreservingFormatting()
    {
        const string script = "/* Fix:\nEXECUTE sp_configure 'remote access', 0;\nRECONFIGURE;\n*/";

        var result = SqlScriptText.ExtractFixScript(script);

        Assert.Equal("EXECUTE sp_configure 'remote access', 0;\nRECONFIGURE;", result?.Replace("\r\n", "\n"));
    }
}
