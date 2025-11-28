using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class HistoryPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly WorkOrderService _workOrderService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly UserService _userService;
        private ObservableCollection<dynamic> _history = new();
        private List<dynamic> _allHistory = new();

        public HistoryPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _workOrderService = new WorkOrderService(_mongoDbService);
            _customerService = new CustomerService(_mongoDbService);
            _deviceService = new DeviceService(_mongoDbService);
            _userService = new UserService(_mongoDbService);
            LoadHistory();
        }

        private async void LoadHistory()
        {
            try
            {
                // Get all completed work orders (including archived ones older than 1 day)
                var completedOrders = await _workOrderService.GetWorkOrdersByStatusAsync("Completed");
                _allHistory.Clear();
                _history.Clear();

                foreach (var wo in completedOrders.OrderByDescending(w => w.CompletedAt))
                {
                    var customer = await _customerService.GetCustomerByIdAsync(wo.CustomerId);
                    var device = await _deviceService.GetDeviceByIdAsync(wo.DeviceId);
                    var technician = wo.TechnicianId.HasValue ? await _userService.GetUserByIdAsync(wo.TechnicianId.Value) : null;
                    var turnaroundDays = wo.Turnaround?.TotalDays ?? 0;

                    var historyItem = new
                    {
                        WorkOrderNumber = wo.WorkOrderNumber,
                        CustomerName = customer?.FullName ?? "Unknown",
                        CustomerPhone = customer?.Phone ?? "N/A",
                        CustomerDisplay = FormatCustomerDisplay(customer),
                        DeviceType = device?.DeviceType ?? "Unknown",
                        DeviceBrand = device?.Brand ?? "Unknown",
                        DeviceDisplay = FormatDeviceDisplay(device),
                        Diagnosis = wo.IssueDescription,
                        TechnicianName = technician != null ? $"{technician.FirstName} {technician.LastName}" : "Unassigned",
                        TechnicianId = wo.TechnicianId,
                        CreatedAt = wo.CreatedAt,
                        CreatedAtDisplay = wo.CreatedAt.ToString("yyyy-MM-dd"),
                        CompletedAt = wo.CompletedAt,
                        CompletedAtDisplay = wo.CompletedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                        TurnaroundDays = (int)turnaroundDays,
                        DurationDisplay = turnaroundDays == 1 ? "1 day" : $"{(int)turnaroundDays} days",
                        TotalCost = $"Php {wo.TotalCost:N2}",
                        Model = wo
                    };

                    _allHistory.Add(historyItem);
                    _history.Add(historyItem);
                }

                HistoryGrid.ItemsSource = _history;

                // Update stats
                await UpdateStats();

                // Load technicians for filter
                await LoadTechnicianFilter();
                
                // Load months for filter
                LoadMonthFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateStats()
        {
            try
            {
                var analytics = await _workOrderService.GetAnalyticsAsync();

                // Total Completed
                TotalCompletedBlock.Text = _allHistory.Count.ToString();

                // Total Revenue
                TotalRevenueBlock.Text = $"Php {analytics["TotalRevenue"]:N2}";

                // Average Completion Time
                if (_allHistory.Count > 0)
                {
                    double avgDays = _allHistory.Average(h => (int)h.TurnaroundDays);
                    AvgCompletionBlock.Text = $"{avgDays:F1} days";
                }
                else
                {
                    AvgCompletionBlock.Text = "0 days";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stats: {ex.Message}");
            }
        }

        private async Task LoadTechnicianFilter()
        {
            try
            {
                TechnicianFilterCombo.Items.Clear();
                TechnicianFilterCombo.Items.Add("All Technicians");

                var technicians = await _userService.GetUsersByRoleAsync("Technician");
                foreach (var tech in technicians)
                {
                    TechnicianFilterCombo.Items.Add($"{tech.FirstName} {tech.LastName}");
                }

                TechnicianFilterCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading technicians: {ex.Message}");
            }
        }

        private void LoadMonthFilter()
        {
            try
            {
                MonthFilterCombo.Items.Clear();
                MonthFilterCombo.Items.Add("All Months");

                var months = new[] 
                { 
                    "January", "February", "March", "April", "May", "June",
                    "July", "August", "September", "October", "November", "December"
                };

                foreach (var month in months)
                {
                    MonthFilterCombo.Items.Add(month);
                }

                MonthFilterCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading months: {ex.Message}");
            }
        }

        private string FormatCustomerDisplay(Customer? customer)
        {
            if (customer == null) return "Unknown";
            return $"{customer.FullName}\n({customer.Phone})";
        }

        private string FormatDeviceDisplay(Device? device)
        {
            if (device == null) return "Unknown";
            return $"{device.DeviceType}\n{device.Brand}";
        }

        private void ApplyFilters()
        {
            try
            {
                var filtered = _allHistory.AsEnumerable();

                // Apply technician filter
                var selectedTechnician = TechnicianFilterCombo.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedTechnician) && selectedTechnician != "All Technicians")
                {
                    filtered = filtered.Where(h => h.TechnicianName == selectedTechnician);
                }

                // Apply month filter
                var selectedMonth = MonthFilterCombo.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedMonth) && selectedMonth != "All Months")
                {
                    int monthNumber = Array.IndexOf(new[] 
                    { 
                        "January", "February", "March", "April", "May", "June",
                        "July", "August", "September", "October", "November", "December"
                    }, selectedMonth) + 1;

                    filtered = filtered.Where(h => 
                    {
                        var completedAt = (DateTime?)h.CompletedAt;
                        return completedAt?.Month == monthNumber && completedAt?.Year == DateTime.Now.Year;
                    });
                }

                _history.Clear();
                foreach (var item in filtered)
                {
                    _history.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying filters: {ex.Message}");
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                ApplyFilters();
            }
            else
            {
                try
                {
                    var results = await _workOrderService.SearchWorkOrdersAsync(searchTerm);
                    var completedResults = results.Where(w => w.Status == "Completed").ToList();

                    _history.Clear();
                    foreach (var wo in completedResults.OrderByDescending(w => w.CompletedAt))
                    {
                        var customer = await _customerService.GetCustomerByIdAsync(wo.CustomerId);
                        var device = await _deviceService.GetDeviceByIdAsync(wo.DeviceId);
                        var technician = wo.TechnicianId.HasValue ? await _userService.GetUserByIdAsync(wo.TechnicianId.Value) : null;
                        var turnaroundDays = wo.Turnaround?.TotalDays ?? 0;

                        _history.Add(new
                        {
                            WorkOrderNumber = wo.WorkOrderNumber,
                            CustomerName = customer?.FullName ?? "Unknown",
                            CustomerDisplay = FormatCustomerDisplay(customer),
                            DeviceType = device?.DeviceType ?? "Unknown",
                            DeviceBrand = device?.Brand ?? "Unknown",
                            DeviceDisplay = FormatDeviceDisplay(device),
                            Diagnosis = wo.IssueDescription,
                            TechnicianName = technician != null ? $"{technician.FirstName} {technician.LastName}" : "Unassigned",
                            TechnicianId = wo.TechnicianId,
                            CreatedAt = wo.CreatedAt,
                            CreatedAtDisplay = wo.CreatedAt.ToString("yyyy-MM-dd"),
                            CompletedAt = wo.CompletedAt,
                            CompletedAtDisplay = wo.CompletedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                            TurnaroundDays = (int)turnaroundDays,
                            DurationDisplay = turnaroundDays == 1 ? "1 day" : $"{(int)turnaroundDays} days",
                            TotalCost = $"Php {wo.TotalCost:N2}",
                            Model = wo
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TechnicianFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void MonthFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ViewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string workOrderNumber)
            {
                try
                {
                    // Find the work order in history
                    var historyItem = _allHistory.FirstOrDefault(h => h.WorkOrderNumber == workOrderNumber);
                    if (historyItem != null)
                    {
                        var workOrder = historyItem.Model as WorkOrder;
                        if (workOrder != null)
                        {
                            // Open the TransactionDetailsWindow
                            TransactionDetailsWindow detailsWindow = new(
                                _workOrderService,
                                _customerService,
                                _deviceService,
                                _userService,
                                workOrder
                            );
                            detailsWindow.Owner = Window.GetWindow(this);
                            detailsWindow.ShowDialog();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening transaction details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
