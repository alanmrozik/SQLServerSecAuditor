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
            DataContext = new MainViewModel(new ConnectionDialogService(this));
        }

        private void ShowAuditTab_Click(object sender, RoutedEventArgs e) => TopLevelTabControl.SelectedIndex = 0;

        private void ShowSnapshotsTab_Click(object sender, RoutedEventArgs e) => TopLevelTabControl.SelectedIndex = 1;

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
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunMaintenanceIntegrityAsync(instance));

        private async void NetworkConnectivityRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunNetworkConnectivityAsync(instance));

        private async void SurfaceAreaReductionRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunSurfaceAreaReductionAsync(instance));

        private async void AuditingMonitoringRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunAuditingMonitoringAsync(instance));

        private async void AuthenticationAccessControlRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunAuthenticationAccessControlAsync(instance));

        private async void AuthorizationPermissionsRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunAuthorizationPermissionsAsync(instance));

        private async void DatabaseSecurityRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunDatabaseSecurityAsync(instance));

        private async void HighAvailabilityDisasterRecoveryRun_Click(object sender, RoutedEventArgs e)
            => await RunCategoryAsync(sender, e, static (viewModel, instance) => viewModel.RunHighAvailabilityDisasterRecoveryAsync(instance));

        private async Task RunCategoryAsync(object sender, RoutedEventArgs e, Func<MainViewModel, SqlInstance, Task> runCategory)
        {
            e.Handled = true;
            if (sender is Button { DataContext: SqlInstance instance } && DataContext is MainViewModel viewModel)
            {
                await runCategory(viewModel, instance);
            }
        }

        private void AddCustomQuery_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) return;
            var dialog = new CustomQueryDialog { Owner = this };
            if (dialog.ShowDialog() == true && dialog.Query is not null)
                viewModel.AddCustomQuery(dialog.Query);
        }

        private async void RunCustomQueries_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { DataContext: SqlInstance instance } || DataContext is not MainViewModel viewModel) return;
            await viewModel.RunCustomQueriesAsync(instance);
        }

        private async void RunCustomQuery_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { DataContext: CustomQuery query, Tag: SqlInstance instance } || DataContext is not MainViewModel viewModel) return;
            await viewModel.RunCustomQueryAsync(instance, query);
        }

        private void DeleteCustomQuery_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { DataContext: CustomQuery query } || DataContext is not MainViewModel viewModel) return;
            viewModel.DeleteCustomQuery(query);
        }

        private void DeleteSavedConnection_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is not Button { DataContext: SavedConnection connection } || DataContext is not MainViewModel viewModel)
            {
                return;
            }

            var answer = MessageBox.Show(
                this,
                $"Delete the saved connection '{connection.DisplayLabel}'?",
                "Delete connection",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                viewModel.DeleteSavedConnectionCommand.Execute(connection);
            }
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
                new RunCategoryOption { Key = "maintenance", Name = "Maintenance and Integrity" },
                new RunCategoryOption { Key = "network", Name = "Network and Connectivity" },
                new RunCategoryOption { Key = "surface", Name = "Surface Area Reduction" },
                new RunCategoryOption { Key = "auditing", Name = "Auditing and Monitoring" },
                new RunCategoryOption { Key = "authentication", Name = "Authentication and Access Control" },
                new RunCategoryOption { Key = "authorization", Name = "Authorization and Permissions" },
                new RunCategoryOption { Key = "database", Name = "Database Security" },
                new RunCategoryOption { Key = "hadr", Name = "High Availability and Disaster Recovery" }
            };

            options.Add(new RunCategoryOption { Key = "custom", Name = "Custom Queries" });

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
                    case "custom":
                        await viewModel.RunCustomQueriesAsync(instance);
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

            var dateStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
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
            MessageBox.Show(this, "Snapshot saved successfully.", "Snapshot", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CompareSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            var instance = viewModel.SelectedInstance;
            if (instance is null)
            {
                MessageBox.Show(this, "Select an instance from the list on the left in the Audit tab first.", "Snapshots", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var openDialog = new OpenFileDialog
            {
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json"
            };

            if (openDialog.ShowDialog(this) != true)
                return;

            try
            {
                var other = ReportSnapshotService.LoadSnapshot(openDialog.FileName);
                var current = ReportSnapshotService.BuildSnapshot(instance);
                var rows = ReportSnapshotService.CompareRows(current, other);

                viewModel.SnapshotComparisonRows.Clear();
                foreach (var row in rows)
                    viewModel.SnapshotComparisonRows.Add(row);

                viewModel.SnapshotComparisonSummary = rows.Count == 0
                    ? "No differences found."
                    : ReportSnapshotService.BuildComparisonSummary(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not compare the snapshot:\n\n{ex.Message}", "Snapshots", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompareTwoSnapshots_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            var dialogA = new OpenFileDialog
            {
                Title = "Select the first snapshot (Snapshot A)",
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json"
            };

            if (dialogA.ShowDialog(this) != true)
                return;

            var dialogB = new OpenFileDialog
            {
                Title = "Select the second snapshot (Snapshot B)",
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json"
            };

            if (dialogB.ShowDialog(this) != true)
                return;

            try
            {
                var snapshotA = ReportSnapshotService.LoadSnapshot(dialogA.FileName);
                var snapshotB = ReportSnapshotService.LoadSnapshot(dialogB.FileName);
                var rows = ReportSnapshotService.CompareRows(snapshotA, snapshotB);

                viewModel.SnapshotComparisonRows.Clear();
                foreach (var row in rows)
                    viewModel.SnapshotComparisonRows.Add(row);

                viewModel.SnapshotComparisonSummary = rows.Count == 0
                    ? "No differences found between the snapshots."
                    : $"[A: {System.IO.Path.GetFileName(dialogA.FileName)}  vs  B: {System.IO.Path.GetFileName(dialogB.FileName)}]  " + ReportSnapshotService.BuildComparisonSummary(rows);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not compare the snapshots:\n\n{ex.Message}", "Snapshots", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: SqlInstance instance })
            {
                return;
            }

            var categoryOptions = GetExecutedCategoryOptions(instance);
            var selectorDialog = new PdfExportCategoryDialog(categoryOptions)
            {
                Owner = this
            };

            if (selectorDialog.ShowDialog() != true)
            {
                return;
            }

            var selectedKeys = selectorDialog.SelectedCategoryKeys;
            if (selectorDialog.ReportType != PdfReportType.Scoring && selectedKeys.Count == 0)
            {
                return;
            }

            var dateStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
            var safeServer = SanitizeFileNamePart(instance.ServerName);
            var safeDatabase = SanitizeFileNamePart(instance.DatabaseName);

            var reportName = selectorDialog.ReportType switch
            {
                PdfReportType.Audit => "Security_Audit_Report",
                PdfReportType.Scoring => "Security_Scoring_Report",
                _ => "Security_Audit_and_Scoring_Report"
            };

            var dialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{reportName}_{safeServer}_{safeDatabase}_{dateStamp}"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            PdfReportExporter.Export(dialog.FileName, instance, selectorDialog.ReportType, selectedKeys);
            MessageBox.Show(this, "PDF report saved successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSnapshotViewer_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            var openDialog = new OpenFileDialog
            {
                Filter = "Snapshot files (*.sqlsa.snapshot.json)|*.sqlsa.snapshot.json|JSON files (*.json)|*.json"
            };

            if (openDialog.ShowDialog(this) != true)
                return;

            try
            {
                var snapshot = ReportSnapshotService.LoadSnapshot(openDialog.FileName);
                var categories = ReportSnapshotService.BuildViewerCategories(snapshot);

                viewModel.SnapshotViewerCategories.Clear();
                foreach (var category in categories)
                    viewModel.SnapshotViewerCategories.Add(category);

                viewModel.SnapshotViewerSummary = ReportSnapshotService.BuildViewerSummary(snapshot);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not load the snapshot:\n\n{ex.Message}", "Snapshot viewer", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private static ObservableCollection<PdfExportCategoryOption> GetExecutedCategoryOptions(SqlInstance instance)
        {
            var options = new ObservableCollection<PdfExportCategoryOption>();

            if (instance.IsGeneralInfoLoaded)
            {
                options.Add(new PdfExportCategoryOption { Key = "general", Name = "General Information" });
            }

            AddIfExecuted(options, "maintenance_integrity", "Maintenance and Integrity", instance.MaintenanceIntegrityResults.Count > 0);
            AddIfExecuted(options, "network_connectivity", "Network and Connectivity", instance.NetworkConnectivityResults.Count > 0);
            AddIfExecuted(options, "surface_area_reduction", "Surface Area Reduction", instance.SurfaceAreaReductionResults.Count > 0);
            AddIfExecuted(options, "auditing_monitoring", "Auditing and Monitoring", instance.AuditingMonitoringResults.Count > 0);
            AddIfExecuted(options, "authentication_access_control", "Authentication and Access Control", instance.AuthenticationAccessControlResults.Count > 0);
            AddIfExecuted(options, "authorization_permissions", "Authorization and Permissions", instance.AuthorizationPermissionsResults.Count > 0);
            AddIfExecuted(options, "database_security", "Database Security", instance.DatabaseSecurityResults.Count > 0);
            AddIfExecuted(options, "high_availability_disaster_recovery", "High Availability and Disaster Recovery", instance.HighAvailabilityDisasterRecoveryResults.Count > 0);

            AddIfExecuted(options, "custom_queries", "Custom Queries", instance.CustomQueryResults.Count > 0);

            return options;
        }

        private static void AddIfExecuted(ObservableCollection<PdfExportCategoryOption> options, string key, string name, bool isExecuted)
        {
            if (!isExecuted)
            {
                return;
            }

            options.Add(new PdfExportCategoryOption
            {
                Key = key,
                Name = name,
                IsSelected = true
            });
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

        private void CopyFixScript_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not ScriptExecutionResult result)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(result.FixScript))
            {
                return;
            }

            try
            {
                Clipboard.SetText(result.FixScript);
                MessageBox.Show("Remediation script copied to the clipboard.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy to the clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
