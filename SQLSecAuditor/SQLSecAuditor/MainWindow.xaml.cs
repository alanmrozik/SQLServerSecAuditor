using SqlSecAuditor.Models;
using SqlSecAuditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
