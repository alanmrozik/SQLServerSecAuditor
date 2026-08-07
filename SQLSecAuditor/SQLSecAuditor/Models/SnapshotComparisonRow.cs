namespace SqlSecAuditor.Models
{
    public sealed class SnapshotComparisonRow
    {
        public string Path { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string SnapshotValue { get; set; } = string.Empty;
        public string Marker { get; set; } = "=";
        public string ChangeType { get; set; } = "Unchanged";
    }
}
