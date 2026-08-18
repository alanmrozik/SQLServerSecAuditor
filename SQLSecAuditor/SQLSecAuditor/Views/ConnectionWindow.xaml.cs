using Microsoft.Data.SqlClient;
using SqlSecAuditor.Infrastructure;
using SqlSecAuditor.Models;
using SqlSecAuditor.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace SqlSecAuditor.Views
{
    public class ConnectionWindow : Window
    {
        private FrameworkElement? _interactiveRoot;
        private ComboBox? _recentCombo;

        public ConnectionWindow()
        {
            Title = "New Connection";
            Height = 600;
            Width = 760;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = GetBrush("AppWindowBackgroundBrush", Color.FromRgb(0xEC, 0xF0, 0xF1));
            DataContext = new ConnectionWindowViewModel();

            Content = BuildLayout();
        }

        public SqlInstance? ResultInstance { get; private set; }

        private UIElement BuildLayout()
        {
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            root.Children.Add(CreateTitleBar());
            root.Children.Add(CreateBody());
            Grid.SetRow(root.Children[1], 1);

            return root;
        }

        private UIElement CreateTitleBar()
        {
            var titleBar = new Grid
            {
                Height = 35,
                Background = GetBrush("AppHeaderBackgroundBrush", Color.FromRgb(0x1E, 0x2B, 0x3C))
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var dragArea = new Grid
            {
                Background = Brushes.Transparent
            };
            dragArea.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;

            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            titlePanel.Children.Add(new TextBlock
            {
                Text = "SQLServerSecAuditor",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            });
            dragArea.Children.Add(titlePanel);
            titleBar.Children.Add(dragArea);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            buttons.Children.Add(CreateChromeButton("—", Minimize_Click, useSharedStyle: true, fontSize: 14, width: 45));
            buttons.Children.Add(CreateChromeButton("X", Close_Click, useSharedStyle: false, fontSize: 14, width: 45, hoverBrushFallback: Color.FromRgb(0xE7, 0x4C, 0x3C)));

            titleBar.Children.Add(buttons);
            Grid.SetColumn(buttons, 1);

            return titleBar;
        }

        private Border CreateBody()
        {
            var shell = new Border
            {
                Background = GetBrush("AppWindowBackgroundBrush", Color.FromRgb(0xEC, 0xF0, 0xF1)),
                Padding = new Thickness(15)
            };

            var surface = new Border
            {
                Background = GetBrush("AppPanelBackgroundBrush", Colors.White),
                BorderBrush = GetBrush("AppAccentBorderBrush", Color.FromRgb(0xBD, 0xC3, 0xC7)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20)
            };

            _interactiveRoot = surface;

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            content.Children.Add(new TextBlock
            {
                Text = "Add a new SQL Server connection",
                Style = (Style)Application.Current.FindResource("AppPageTitleStyle")
            });

            var recentRow = CreateRecentConnectionsRow();
            content.Children.Add(recentRow);
            Grid.SetRow(content.Children[1], 1);

            content.Children.Add(CreateConnectionGroup());
            Grid.SetRow(content.Children[2], 2);

            content.Children.Add(CreateAuthenticationGroup());
            Grid.SetRow(content.Children[3], 3);

            content.Children.Add(CreateButtonsPanel());
            Grid.SetRow(content.Children[4], 4);

            surface.Child = content;
            shell.Child = surface;
            return shell;
        }

        private UIElement CreateRecentConnectionsRow()
        {
            var recent = RecentConnectionsStore.Load();

            var panel = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            panel.Children.Add(new TextBlock
            {
                Text = "Recent connections:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            _recentCombo = new ComboBox
            {
                IsEditable = false,
                DisplayMemberPath = nameof(SavedConnection.DisplayLabel),
                IsEnabled = recent.Count > 0
            };

            if (recent.Count > 0)
            {
                foreach (var c in recent)
                    _recentCombo.Items.Add(c);
                _recentCombo.SelectedIndex = -1;
            }
            else
            {
                _recentCombo.Items.Add("(Brak zapisanych połączeń)");
                _recentCombo.SelectedIndex = 0;
            }

            _recentCombo.SelectionChanged += RecentConnectionSelected;

            Grid.SetColumn(_recentCombo, 1);
            panel.Children.Add(_recentCombo);

            return panel;
        }

        private void RecentConnectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_recentCombo?.SelectedItem is not SavedConnection saved)
                return;
            if (DataContext is not ConnectionWindowViewModel vm)
                return;

            vm.ServerName = saved.ServerName;
            vm.Port = saved.Port;
            vm.DatabaseName = saved.DatabaseName;
            vm.UseWindowsAuthentication = saved.UseWindowsAuthentication;
            vm.UseSqlAuthentication = !saved.UseWindowsAuthentication;
            vm.SqlUserName = saved.SqlUserName;
            vm.EncryptConnection = saved.EncryptConnection;
            vm.TrustServerCertificate = saved.TrustServerCertificate;
        }

        private GroupBox CreateConnectionGroup()
        {
            var group = new GroupBox
            {
                Header = "Connection",
                Style = (Style)Application.Current.FindResource("AppGroupBoxStyle")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(CreateLabel("Server name", 0, 0, 0, 12, 8));
            grid.Children.Add(CreateBoundTextBox(nameof(ConnectionWindowViewModel.ServerName), 1, 0, 0, 8));

            grid.Children.Add(CreateLabel("Port", 3, 0, 0, 12, 8));
            grid.Children.Add(CreateBoundTextBox(nameof(ConnectionWindowViewModel.Port), 4, 0, 0, 8));

            grid.Children.Add(CreateLabel("Database", 0, 1, 0, 12, 8));
            grid.Children.Add(CreateBoundTextBox(nameof(ConnectionWindowViewModel.DatabaseName), 1, 1, 4, 8));

            grid.Children.Add(CreateLabel("Connection options", 0, 2, 0, 12, 0));
            var optionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var encryptBox = new CheckBox { Content = "Encrypt connection", Margin = new Thickness(0, 0, 24, 0) };
            encryptBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(ConnectionWindowViewModel.EncryptConnection)) { Mode = BindingMode.TwoWay });
            var trustBox = new CheckBox { Content = "Trust server certificate" };
            trustBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(ConnectionWindowViewModel.TrustServerCertificate)) { Mode = BindingMode.TwoWay });
            optionsPanel.Children.Add(encryptBox);
            optionsPanel.Children.Add(trustBox);
            grid.Children.Add(optionsPanel);
            Grid.SetColumn(optionsPanel, 1);
            Grid.SetColumnSpan(optionsPanel, 4);
            Grid.SetRow(optionsPanel, 2);

            group.Content = grid;
            return group;
        }

        private GroupBox CreateAuthenticationGroup()
        {
            var group = new GroupBox
            {
                Header = "Authentication",
                Style = (Style)Application.Current.FindResource("AppGroupBoxStyle")
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var authPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var windowsRadio = new RadioButton
            {
                Content = "Windows authentication",
                GroupName = "AuthenticationMode",
                Margin = new Thickness(0, 0, 24, 0)
            };
            windowsRadio.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(ConnectionWindowViewModel.UseWindowsAuthentication)) { Mode = BindingMode.TwoWay });
            var sqlRadio = new RadioButton
            {
                Content = "SQL Server authentication",
                GroupName = "AuthenticationMode"
            };
            sqlRadio.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(ConnectionWindowViewModel.UseSqlAuthentication)) { Mode = BindingMode.TwoWay });
            authPanel.Children.Add(windowsRadio);
            authPanel.Children.Add(sqlRadio);
            grid.Children.Add(authPanel);

            var infoText = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 10),
                Foreground = GetBrush("AppMutedTextBrush", Color.FromRgb(0x55, 0x66, 0x77)),
                TextWrapping = TextWrapping.Wrap,
                Text = "Windows authentication uses the current Windows account. SQL authentication requires a user name and password."
            };
            grid.Children.Add(infoText);
            Grid.SetRow(infoText, 1);

            var credentialsGrid = new Grid();
            credentialsGrid.SetBinding(IsEnabledProperty, new Binding(nameof(ConnectionWindowViewModel.UseSqlAuthentication)));
            credentialsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            credentialsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            credentialsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            credentialsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            credentialsGrid.Children.Add(CreateLabel("User name", 0, 0, 0, 12, 8));
            credentialsGrid.Children.Add(CreateBoundTextBox(nameof(ConnectionWindowViewModel.SqlUserName), 1, 0, 0, 8));

            credentialsGrid.Children.Add(CreateLabel("Password", 0, 1, 0, 12, 0));
            var passwordBox = new PasswordBox
            {
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            credentialsGrid.Children.Add(passwordBox);
            Grid.SetColumn(passwordBox, 1);
            Grid.SetRow(passwordBox, 1);

            grid.Children.Add(credentialsGrid);
            Grid.SetRow(credentialsGrid, 2);

            group.Content = grid;
            return group;
        }

        private StackPanel CreateButtonsPanel()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Margin = new Thickness(0, 0, 12, 0)
            };
            cancelButton.Click += Close_Click;

            var addButton = new Button
            {
                Content = "Add connection",
                Width = 130
            };
            addButton.Click += Confirm_Click;

            panel.Children.Add(cancelButton);
            panel.Children.Add(addButton);
            return panel;
        }

        private Button CreateChromeButton(string content, RoutedEventHandler clickHandler, bool useSharedStyle, double fontSize, double width, Color? hoverBrushFallback = null)
        {
            var button = new Button
            {
                Content = content,
                Width = width,
                FontSize = fontSize,
                FontFamily = new FontFamily("Consolas"),
                ClickMode = ClickMode.Release,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White
            };

            if (useSharedStyle)
            {
                button.Style = (Style)Application.Current.FindResource("AppChromeButtonStyle");
            }
            else
            {
                var hoverBrush = hoverBrushFallback.HasValue
                    ? new SolidColorBrush(hoverBrushFallback.Value)
                    : GetBrush("AppHeaderHoverBrush", Color.FromRgb(0x34, 0x49, 0x5E));

                var style = new Style(typeof(Button));
                style.Setters.Add(new Setter(Control.TemplateProperty, CreateChromeTemplate()));
                style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
                style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
                style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));

                var trigger = new Trigger
                {
                    Property = UIElement.IsMouseOverProperty,
                    Value = true
                };
                trigger.Setters.Add(new Setter(Control.BackgroundProperty, hoverBrush));
                style.Triggers.Add(trigger);
                button.Style = style;
            }

            button.Click += clickHandler;
            return button;
        }

        private static ControlTemplate CreateChromeTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentFactory);
            template.VisualTree = borderFactory;
            return template;
        }

        private static TextBlock CreateLabel(string text, int column, int row, double left, double right, double bottom)
        {
            var label = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(left, 0, right, bottom)
            };
            Grid.SetColumn(label, column);
            Grid.SetRow(label, row);
            return label;
        }

        private static TextBox CreateBoundTextBox(string path, int column, int row, int columnSpan, double bottom)
        {
            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, bottom),
                Padding = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            textBox.SetBinding(TextBox.TextProperty, new Binding(path) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            Grid.SetColumn(textBox, column);
            Grid.SetRow(textBox, row);
            if (columnSpan > 1)
            {
                Grid.SetColumnSpan(textBox, columnSpan);
            }
            return textBox;
        }

        private static Brush GetBrush(string resourceKey, Color fallbackColor)
        {
            return Application.Current?.TryFindResource(resourceKey) as Brush ?? new SolidColorBrush(fallbackColor);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConnectionWindowViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ConnectionWindowViewModel viewModel)
            {
                return;
            }

            if (viewModel.UseSqlAuthentication && string.IsNullOrWhiteSpace(viewModel.SqlUserName))
            {
                MessageBox.Show(this, "Podaj nazwę użytkownika dla logowania SQL.", "Connection test", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.UseSqlAuthentication && string.IsNullOrWhiteSpace(viewModel.Password))
            {
                MessageBox.Show(this, "Podaj hasło dla logowania SQL.", "Connection test", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _interactiveRoot!.IsEnabled = false;
            Cursor = Cursors.Wait;

            try
            {
                var dataSource = string.IsNullOrWhiteSpace(viewModel.Port)
                    ? viewModel.ServerName
                    : $"{viewModel.ServerName},{viewModel.Port}";

                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = dataSource,
                    InitialCatalog = string.IsNullOrWhiteSpace(viewModel.DatabaseName) ? "master" : viewModel.DatabaseName,
                    Encrypt = viewModel.EncryptConnection,
                    TrustServerCertificate = viewModel.TrustServerCertificate,
                    ConnectTimeout = 5
                };

                if (viewModel.UseSqlAuthentication)
                {
                    builder.IntegratedSecurity = false;
                    builder.UserID = viewModel.SqlUserName;
                    builder.Password = viewModel.Password;
                }
                else
                {
                    builder.IntegratedSecurity = true;
                }

                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT CAST(@@SERVERNAME AS nvarchar(128))";
                var serverNameResult = await command.ExecuteScalarAsync();
                var resolvedServerName = serverNameResult as string;

                if (string.IsNullOrWhiteSpace(resolvedServerName))
                {
                    resolvedServerName = viewModel.ServerName;
                }

                var databaseName = string.IsNullOrWhiteSpace(viewModel.DatabaseName)
                    ? "master"
                    : viewModel.DatabaseName;

                ResultInstance = new SqlInstance
                {
                    ServerName = resolvedServerName,
                    DatabaseName = databaseName,
                    ConnectionString = builder.ConnectionString
                };

                RecentConnectionsStore.Save(new SavedConnection
                {
                    ServerName = viewModel.ServerName,
                    Port = viewModel.Port,
                    DatabaseName = viewModel.DatabaseName,
                    UseWindowsAuthentication = viewModel.UseWindowsAuthentication,
                    SqlUserName = viewModel.SqlUserName,
                    EncryptConnection = viewModel.EncryptConnection,
                    TrustServerCertificate = viewModel.TrustServerCertificate
                });

                MessageBox.Show(this, "Połączenie zostało nawiązane.", "Connection test", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Nie udało się nawiązać połączenia:\n\n{ex.Message}", "Connection test", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                _interactiveRoot.IsEnabled = true;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
