using System.Windows;
using SqlSecAuditor.Models;
using SqlSecAuditor.ViewModels;

namespace SqlSecAuditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}