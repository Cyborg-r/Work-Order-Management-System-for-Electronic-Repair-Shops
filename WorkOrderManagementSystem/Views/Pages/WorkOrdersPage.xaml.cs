using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MongoDB.Bson;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class WorkOrdersPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly WorkOrderService _workOrderService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly UserService _userService;
        private ObservableCollection<dynamic> _workOrders = new();
        private string _currentFilter = "All";

        public WorkOrdersPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _workOrderService = new WorkOrderService(_mongoDbService);
            _customerService = new CustomerService(_mongoDbService);
            _deviceService = new DeviceService(_mongoDbService);
            _userService = new UserService(_mongoDbService);
            LoadWorkOrders();
        }

        private async void LoadWorkOrders(string? statusFilter = null)
        {
            try
            {
                List<WorkOrder> workOrders;
                if (statusFilter != null && statusFilter != "All")
                {
                    workOrders = await _workOrderService.GetWorkOrdersByStatusAsync(statusFilter);
                }
                else
                {
                    workOrders = await _workOrderService.GetAllWorkOrdersAsync();
                }

                _workOrders.Clear();
                foreach (var wo in workOrders.OrderByDescending(x => x.CreatedAt))
                {
                    var customer = await _customerService.GetCustomerByIdAsync(wo.CustomerId);
                    var device = await _deviceService.GetDeviceByIdAsync(wo.DeviceId);
                    var technician = wo.TechnicianId.HasValue ? await _userService.GetUserByIdAsync(wo.TechnicianId.Value) : null;

                    _workOrders.Add(new
                    {
                        Id = wo.Id,
                        WorkOrderNumber = wo.WorkOrderNumber,
                        CustomerName = customer?.FullName ?? "Unknown",
                        CustomerPhone = customer?.Phone ?? "N/A",
                        CustomerDisplay = FormatCustomerDisplay(customer),
                        DeviceType = device?.DeviceType ?? "Unknown",
                        DeviceBrand = device?.Brand ?? "Unknown",
                        DeviceDisplay = FormatDeviceDisplay(device),
                        SerialNumber = device?.SerialNumber ?? "N/A",
                        Diagnosis = wo.IssueDescription,
                        TechnicianName = technician != null ? $"{technician.FirstName} {technician.LastName}" : "Unassigned",
                        Status = wo.Status,
                        TotalCost = wo.TotalCost,
                        TotalCostDisplay = $"Php {wo.TotalCost:N2}",
                        LaborCost = wo.LaborCost,
                        PartsCost = wo.PartsCost,
                        PartsRequired = wo.PartsRequired,
                        IssueDescription = wo.IssueDescription,
                        CreatedAt = wo.CreatedAt.ToString("yyyy-MM-dd"),
                        CompletedAt = wo.CompletedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                        Model = wo
                    });
                }

                WorkOrdersGrid.ItemsSource = _workOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading work orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatCustomerDisplay(Customer? customer)
        {
            if (customer == null) return "Unknown";
            return $"{customer.FullName}\n{customer.Phone}";
        }

        private string FormatDeviceDisplay(Device? device)
        {
            if (device == null) return "Unknown";
            return $"{device.DeviceType}\n{device.Brand}";
        }

        private void AddWorkOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            AddWorkOrderWindow window = new(_workOrderService, _customerService, _deviceService, _userService);
            if (window.ShowDialog() == true)
            {
                LoadWorkOrders(_currentFilter == "All" ? null : _currentFilter);
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filter)
            {
                _currentFilter = filter;
                
                // Update button styles
                AllOrdersBtn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                AllOrdersBtn.BorderThickness = new Thickness(0);
                PendingBtn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                PendingBtn.BorderThickness = new Thickness(0);
                InProgressBtn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                InProgressBtn.BorderThickness = new Thickness(0);
                CompletedBtn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                CompletedBtn.BorderThickness = new Thickness(0);

                btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));
                btn.BorderThickness = new Thickness(0, 0, 0, 2);
                btn.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243));

                LoadWorkOrders(filter == "All" ? null : filter);
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ObjectId workOrderId)
            {
                var workOrderItem = _workOrders.FirstOrDefault(w => (ObjectId)w.Id == workOrderId);
                if (workOrderItem != null)
                {
                    WorkOrder wo = workOrderItem.Model;
                    WorkOrderDetailsWindow detailsWindow = new(_mongoDbService, _workOrderService, _customerService, _deviceService, _userService, wo);
                    if (detailsWindow.ShowDialog() == true)
                    {
                        LoadWorkOrders(_currentFilter == "All" ? null : _currentFilter);
                    }
                }
            }
        }
    }
}
