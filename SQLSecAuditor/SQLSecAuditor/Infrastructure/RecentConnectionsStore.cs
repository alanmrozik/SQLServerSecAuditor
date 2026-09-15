using SqlSecAuditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SqlSecAuditor.Infrastructure
{
    public static class RecentConnectionsStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SQLServerSecAuditor",
            "recent_connections.json");

        public static List<SavedConnection> Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new List<SavedConnection>();

                var json = File.ReadAllText(FilePath);
                var connections = JsonSerializer.Deserialize<List<SavedConnection>>(json) ?? new List<SavedConnection>();
                var requiresMigration = false;
                foreach (var connection in connections)
                {
                    if (connection.Id == Guid.Empty)
                    {
                        connection.Id = Guid.NewGuid();
                        requiresMigration = true;
                    }
                }

                if (requiresMigration)
                {
                    Write(connections);
                }

                return connections;
            }
            catch
            {
                return new List<SavedConnection>();
            }
        }

        public static void Save(SavedConnection entry)
        {
            try
            {
                var list = Load();

                if (entry.Id == Guid.Empty)
                {
                    entry.Id = Guid.NewGuid();
                }

                var existing = list.Find(c => c.Id == entry.Id) ?? list.Find(c => IsEquivalent(c, entry));
                if (existing is not null)
                {
                    entry.Id = existing.Id;
                }

                // Keep saved connections unique and order them by most recent save/use.
                list.RemoveAll(c => c.Id == entry.Id || IsEquivalent(c, entry));
                list.Insert(0, entry.Copy());

                Write(list);
            }
            catch
            {
                // Persistence is helpful but must never prevent connecting.
            }
        }

        public static void Delete(Guid id)
        {
            try
            {
                var list = Load();
                list.RemoveAll(c => c.Id == id);
                Write(list);
            }
            catch
            {
                // Persistence failures are non-fatal for the running application.
            }
        }

        private static bool IsEquivalent(SavedConnection left, SavedConnection right) =>
            string.Equals(left.ServerName, right.ServerName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(left.Port, right.Port, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(left.DatabaseName, right.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
            left.UseWindowsAuthentication == right.UseWindowsAuthentication &&
            string.Equals(left.SqlUserName, right.SqlUserName, StringComparison.OrdinalIgnoreCase);

        private static void Write(IEnumerable<SavedConnection> connections)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(
                FilePath,
                JsonSerializer.Serialize(connections, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
