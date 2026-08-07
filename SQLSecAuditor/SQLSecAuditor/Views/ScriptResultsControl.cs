using SqlSecAuditor.Infrastructure;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SqlSecAuditor.Views
{
    public class ScriptResultsControl : UserControl
    {
        private static readonly Brush GreenRowBrush = CreateBrush("#D4EFDF");
        private static readonly Brush RedRowBrush = CreateBrush("#FADBD8");
        private static readonly Brush YellowRowBrush = CreateBrush("#FCF3CF");

        public static readonly DependencyProperty ResultsProperty = DependencyProperty.Register(
            nameof(Results), typeof(DataTable), typeof(ScriptResultsControl), new PropertyMetadata(null, OnResultsChanged));

        public static readonly DependencyProperty ScriptNameProperty = DependencyProperty.Register(
            nameof(ScriptName), typeof(string), typeof(ScriptResultsControl), new PropertyMetadata(string.Empty, OnResultsChanged));

        public DataTable Results
        {
            get => (DataTable)GetValue(ResultsProperty);
            set => SetValue(ResultsProperty, value);
        }

        public string ScriptName
        {
            get => (string)GetValue(ScriptNameProperty);
            set => SetValue(ScriptNameProperty, value);
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

            grid.PreviewMouseWheel += DataGrid_PreviewMouseWheel;
            grid.LoadingRow += DataGrid_LoadingRow;

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
                root.Children.Add(new TextBlock { Text = "Brak wierszy.", Margin = new Thickness(6, 4, 0, 0), Foreground = Brushes.Gray });
            }

            Content = root;
        }

        private void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is not DataRowView rowView)
            {
                return;
            }

            var rowBrush = EvaluateRowBrush(rowView.Row);
            if (rowBrush == null)
            {
                e.Row.ClearValue(BackgroundProperty);
                return;
            }

            e.Row.Background = rowBrush;
        }

        private Brush? EvaluateRowBrush(DataRow row)
        {
            var evaluation = RowEvaluationService.Evaluate(ScriptName, Results, row);
            return evaluation switch
            {
                RowEvaluation.Green => GreenRowBrush,
                RowEvaluation.Red => RedRowBrush,
                RowEvaluation.Yellow => YellowRowBrush,
                _ => null
            };
        }


        private static Brush CreateBrush(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFrom(hex)!;
            brush.Freeze();
            return brush;
        }

        private static void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                var sv = FindVisualChild<ScrollViewer>(dataGrid);
                if (sv != null)
                {
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta / 3.0);
                    e.Handled = true;
                }
                return;
            }

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
