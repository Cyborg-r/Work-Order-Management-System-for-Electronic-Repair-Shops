using System.Windows;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class TransactionDetailsWindow : Window
    {
        private readonly WorkOrderService _workOrderService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly UserService _userService;
        private readonly WorkOrder _workOrder;
        private Customer? _customer;
        private Device? _device;

        public TransactionDetailsWindow(WorkOrderService workOrderService, CustomerService customerService, 
            DeviceService deviceService, UserService userService, WorkOrder workOrder)
        {
            InitializeComponent();
            _workOrderService = workOrderService;
            _customerService = customerService;
            _deviceService = deviceService;
            _userService = userService;
            _workOrder = workOrder;
            LoadTransactionDetails();
        }

        private async void LoadTransactionDetails()
        {
            try
            {
                // Set title with work order number
                TitleBlock.Text = $"Transaction Details - {_workOrder.WorkOrderNumber}";

                // Load customer info
                _customer = await _customerService.GetCustomerByIdAsync(_workOrder.CustomerId);
                if (_customer != null)
                {
                    CustomerNameBlock.Text = _customer.FullName;
                    CustomerPhoneBlock.Text = _customer.Phone;
                    CustomerEmailBlock.Text = _customer.Email;
                }

                // Load device info
                _device = await _deviceService.GetDeviceByIdAsync(_workOrder.DeviceId);
                if (_device != null)
                {
                    DeviceTypeBlock.Text = _device.DeviceType;
                    BrandBlock.Text = _device.Brand;
                    SerialNumberBlock.Text = _device.SerialNumber;
                }

                // Set work order details
                StatusBlock.Text = _workOrder.Status;
                StatusBlock.Background = GetStatusColor(_workOrder.Status);
                IssueBlock.Text = _workOrder.IssueDescription ?? "N/A";
                PartsUsedBlock.Text = _workOrder.PartsRequired ?? "N/A";
                LaborCostBlock.Text = $"Php {_workOrder.LaborCost:N2}";
                PartsCostBlock.Text = $"Php {_workOrder.PartsCost:N2}";
                TotalCostBlock.Text = $"Php {_workOrder.TotalCost:N2}";

                // Set technician
                if (_workOrder.TechnicianId.HasValue)
                {
                    var technician = await _userService.GetUserByIdAsync(_workOrder.TechnicianId.Value);
                    if (technician != null)
                    {
                        TechnicianBlock.Text = $"{technician.FirstName} {technician.LastName}";
                    }
                }
                else
                {
                    TechnicianBlock.Text = "Unassigned";
                }

                // Set dates
                DateCreatedBlock.Text = _workOrder.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                DateCompletedBlock.Text = _workOrder.CompletedAt?.ToString("yyyy-MM-dd") ?? "N/A";

                // Calculate duration
                if (_workOrder.Turnaround.HasValue)
                {
                    int days = (int)_workOrder.Turnaround.Value.TotalDays;
                    DurationBlock.Text = days == 1 ? "1 day" : $"{days} days";
                }
                else
                {
                    DurationBlock.Text = "N/A";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transaction details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Media.Brush GetStatusColor(string status)
        {
            return status switch
            {
                "Completed" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 34, 34)),  // Dark gray
                "In Progress" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)), // Blue
                "Pending" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)),     // Orange
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(117, 117, 117))           // Gray
            };
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show($"Receipt for {_workOrder.WorkOrderNumber} is printing...", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                // You can implement actual printing logic here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
