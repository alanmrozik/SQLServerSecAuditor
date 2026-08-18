using Microsoft.Data.SqlClient;
using System.Data;

namespace SqlSecAuditor.Infrastructure
{
    /// <summary>Converts a SQL result set into a bindable <see cref="DataTable"/>.</summary>
    public static class SqlDataReaderTableReader
    {
        public static async Task<DataTable> ReadAsync(SqlDataReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);

            var table = new DataTable();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnType = typeof(object);
                try
                {
                    columnType = reader.GetFieldType(i) ?? typeof(object);
                }
                catch (InvalidOperationException)
                {
                    // Some providers cannot expose the type; an object column remains safe to bind.
                }

                table.Columns.Add(GetUniqueColumnName(table, reader.GetName(i), i), columnType);
            }

            while (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }

                table.Rows.Add(values);
            }

            return table;
        }

        private static string GetUniqueColumnName(DataTable table, string? proposedName, int ordinal)
        {
            var baseName = string.IsNullOrWhiteSpace(proposedName) ? $"Column{ordinal + 1}" : proposedName;
            var name = baseName;
            var suffix = 2;

            while (table.Columns.Contains(name))
            {
                name = $"{baseName}_{suffix++}";
            }

            return name;
        }
    }
}
