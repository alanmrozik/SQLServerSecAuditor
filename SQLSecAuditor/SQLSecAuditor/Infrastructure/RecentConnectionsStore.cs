using SqlSecAuditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SqlSecAuditor.Infrastructure
{
    public static class RecentConnectionsStore
    {
        private const int MaxEntries = 10;

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
                return JsonSerializer.Deserialize<List<SavedConnection>>(json) ?? new List<SavedConnection>();
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

                // Remove duplicate (same server+port+db+auth mode+user)
                list.RemoveAll(c =>
                    string.Equals(c.ServerName, entry.ServerName, StringComparison.OrdinalIgnoreCase) &&
                    c.Port == entry.Port &&
                    string.Equals(c.DatabaseName, entry.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
                    c.UseWindowsAuthentication == entry.UseWindowsAuthentication &&
                    string.Equals(c.SqlUserName, entry.SqlUserName, StringComparison.OrdinalIgnoreCase));

                list.Insert(0, entry);

                if (list.Count > MaxEntries)
                    list = list.Take(MaxEntries).ToList();

                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Silently ignore persistence errors — not critical
            }
        }
    }
}
