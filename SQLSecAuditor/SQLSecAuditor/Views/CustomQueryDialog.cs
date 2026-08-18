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

            var panel = new Grid { Margin = new Thickness(16) };
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            panel.Children.Add(new TextBlock { Text = "Nazwa zapytania", Margin = new Thickness(0, 0, 0, 5) });
            _nameBox = new TextBox { Margin = new Thickness(0, 0, 0, 14) };
            Grid.SetRow(_nameBox, 1);
            panel.Children.Add(_nameBox);

            var sqlLabel = new TextBlock { Text = "SQL", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(sqlLabel, 2);
            panel.Children.Add(sqlLabel);
            _sqlBox = new TextBox { AcceptsReturn = true, AcceptsTab = true, FontFamily = new System.Windows.Media.FontFamily("Consolas"), VerticalScrollBarVisibility = ScrollBarVisibility.Auto, TextWrapping = TextWrapping.NoWrap, Margin = new Thickness(0, 24, 0, 14) };
            Grid.SetRow(_sqlBox, 2);
            panel.Children.Add(_sqlBox);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "Anuluj", IsCancel = true, Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 0) };
            var save = new Button { Content = "Dodaj", IsDefault = true, Padding = new Thickness(12, 5, 12, 5) };
            save.Click += Save_Click;
            buttons.Children.Add(cancel);
            buttons.Children.Add(save);
            Grid.SetRow(buttons, 3);
            panel.Children.Add(buttons);

            Content = panel;
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

            Query = new CustomQuery { Name = name, Sql = sql };
            DialogResult = true;
        }
    }
}
