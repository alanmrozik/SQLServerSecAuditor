using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlSecAuditor.Models
{
    public sealed class PdfExportCategoryOption : INotifyPropertyChanged
    {
        private bool _isSelected = true;

        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
