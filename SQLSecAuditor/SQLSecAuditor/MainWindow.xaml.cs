using System.Windows;
using System.Windows.Controls;
using System.Data;
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
