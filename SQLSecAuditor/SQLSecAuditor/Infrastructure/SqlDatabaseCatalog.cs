using Microsoft.Data.SqlClient;

namespace SqlSecAuditor.Infrastructure
{
    public static class SqlDatabaseCatalog
    {
        private const string OnlineUserDatabasesQuery = """
            SELECT name
            FROM sys.databases
            WHERE state_desc = 'ONLINE'
              AND name NOT IN ('master', 'tempdb', 'model', 'msdb')
            ORDER BY name
            """;

        public static async Task<IReadOnlyList<string>> GetOnlineUserDatabasesAsync(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            var names = new List<string>();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = OnlineUserDatabasesQuery;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }

            return names;
        }
    }
}
