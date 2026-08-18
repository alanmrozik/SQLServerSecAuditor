using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SqlSecAuditor.Views
{
    public sealed class DangerousQueryWarningDialog : Window
    {
        public DangerousQueryWarningDialog(IReadOnlyList<string> risks)
        {
            Title = "Ostrzeżenie bezpieczeństwa";
            Width = 560;
            SizeToContent = SizeToContent.Height;
            MinHeight = 270;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = Application.Current.TryFindResource("AppWindowBackgroundBrush") as Brush ?? Brushes.White;

            var shell = new Border
            {
                Background = Application.Current.TryFindResource("AppPanelBackgroundBrush") as Brush ?? Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xF5, 0xA6, 0x23)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(18),
                Padding = new Thickness(22)
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "⚠ Potencjalnie niebezpieczne zapytanie",
                FontSize = 19,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)),
                Margin = new Thickness(0, 0, 0, 10)
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Zapytanie zawiera elementy, które mogą modyfikować dane, strukturę bazy, uprawnienia lub uruchamiać polecenia administracyjne.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var riskList = new ItemsControl { ItemsSource = risks, Margin = new Thickness(8, 0, 0, 16) };
            riskList.ItemTemplate = new DataTemplate
            {
                VisualTree = CreateRiskTemplate()
            };
            panel.Children.Add(riskList);

            panel.Children.Add(new TextBlock
            {
                Text = "Aplikacja zapisze zapytanie i może je później wykonać z uprawnieniami aktualnie użytego konta SQL Server.",
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 18)
            });

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var cancel = new Button { Content = "Anuluj", IsCancel = true, Margin = new Thickness(0, 0, 8, 0) };
            var confirm = new Button
            {
                Content = "Wiem, co robię",
                IsDefault = true,
                Background = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C))
            };
            confirm.Click += (_, _) => DialogResult = true;
            buttons.Children.Add(cancel);
            buttons.Children.Add(confirm);
            panel.Children.Add(buttons);

            shell.Child = panel;
            Content = shell;
        }

        private static FrameworkElementFactory CreateRiskTemplate()
        {
            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextProperty, "• ");
            text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding { StringFormat = "• {0}" });
            text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            text.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 2, 0, 2));
            return text;
        }
    }
}
