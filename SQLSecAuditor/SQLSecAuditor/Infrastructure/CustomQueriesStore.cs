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
            catch (IOException)
            {
                return Array.Empty<CustomQuery>();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<CustomQuery>();
            }
        }

        public static void Save(IEnumerable<CustomQuery> queries)
        {
            var directory = Path.GetDirectoryName(FilePath)!;
            Directory.CreateDirectory(directory);

            var temporaryPath = Path.Combine(directory, $"{Path.GetFileName(FilePath)}.{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(queries, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temporaryPath, FilePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
