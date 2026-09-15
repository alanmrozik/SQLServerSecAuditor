using SqlSecAuditor.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SqlSecAuditor.Views
{
    public class PdfExportCategoryDialog : Window
    {
        private readonly ObservableCollection<PdfExportCategoryOption> _options;
        private readonly StackPanel _optionsPanel = new();
        private readonly StackPanel _categoryList = new();
        private readonly Button _exportButton = new();
        private PdfReportType _reportType;

        public PdfExportCategoryDialog(ObservableCollection<PdfExportCategoryOption> options)
        {
            _options = options;

            _reportType = options.Count > 0 ? PdfReportType.Combined : PdfReportType.Scoring;
            Title = "Export PDF report";
            Width = 560;
            Height = 650;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Application.Current.TryFindResource("AppWindowBackgroundBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White;

            Content = BuildLayout();
        }

        public IReadOnlyList<string> SelectedCategoryKeys =>
            _options.Where(o => o.IsSelected).Select(o => o.Key).ToList();

        public PdfReportType ReportType => _reportType;

        private UIElement BuildLayout()
        {
            var root = new Grid { Margin = new Thickness(16) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(new TextBlock
            {
                Text = "Choose the report content:",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var reportTypes = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };
            reportTypes.Children.Add(CreateReportTypeRadio("Combined audit and scoring report", PdfReportType.Combined, _reportType == PdfReportType.Combined));
            reportTypes.Children.Add(CreateReportTypeRadio("Audit report", PdfReportType.Audit, false));
            reportTypes.Children.Add(CreateReportTypeRadio("Scoring report", PdfReportType.Scoring, _reportType == PdfReportType.Scoring));
            root.Children.Add(reportTypes);
            Grid.SetRow(reportTypes, 1);

            var quickActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var selectAllButton = new Button { Content = "Select all", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
            selectAllButton.Click += (_, __) =>
            {
                foreach (var option in _options)
                {
                    option.IsSelected = true;
                }
            };

            var clearAllButton = new Button { Content = "Clear all", Padding = new Thickness(10, 4, 10, 4) };
            clearAllButton.Click += (_, __) =>
            {
                foreach (var option in _options)
                {
                    option.IsSelected = false;
                }
            };

            quickActions.Children.Add(selectAllButton);
            quickActions.Children.Add(clearAllButton);
            _optionsPanel.Children.Add(new TextBlock
            {
                Text = "Audit categories:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            _optionsPanel.Children.Add(quickActions);
            root.Children.Add(_optionsPanel);
            Grid.SetRow(_optionsPanel, 2);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            foreach (var option in _options)
            {
                var cb = new CheckBox
                {
                    Content = option.Name,
                    Margin = new Thickness(0, 0, 0, 8),
                    FontSize = 13,
                    DataContext = option
                };

                cb.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(PdfExportCategoryOption.IsSelected))
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

                _categoryList.Children.Add(cb);
            }

            if (_options.Count == 0)
            {
                _categoryList.Children.Add(new TextBlock
                {
                    Text = "No audit categories have been run yet. A scoring-only report is still available.",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                });
            }

            scroll.Content = _categoryList;
            root.Children.Add(scroll);
            Grid.SetRow(scroll, 3);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };

            var cancelButton = new Button { Content = "Cancel", Width = 100, Margin = new Thickness(0, 0, 8, 0) };
            cancelButton.Click += (_, __) => DialogResult = false;

            _exportButton.Content = "Export";
            _exportButton.Width = 120;
            _exportButton.Background = System.Windows.Media.Brushes.SteelBlue;
            _exportButton.Foreground = System.Windows.Media.Brushes.White;
            _exportButton.Click += (_, __) =>
            {
                if (_reportType != PdfReportType.Scoring && _options.All(o => !o.IsSelected))
                {
                    MessageBox.Show(this, "Select at least one audit category.", "PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(_exportButton);

            root.Children.Add(buttons);
            Grid.SetRow(buttons, 4);

            UpdateReportTypeState();

            return root;
        }

        private RadioButton CreateReportTypeRadio(string label, PdfReportType reportType, bool isChecked)
        {
            var radio = new RadioButton
            {
                Content = label,
                GroupName = "PdfReportType",
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 0, 7),
                Tag = reportType
            };
            radio.Checked += (_, __) =>
            {
                _reportType = reportType;
                UpdateReportTypeState();
            };
            return radio;
        }

        private void UpdateReportTypeState()
        {
            if (_optionsPanel is null || _exportButton is null)
            {
                return;
            }

            _optionsPanel.IsEnabled = _reportType != PdfReportType.Scoring;
            _categoryList.IsEnabled = _reportType != PdfReportType.Scoring;
            _exportButton.IsEnabled = _reportType == PdfReportType.Scoring || _options.Count > 0;
        }
    }
}
