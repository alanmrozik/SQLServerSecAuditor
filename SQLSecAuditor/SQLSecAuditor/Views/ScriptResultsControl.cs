using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SqlSecAuditor.Views
{
    public class ScriptResultsControl : UserControl
    {
        public static readonly DependencyProperty ResultsProperty = DependencyProperty.Register(
            nameof(Results), typeof(DataTable), typeof(ScriptResultsControl), new PropertyMetadata(null, OnResultsChanged));

        public DataTable Results
        {
            get => (DataTable)GetValue(ResultsProperty);
            set => SetValue(ResultsProperty, value);
        }

        private static void OnResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScriptResultsControl ctrl)
            {
                ctrl.RenderResults();
            }
        }

        private void RenderResults()
        {
            var root = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

            // Always present results as a read-only DataGrid. If there are no rows, show an explicit message but still render the table (headers).
            if (Results == null || Results.Columns.Count == 0)
            {
                root.Children.Add(new TextBlock { Text = "Brak wyników.", Margin = new Thickness(10) });
                Content = root;
                return;
            }

            var grid = new DataGrid
            {
                AutoGenerateColumns = true,
                IsReadOnly = true,
                ItemsSource = Results.DefaultView,
                Margin = new Thickness(0, 4, 0, 4)
            };

            root.Children.Add(grid);

            if (Results.Rows.Count == 0)
            {
                root.Children.Add(new TextBlock { Text = "Brak wierszy.", Margin = new Thickness(6,4,0,0), Foreground = System.Windows.Media.Brushes.Gray });
            }

            Content = root;
        }
    }
}
