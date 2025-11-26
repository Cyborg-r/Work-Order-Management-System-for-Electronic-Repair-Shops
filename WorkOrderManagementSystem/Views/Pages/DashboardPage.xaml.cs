using System.Windows.Controls;
using MongoDB.Driver;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly WorkOrderService _workOrderService;

        public DashboardPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _workOrderService = new WorkOrderService(_mongoDbService);
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                var analytics = await _workOrderService.GetAnalyticsAsync();
                var workOrders = await _workOrderService.GetAllWorkOrdersAsync();

                // Update stats
                TotalWorkOrdersText.Text = analytics["TotalWorkOrders"].ToString();
                CompletedOrdersText.Text = analytics["CompletedOrders"].ToString();
                PendingOrdersText.Text = analytics["PendingOrders"].ToString();
                TotalRevenueText.Text = $"Php {analytics["TotalRevenue"]:N2}";

                // Load recent work orders
                var customerService = new CustomerService(_mongoDbService);
                var deviceService = new DeviceService(_mongoDbService);

                var recentOrders = workOrders.Take(5).ToList();
                var displayOrders = new System.Collections.ObjectModel.ObservableCollection<dynamic>();

                foreach (var wo in recentOrders)
                {
                    var customer = await customerService.GetCustomerByIdAsync(wo.CustomerId);
                    var device = await deviceService.GetDeviceByIdAsync(wo.DeviceId);

                    displayOrders.Add(new
                    {
                        WorkOrderNumber = wo.WorkOrderNumber,
                        CustomerName = customer?.FullName ?? "Unknown",
                        DeviceType = device?.DeviceType ?? "Unknown",
                        Status = wo.Status,
                        CreatedAt = wo.CreatedAt
                    });
                }

                RecentWorkOrdersGrid.ItemsSource = displayOrders;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }
    }
}
