using SqlSecAuditor.Models;
using System.IO;
using System.Text.Json;

namespace SqlSecAuditor.Infrastructure
{
    public static class CustomQueriesStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SQLServerSecAuditor",
            "custom-queries.json");

        public static IReadOnlyList<CustomQuery> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return Array.Empty<CustomQuery>();
                return JsonSerializer.Deserialize<List<CustomQuery>>(File.ReadAllText(FilePath)) ?? new List<CustomQuery>();
            }
            catch (JsonException)
            {
                return Array.Empty<CustomQuery>();
            }
        }

        public static void Save(IEnumerable<CustomQuery> queries)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(queries, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
