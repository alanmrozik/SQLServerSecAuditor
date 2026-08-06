using System.Collections.ObjectModel;

namespace SqlSecAuditor.Models
{
    public sealed class SnapshotViewerCategory
    {
        public string Name { get; set; } = string.Empty;
        public string? Error { get; set; }
        public ObservableCollection<ScriptExecutionResult> Scripts { get; } = new();
    }
}
