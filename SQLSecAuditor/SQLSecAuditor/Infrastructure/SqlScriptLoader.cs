using System.IO;
using System.Reflection;
using System.Text;

namespace SqlSecAuditor.Infrastructure
{
    public static class SqlScriptLoader
    {
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        private static readonly string RootNamespace = Assembly.GetExecutingAssembly().GetName().Name!;

        /// <summary>
        /// Returns the SQL text of a single embedded script.
        /// Category is the subfolder name, e.g. "General".
        /// </summary>
        public static async Task<string> LoadScriptAsync(string category, string scriptFileName)
        {
            var resourceName = BuildResourceName(category, scriptFileName);
            await using var stream = Assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns all SQL script texts for a given category folder, ordered by file name.
        /// </summary>
        public static async Task<IReadOnlyList<(string FileName, string Sql)>> LoadCategoryScriptsAsync(string category)
        {
            var prefix = BuildCategoryPrefix(category);

            var resources = Assembly
                .GetManifestResourceNames()
                .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                            n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (resources.Length == 0)
            {
                throw new FileNotFoundException($"No embedded SQL scripts found for category: {category}");
            }

            var results = new List<(string FileName, string Sql)>(resources.Length);

            foreach (var resourceName in resources)
            {
                await using var stream = Assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var sql = await reader.ReadToEndAsync();
                var fileName = resourceName.Substring(prefix.Length);
                results.Add((fileName, sql));
            }

            return results;
        }

        private static string BuildResourceName(string category, string scriptFileName)
        {
            var safeCategory = SanitizeResourceSegment(category);
            var safeName = SanitizeResourceSegment(scriptFileName);
            return $"{RootNamespace}.Scripts.{safeCategory}.{safeName}";
        }

        private static string BuildCategoryPrefix(string category)
        {
            var safeCategory = SanitizeResourceSegment(category);
            return $"{RootNamespace}.Scripts.{safeCategory}.";
        }

        /// <summary>
        /// .NET replaces characters that are not valid in resource names with '_'.
        /// This mirrors the replacement for the file name part.
        /// </summary>
        private static string SanitizeResourceSegment(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (var ch in name)
            {
                sb.Append(char.IsLetterOrDigit(ch) || ch == '.' || ch == '_' ? ch : '_');
            }
            return sb.ToString();
        }
    }
}
