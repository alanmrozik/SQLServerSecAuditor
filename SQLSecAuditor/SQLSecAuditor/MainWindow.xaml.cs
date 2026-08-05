using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Views;
using SqlSecAuditor.Models;
using SqlSecAuditor.ViewModels;

namespace SqlSecAuditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private async void GeneralInfoExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not Expander { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.LoadGeneralInfoAsync(instance);
        }

        private async void MaintenanceIntegrityRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunMaintenanceIntegrityAsync(instance);
        }

        private async void NetworkConnectivityRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunNetworkConnectivityAsync(instance);
        }

        private async void SurfaceAreaReductionRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunSurfaceAreaReductionAsync(instance);
        }

        private async void AuditingMonitoringRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunAuditingMonitoringAsync(instance);
        }

        private async void AuthenticationAccessControlRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunAuthenticationAccessControlAsync(instance);
        }

        private async void AuthorizationPermissionsRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunAuthorizationPermissionsAsync(instance);
        }

        private async void DatabaseSecurityRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunDatabaseSecurityAsync(instance);
        }

        private async void HighAvailabilityDisasterRecoveryRun_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            await viewModel.RunHighAvailabilityDisasterRecoveryAsync(instance);
        }

        private void SaveSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: SqlInstance instance })
            {
                return;
            }

            var dateStamp = DateTime.Now.ToString("dd_MM_yyyy");
            var safeServer = SanitizeFileNamePart(instance.ServerName);
            var safeDatabase = SanitizeFileNamePart(instance.DatabaseName);

            var dialog = new SaveFileDialog
            {
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json",
                FileName = $"Snapshot_{safeServer}_{safeDatabase}_{dateStamp}"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            ReportSnapshotService.SaveSnapshot(dialog.FileName, instance);
            MessageBox.Show(this, "Snapshot został zapisany.", "Snapshot", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CompareSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: SqlInstance instance })
            {
                return;
            }

            var openDialog = new OpenFileDialog
            {
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json"
            };

            if (openDialog.ShowDialog(this) != true)
            {
                return;
            }

            var other = ReportSnapshotService.LoadSnapshot(openDialog.FileName);
            var current = ReportSnapshotService.BuildSnapshot(instance);
            var diff = ReportSnapshotService.Compare(current, other);

            var dateStamp = DateTime.Now.ToString("dd_MM_yyyy");
            var safeServer = SanitizeFileNamePart(instance.ServerName);
            var safeDatabase = SanitizeFileNamePart(instance.DatabaseName);

            var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = $"SnapshotDiff_{safeServer}_{safeDatabase}_{dateStamp}.txt"
            };

            if (saveDialog.ShowDialog(this) == true)
            {
                System.IO.File.WriteAllText(saveDialog.FileName, diff);
            }

            var preview = diff.Length > 3000 ? diff.Substring(0, 3000) + "\n..." : diff;
            MessageBox.Show(this, preview, "Snapshot Compare", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: SqlInstance instance })
            {
                return;
            }

            var dateStamp = DateTime.Now.ToString("dd_MM_yyyy");
            var safeServer = SanitizeFileNamePart(instance.ServerName);
            var safeDatabase = SanitizeFileNamePart(instance.DatabaseName);

            var dialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Raport_Audytu_{safeServer}_{safeDatabase}_{dateStamp}.pdf"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            PdfReportExporter.Export(dialog.FileName, instance);
            MessageBox.Show(this, "Raport PDF został zapisany.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Public helper to display script results in the main UI.
        // The method will render the provided DataTable according to the rules:
        // - If table has exactly 2 columns and one column is named 'name' (case-insensitive):
        //   create a set of Expanders where the header is the 'name' value and the content is the second column value.
        // - Otherwise render the DataTable as a read-only DataGrid.
        public void ShowScriptResults(DataTable table)
        {
            // ResultsPanel is defined inside a DataTemplate, so we need to find the instantiated element in the visual tree.
            var panel = FindResultsPanel();
            if (panel == null)
                return;

            panel.Children.Clear();
            var control = new ScriptResultsControl { Results = table };
            panel.Children.Add(control);
        }

        private StackPanel? FindResultsPanel()
        {
            return FindChildByName<StackPanel>(this, "ResultsPanel");
        }

        private static string SanitizeFileNamePart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
            return sanitized.Trim();
        }

        private static T? FindChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement fe && fe.Name == name && child is T t)
                    return t;

                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
