using System;
using System.Data;
using System.Globalization;
using System.Linq;
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
            var normalizedScript = NormalizeToken(ScriptName);
            var rowText = string.Join(" | ", row.ItemArray.Select(ToText)).ToLowerInvariant();

            if (normalizedScript.Contains("encryptionchecks") || normalizedScript.Contains("generalinfoaboutserver"))
            {
                return null;
            }

            if (normalizedScript.Contains("builtinlogins")
                || normalizedScript.Contains("expirationforsqlloginsysadmins")
                || normalizedScript.Contains("sysadminlogins")
                || normalizedScript.Contains("guestpermissions")
                || normalizedScript.Contains("permissionsondblevelpoprawiony")
                || normalizedScript.Contains("serviceaccounts")
                || normalizedScript.Contains("sqlserverport"))
            {
                return YellowRowBrush;
            }

            if (normalizedScript.Contains("orphanedusers")
                || normalizedScript.Contains("publicroleisnotgrantedtoproxies")
                || normalizedScript.Contains("autoclose")
                || normalizedScript.Contains("clrenabled"))
            {
                return RedRowBrush;
            }

            if (normalizedScript.Contains("defaulttraceenabled"))
            {
                return rowText.Contains("enabled") ? GreenRowBrush : rowText.Contains("disabled") ? RedRowBrush : null;
            }

            if (normalizedScript.Contains("loginauditing"))
            {
                return rowText.Contains("failed") && rowText.Contains("login") ? GreenRowBrush : RedRowBrush;
            }

            if (normalizedScript.Contains("issadisabled")
                || normalizedScript.Contains("scanforstartupprocs")
                || normalizedScript.Contains("crossdbownershipchaining")
                || normalizedScript.Contains("trustworthyofdatabase")
                || normalizedScript.Contains("adhocdistributedqueries")
                || normalizedScript.Contains("clrstrictsecurity")
                || normalizedScript.Contains("databasemailxps")
                || normalizedScript.Contains("oleautomationprocedures")
                || normalizedScript.Contains("remoteacces")
                || normalizedScript.Contains("remoteadminconnections"))
            {
                return rowText.Contains("disabled") ? GreenRowBrush : rowText.Contains("enabled") ? RedRowBrush : null;
            }

            if (normalizedScript.Contains("passwordpolicyforsqllogins"))
            {
                if (rowText.Contains("not checked") || rowText.Contains("n/a") || rowText.Contains("0"))
                {
                    return RedRowBrush;
                }

                if (rowText.Contains("checked") || rowText.Contains("1") || rowText.Contains("true"))
                {
                    return GreenRowBrush;
                }

                return null;
            }

            if (normalizedScript.Contains("hideinstance"))
            {
                if (HasExactValue(row, "1"))
                {
                    return GreenRowBrush;
                }

                if (HasExactValue(row, "0"))
                {
                    return RedRowBrush;
                }

                return null;
            }

            if (normalizedScript.Contains("ifconnectionusekerberos"))
            {
                return rowText.Contains("kerberos") ? GreenRowBrush : rowText.Contains("ntlm") ? RedRowBrush : null;
            }

            if (normalizedScript.Contains("isagenabled")
                || normalizedScript.Contains("isclustered")
                || normalizedScript.Contains("islogshipped")
                || normalizedScript.Contains("ismirrored")
                || normalizedScript.Contains("isreplicated"))
            {
                if (HasExactValue(row, "1"))
                {
                    return GreenRowBrush;
                }

                if (HasExactValue(row, "0"))
                {
                    var hasAnyEnabled = Results.ExtendedProperties["HaDrAnyEnabled"] as bool? == true;
                    return hasAnyEnabled ? YellowRowBrush : RedRowBrush;
                }

                return null;
            }

            if (normalizedScript.Contains("lastbackupdates"))
            {
                if (TryFindDate(row, out var dt))
                {
                    return dt >= DateTime.Now.AddMonths(-1) ? GreenRowBrush : RedRowBrush;
                }

                return RedRowBrush;
            }

            if (normalizedScript.Contains("lastknowgoodcheckdb"))
            {
                if (TryFindDate(row, out var dt))
                {
                    if (dt.Year == 1900)
                    {
                        return RedRowBrush;
                    }

                    if (dt >= DateTime.Now.AddMonths(-1))
                    {
                        return GreenRowBrush;
                    }
                }

                return null;
            }

            return null;
        }

        private static bool HasExactValue(DataRow row, string expected)
        {
            return row.ItemArray.Select(ToText).Any(v => string.Equals(v, expected, StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryFindDate(DataRow row, out DateTime date)
        {
            foreach (var value in row.ItemArray)
            {
                if (value is DateTime dt)
                {
                    date = dt;
                    return true;
                }

                var text = ToText(value);
                if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt)
                    || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    date = dt;
                    return true;
                }
            }

            date = default;
            return false;
        }

        private static string ToText(object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        }

        private static string NormalizeToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
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
