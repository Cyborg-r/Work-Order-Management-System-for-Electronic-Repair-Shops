using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Printing;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class WorkOrderDetailsWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        private readonly WorkOrderService _workOrderService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly UserService _userService;
        private readonly WorkOrder _workOrder;
        private Customer? _customer;
        private Device? _device;

        public WorkOrderDetailsWindow(MongoDbService mongoDbService, WorkOrderService workOrderService, 
            CustomerService customerService, DeviceService deviceService, UserService userService, WorkOrder workOrder)
        {
            InitializeComponent();
            _mongoDbService = mongoDbService;
            _workOrderService = workOrderService;
            _customerService = customerService;
            _deviceService = deviceService;
            _userService = userService;
            _workOrder = workOrder;
            LoadWorkOrderDetails();
        }

        private async void LoadWorkOrderDetails()
        {
            try
            {
                // Set title with work order number
                TitleBlock.Text = $"Work Order Details - {_workOrder.WorkOrderNumber}";

                // Load customer info
                _customer = await _customerService.GetCustomerByIdAsync(_workOrder.CustomerId);
                if (_customer != null)
                {
                    CustomerNameBlock.Text = _customer.FullName;
                    CustomerPhoneBlock.Text = _customer.Phone;
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
                TitleBlock.Text = $"Work Order Details - {_workOrder.WorkOrderNumber}";
                StatusCombo.SelectedItem = _workOrder.Status;
                IssueBlock.Text = _workOrder.IssueDescription ?? "N/A";
                PartsRequiredBlock.Text = _workOrder.PartsRequired ?? "N/A";
                LaborCostBlock.Text = $"Php {_workOrder.LaborCost:N2}";
                PartsCostBlock.Text = $"Php {_workOrder.PartsCost:N2}";
                TotalCostBlock.Text = $"Php {_workOrder.TotalCost:N2}";
                
                // Set technician
                if (_workOrder.TechnicianId.HasValue)
                {
                    var technician = await _userService.GetUserByIdAsync(_workOrder.TechnicianId.Value);
                    if (technician != null)
                    {
                        TechnicianCombo.Text = $"{technician.FirstName} {technician.LastName}";
                    }
                }

                // Set date created
                DateCreatedBlock.Text = _workOrder.CreatedAt.ToString("yyyy-MM-dd");

                // Load available technicians
                await LoadTechnicians();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading work order details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTechnicians()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var technicians = users.Where(u => u.Role == "Technician").ToList();
                
                TechnicianCombo.Items.Clear();
                foreach (var tech in technicians)
                {
                    TechnicianCombo.Items.Add($"{tech.FirstName} {tech.LastName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading technicians: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update status
                if (StatusCombo.SelectedItem is ComboBoxItem statusItem)
                {
                    string newStatus = statusItem.Content?.ToString() ?? _workOrder.Status;
                    
                    // Update timestamps based on status
                    if (newStatus == "In Progress" && !_workOrder.StartedAt.HasValue)
                    {
                        _workOrder.StartedAt = DateTime.UtcNow;
                    }
                    else if (newStatus == "Completed" && !_workOrder.CompletedAt.HasValue)
                    {
                        _workOrder.CompletedAt = DateTime.UtcNow;
                    }
                    
                    _workOrder.Status = newStatus;
                }

                // Save to database
                await _workOrderService.UpdateWorkOrderAsync(_workOrder);
                MessageBox.Show("Work order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving work order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DropBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete this work order?\n\nThis action will permanently delete the work order and its associated device and cannot be undone.",
                    "Confirm Delete Work Order",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                // If user confirms the deletion
                if (result == MessageBoxResult.Yes)
                {
                    // Delete the associated device first
                    if (_device != null)
                    {
                        await _deviceService.DeleteDeviceAsync(_device.Id);
                    }

                    // Delete the work order
                    await _workOrderService.DeleteWorkOrderAsync(_workOrder.Id);
                    
                    MessageBox.Show("Work order and associated device deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting work order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Reciept is printing...", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                // You can implement actual printing logic here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
