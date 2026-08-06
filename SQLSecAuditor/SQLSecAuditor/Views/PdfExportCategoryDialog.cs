using SqlSecAuditor.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SqlSecAuditor.Views
{
    public class PdfExportCategoryDialog : Window
    {
        private readonly ObservableCollection<PdfExportCategoryOption> _options;

        public PdfExportCategoryDialog(ObservableCollection<PdfExportCategoryOption> options)
        {
            _options = options;

            Title = "Wybierz kategorie do PDF";
            Width = 560;
            Height = 520;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Application.Current.TryFindResource("AppWindowBackgroundBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White;

            Content = BuildLayout();
        }

        public IReadOnlyList<string> SelectedCategoryKeys =>
            _options.Where(o => o.IsSelected).Select(o => o.Key).ToList();

        private UIElement BuildLayout()
        {
            var root = new Grid { Margin = new Thickness(16) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(new TextBlock
            {
                Text = "Wybierz uruchomione kategorie, które mają trafić do PDF:",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var quickActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var selectAllButton = new Button { Content = "Zaznacz wszystko", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
            selectAllButton.Click += (_, __) =>
            {
                foreach (var option in _options)
                {
                    option.IsSelected = true;
                }
            };

            var clearAllButton = new Button { Content = "Odznacz wszystko", Padding = new Thickness(10, 4, 10, 4) };
            clearAllButton.Click += (_, __) =>
            {
                foreach (var option in _options)
                {
                    option.IsSelected = false;
                }
            };

            quickActions.Children.Add(selectAllButton);
            quickActions.Children.Add(clearAllButton);
            root.Children.Add(quickActions);
            Grid.SetRow(quickActions, 1);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var optionsPanel = new StackPanel();
            foreach (var option in _options)
            {
                var cb = new CheckBox
                {
                    Content = option.Name,
                    IsChecked = option.IsSelected,
                    Margin = new Thickness(0, 0, 0, 8),
                    FontSize = 13
                };
                cb.Checked += (_, __) => option.IsSelected = true;
                cb.Unchecked += (_, __) => option.IsSelected = false;
                optionsPanel.Children.Add(cb);
            }

            scroll.Content = optionsPanel;
            root.Children.Add(scroll);
            Grid.SetRow(scroll, 2);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };

            var cancelButton = new Button { Content = "Anuluj", Width = 100, Margin = new Thickness(0, 0, 8, 0) };
            cancelButton.Click += (_, __) => DialogResult = false;

            var exportButton = new Button { Content = "Eksportuj", Width = 120, Background = System.Windows.Media.Brushes.SteelBlue, Foreground = System.Windows.Media.Brushes.White };
            exportButton.Click += (_, __) =>
            {
                if (_options.All(o => !o.IsSelected))
                {
                    MessageBox.Show(this, "Wybierz przynajmniej jedną kategorię.", "PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DialogResult = true;
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(exportButton);

            root.Children.Add(buttons);
            Grid.SetRow(buttons, 3);

            return root;
        }
    }
}
