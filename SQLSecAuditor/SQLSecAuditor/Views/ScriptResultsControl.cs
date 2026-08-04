using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            // Pass vertical scroll to parent; Shift+wheel scrolls horizontally
            grid.PreviewMouseWheel += DataGrid_PreviewMouseWheel;

            // Show database name as sub-header if TableName is set
            if (!string.IsNullOrWhiteSpace(Results.TableName))
            {
                root.Children.Add(new TextBlock
                {
                    Text = Results.TableName,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 4, 0, 2)
                });
            }

            root.Children.Add(grid);

            if (Results.Rows.Count == 0)
            {
                root.Children.Add(new TextBlock { Text = "Brak wierszy.", Margin = new Thickness(6,4,0,0), Foreground = System.Windows.Media.Brushes.Gray });
            }

            Content = root;
        }

        private static void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Horizontal scroll: find the DataGrid's internal ScrollViewer
                var sv = FindVisualChild<ScrollViewer>(dataGrid);
                if (sv != null)
                {
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta / 3.0);
                    e.Handled = true;
                }
                return;
            }

            // Vertical scroll: bubble up to the parent ScrollViewer
            e.Handled = true;
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            dataGrid.RaiseEvent(args);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    return t;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
