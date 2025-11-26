using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkOrderManagementSystem.Services;
using WorkOrderManagementSystem.Views;

namespace WorkOrderManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AuthenticationService _authService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly WorkOrderService _workOrderService;
        private readonly UserService _userService;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            var mongoService = new MongoDbService();
            _authService = App.AuthService;
            _customerService = new CustomerService(mongoService);
            _deviceService = new DeviceService(mongoService);
            _workOrderService = new WorkOrderService(mongoService);
            _userService = new UserService(mongoService);

            // Check if user is authenticated
            if (!_authService.IsAuthenticated || _authService.CurrentUser == null)
            {
                Close();
                return;
            }

            // Set user greeting
            UserGreeting.Text = $"Welcome, {_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName}";

            // Show admin features if user is admin
            if (_authService.CurrentUser.Role == "Admin")
            {
                UsersBtn.Visibility = Visibility.Visible;
            }

            // Load dashboard
            LoadDashboard();
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Name;
                switch (tag)
                {
                    case "DashboardBtn":
                        LoadDashboard();
                        break;
                    case "CustomersBtn":
                        ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/CustomersPage.xaml"));
                        break;
                    case "WorkOrdersBtn":
                        ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/WorkOrdersPage.xaml"));
                        break;
                    case "HistoryBtn":
                        ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/HistoryPage.xaml"));
                        break;
                    case "AnalyticsBtn":
                        ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/AnalyticsPage.xaml"));
                        break;
                    case "UsersBtn":
                        ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/UsersPage.xaml"));
                        break;
                }
            }
        }

        private void LoadDashboard()
        {
            ContentFrame.Navigate(new Uri("pack://application:,,,/Views/Pages/DashboardPage.xaml"));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            LoginWindow loginWindow = new(_authService);
            loginWindow.Show();
            Close();
        }
    }
}