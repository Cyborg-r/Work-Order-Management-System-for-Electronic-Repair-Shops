using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class AnalyticsPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly WorkOrderService _workOrderService;
        private readonly UserService _userService;
        private readonly DeviceService _deviceService;
        private readonly CustomerService _customerService;

        public AnalyticsPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _workOrderService = new WorkOrderService(_mongoDbService);
            _userService = new UserService(_mongoDbService);
            _deviceService = new DeviceService(_mongoDbService);
            _customerService = new CustomerService(_mongoDbService);
            LoadAnalytics();
        }

        private async void LoadAnalytics()
        {
            try
            {
                var allWorkOrders = await _workOrderService.GetAllWorkOrdersAsync();
                var allCustomers = await _customerService.GetAllCustomersAsync();
                var completedOrders = allWorkOrders.Where(w => w.Status == "Completed").ToList();

                // Calculate metrics
                decimal totalRevenue = completedOrders.Sum(w => w.TotalCost);
                int jobsCompleted = completedOrders.Count;
                double avgTurnaround = completedOrders.Any()
                    ? completedOrders.Average(w => (w.CompletedAt - w.CreatedAt)?.TotalDays ?? 0)
                    : 0;
                int activeCustomers = allCustomers.Count;

                // Update metrics
                TotalRevenueText.Text = $"Php {totalRevenue:N0}";
                JobsCompletedText.Text = jobsCompleted.ToString();
                AvgTurnaroundText.Text = $"{avgTurnaround:N1} days";
                ActiveCustomersText.Text = activeCustomers.ToString();

                // Load technician performance
                await LoadTechnicianPerformance(allWorkOrders);

                // Load job type distribution
                await LoadJobTypeDistribution(allWorkOrders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTechnicianPerformance(List<Models.WorkOrder> allWorkOrders)
        {
            try
            {
                var technicians = await _userService.GetUsersByRoleAsync("Technician");
                var techPerformance = new ObservableCollection<dynamic>();

                foreach (var tech in technicians)
                {
                    var techJobs = allWorkOrders.Where(w => w.TechnicianId == tech.Id).ToList();
                    var completedJobs = techJobs.Where(w => w.Status == "Completed").ToList();
                    var pendingJobs = techJobs.Where(w => w.Status == "Pending").ToList();
                    var inProgressJobs = techJobs.Where(w => w.Status == "In Progress").ToList();
                    
                    double avgTurnaround = completedJobs.Any()
                        ? completedJobs.Average(w => (w.CompletedAt - w.CreatedAt)?.TotalDays ?? 0)
                        : 0;
                    
                    decimal revenue = completedJobs.Sum(w => w.TotalCost);

                    techPerformance.Add(new
                    {
                        Name = $"{tech.FirstName} {tech.LastName}",
                        CompletedJobs = completedJobs.Count,
                        PendingJobs = pendingJobs.Count,
                        InProgressJobs = inProgressJobs.Count,
                        AvgTurnaround = $"{avgTurnaround:N1} days",
                        Revenue = $"Php {revenue:N0}"
                    });
                }

                TechnicianGrid.ItemsSource = techPerformance;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading technician performance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadJobTypeDistribution(List<Models.WorkOrder> allWorkOrders)
        {
            try
            {
                var allDevices = await _deviceService.GetAllDevicesAsync();
                var completedOrders = allWorkOrders.Where(w => w.Status == "Completed").ToList();
                var totalRevenue = completedOrders.Sum(w => w.TotalCost);
                var totalJobs = completedOrders.Count;

                var jobDistribution = new ObservableCollection<dynamic>();

                // Group by IssueDescription (repair type) and DeviceId
                var groupedByIssueAndDevice = completedOrders.GroupBy(w => new { w.IssueDescription, w.DeviceId });

                foreach (var group in groupedByIssueAndDevice)
                {
                    var count = group.Count();
                    var percentage = totalJobs > 0 ? (count * 100.0) / totalJobs : 0;
                    var revenue = group.Sum(w => w.TotalCost);
                    var device = allDevices.FirstOrDefault(d => d.Id == group.Key.DeviceId);
                    var deviceType = device?.DeviceType ?? "Unknown";

                    jobDistribution.Add(new
                    {
                        Device = deviceType,
                        JobType = group.Key.IssueDescription,
                        Count = count,
                        Share = $"{percentage:N0}%",
                        Revenue = $"Php {revenue:N0}"
                    });
                }

                JobTypeGrid.ItemsSource = jobDistribution.OrderByDescending(x => (int)x.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading job type distribution: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
