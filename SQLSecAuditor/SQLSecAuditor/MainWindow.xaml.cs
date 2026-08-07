using Microsoft.Win32;
using System.Collections.ObjectModel;
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

        private async void RunMultipleCategories_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: SqlInstance instance })
            {
                return;
            }

            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            var options = new ObservableCollection<RunCategoryOption>
            {
                new RunCategoryOption { Key = "maintenance", Name = "Utrzymanie i integralność" },
                new RunCategoryOption { Key = "network", Name = "Sieć i łączność" },
                new RunCategoryOption { Key = "surface", Name = "Redukcja powierzchni ataku" },
                new RunCategoryOption { Key = "auditing", Name = "Audyt i monitoring" },
                new RunCategoryOption { Key = "authentication", Name = "Uwierzytelnianie i kontrola dostępu" },
                new RunCategoryOption { Key = "authorization", Name = "Autoryzacja i uprawnienia" },
                new RunCategoryOption { Key = "database", Name = "Bezpieczeństwo baz danych" },
                new RunCategoryOption { Key = "hadr", Name = "Wysoka dostępność i odzyskiwanie po awarii" }
            };

            var dialog = new RunMultipleCategoriesDialog(options)
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var selected = dialog.SelectedCategoryKeys;
            foreach (var key in selected)
            {
                switch (key)
                {
                    case "maintenance":
                        await viewModel.RunMaintenanceIntegrityAsync(instance);
                        break;
                    case "network":
                        await viewModel.RunNetworkConnectivityAsync(instance);
                        break;
                    case "surface":
                        await viewModel.RunSurfaceAreaReductionAsync(instance);
                        break;
                    case "auditing":
                        await viewModel.RunAuditingMonitoringAsync(instance);
                        break;
                    case "authentication":
                        await viewModel.RunAuthenticationAccessControlAsync(instance);
                        break;
                    case "authorization":
                        await viewModel.RunAuthorizationPermissionsAsync(instance);
                        break;
                    case "database":
                        await viewModel.RunDatabaseSecurityAsync(instance);
                        break;
                    case "hadr":
                        await viewModel.RunHighAvailabilityDisasterRecoveryAsync(instance);
                        break;
                }
            }
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

            try
            {
                var other = ReportSnapshotService.LoadSnapshot(openDialog.FileName);
                var current = ReportSnapshotService.BuildSnapshot(instance);
                var rows = ReportSnapshotService.CompareRows(current, other);

                instance.SnapshotComparisonRows.Clear();
                foreach (var row in rows)
                {
                    instance.SnapshotComparisonRows.Add(row);
                }

                instance.SnapshotComparisonSummary = ReportSnapshotService.BuildComparisonSummary(rows);

                if (rows.Count == 0)
                {
                    instance.SnapshotComparisonSummary = "Brak różnic.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Nie udało się porównać snapshotu:\n\n{ex.Message}", "Snapshots", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                FileName = $"Raport_Audytu_{safeServer}_{safeDatabase}_{dateStamp}"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            PdfReportExporter.Export(dialog.FileName, instance);
            MessageBox.Show(this, "Raport PDF został zapisany.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSnapshotViewer_Click(object sender, RoutedEventArgs e)
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

            try
            {
                var snapshot = ReportSnapshotService.LoadSnapshot(openDialog.FileName);
                var categories = ReportSnapshotService.BuildViewerCategories(snapshot);

                instance.SnapshotViewerCategories.Clear();
                foreach (var category in categories)
                {
                    instance.SnapshotViewerCategories.Add(category);
                }

                instance.SnapshotViewerSummary = ReportSnapshotService.BuildViewerSummary(snapshot);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Nie udało się wczytać snapshotu:\n\n{ex.Message}", "Snapshot viewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
