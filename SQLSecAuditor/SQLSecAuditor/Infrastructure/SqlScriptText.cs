using System.IO;
using System.Text;

namespace SqlSecAuditor.Infrastructure
{
    /// <summary>Extracts application metadata from embedded audit scripts.</summary>
    public static class SqlScriptText
    {
        public static string RemoveBatchSeparators(string script)
        {
            ArgumentNullException.ThrowIfNull(script);

            var builder = new StringBuilder();
            using var reader = new StringReader(script);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (!line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendLine(line);
                }
            }

            return builder.ToString();
        }

        public static string? ExtractFixScript(string script) => ExtractCommentValue(script, "Fix:");

        public static string? ExtractDescription(string script)
        {
            var description = ExtractCommentValue(script, "Description:");
            if (description is not null)
            {
                var rationaleIndex = description.IndexOf("Rationale:", StringComparison.OrdinalIgnoreCase);
                return NormalizeWhitespace(rationaleIndex >= 0 ? description[..rationaleIndex] : description);
            }

            return ExtractLeadingComment(script);
        }

        private static string? ExtractCommentValue(string script, string token)
        {
            if (string.IsNullOrWhiteSpace(script)) return null;

            var tokenIndex = script.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (tokenIndex < 0) return null;

            var blockStart = script.LastIndexOf("/*", tokenIndex, StringComparison.Ordinal);
            var blockEnd = script.IndexOf("*/", tokenIndex, StringComparison.Ordinal);
            if (blockStart < 0 || blockEnd <= blockStart) return null;

            var block = script[(blockStart + 2)..blockEnd];
            var valueIndex = block.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            return valueIndex < 0 ? null : block[(valueIndex + token.Length)..].Trim();
        }

        private static string? ExtractLeadingComment(string script)
        {
            if (string.IsNullOrWhiteSpace(script)) return null;

            using var reader = new StringReader(script);
            string? line;
            while ((line = reader.ReadLine()) is not null && string.IsNullOrWhiteSpace(line)) { }
            if (line is null) return null;

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("/*", StringComparison.Ordinal))
            {
                var endIndex = trimmed.IndexOf("*/", StringComparison.Ordinal);
                if (endIndex >= 0) return NormalizeWhitespace(trimmed[2..endIndex]);

                var content = new StringBuilder(trimmed[2..]);
                while ((line = reader.ReadLine()) is not null)
                {
                    endIndex = line.IndexOf("*/", StringComparison.Ordinal);
                    if (endIndex >= 0)
                    {
                        content.AppendLine(line[..endIndex]);
                        break;
                    }

                    content.AppendLine(line);
                }

                return NormalizeWhitespace(content.ToString());
            }

            if (!trimmed.StartsWith("--", StringComparison.Ordinal)) return null;

            var lines = new StringBuilder();
            do
            {
                lines.AppendLine(trimmed.Length > 2 ? trimmed[2..].TrimStart() : string.Empty);
                line = reader.ReadLine();
                trimmed = line?.TrimStart() ?? string.Empty;
            }
            while (line is not null && trimmed.StartsWith("--", StringComparison.Ordinal));

            return NormalizeWhitespace(lines.ToString());
        }

        private static string NormalizeWhitespace(string input) => string.Join(" ", input
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim()))
            .Trim();
    }
}
