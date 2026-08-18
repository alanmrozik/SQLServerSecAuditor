using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using System.Windows;
using System.Windows.Controls;

namespace SqlSecAuditor.Views
{
    public sealed class CustomQueryDialog : Window
    {
        private readonly TextBox _nameBox;
        private readonly TextBox _sqlBox;

        public CustomQuery? Query { get; private set; }

        public CustomQueryDialog()
        {
            Title = "Dodaj własne zapytanie";
            Width = 650;
            Height = 500;
            MinWidth = 500;
            MinHeight = 380;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = Application.Current.TryFindResource("AppWindowBackgroundBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White;

            var shell = new Border
            {
                Background = Application.Current.TryFindResource("AppPanelBackgroundBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White,
                BorderBrush = Application.Current.TryFindResource("AppPanelBorderBrush") as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(18),
                Padding = new Thickness(22)
            };
            var panel = new Grid();
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            panel.Children.Add(new TextBlock { Text = "NAZWA ZAPYTANIA", FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = Application.Current.TryFindResource("AppMutedTextBrush") as System.Windows.Media.Brush, Margin = new Thickness(0, 0, 0, 5) });
            _nameBox = new TextBox { Margin = new Thickness(0, 0, 0, 14), ToolTip = "Nazwa zapytania" };
            Grid.SetRow(_nameBox, 1);
            panel.Children.Add(_nameBox);

            var sqlLabel = new TextBlock { Text = "TREŚĆ SQL", FontWeight = FontWeights.SemiBold, Foreground = Application.Current.TryFindResource("AppMutedTextBrush") as System.Windows.Media.Brush, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(sqlLabel, 2);
            panel.Children.Add(sqlLabel);
            _sqlBox = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                TextWrapping = TextWrapping.Wrap,
                Height = double.NaN,
                MinHeight = 180,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(4, 2, 0, 0),
                Margin = new Thickness(0, 0, 0, 14),
                ToolTip = "Wklej lub wpisz zapytanie SQL"
            };
            Grid.SetRow(_sqlBox, 3);
            panel.Children.Add(_sqlBox);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "Anuluj", IsCancel = true, Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 0) };
            var save = new Button { Content = "Dodaj", IsDefault = true, Padding = new Thickness(12, 5, 12, 5) };
            save.Click += Save_Click;
            buttons.Children.Add(cancel);
            buttons.Children.Add(save);
            Grid.SetRow(buttons, 4);
            panel.Children.Add(buttons);

            shell.Child = panel;
            Content = shell;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var name = _nameBox.Text.Trim();
            var sql = _sqlBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sql))
            {
                MessageBox.Show(this, "Podaj nazwę i treść zapytania SQL.", "Własne zapytanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var risks = DangerousSqlDetector.FindRisks(sql);
            if (risks.Count > 0)
            {
                var warning = new DangerousQueryWarningDialog(risks) { Owner = this };
                if (warning.ShowDialog() != true)
                {
                    return;
                }
            }

            Query = new CustomQuery { Name = name, Sql = sql };
            DialogResult = true;
        }
    }
}
