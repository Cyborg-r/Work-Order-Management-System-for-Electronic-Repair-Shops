using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class AddWorkOrderWindow : Window
    {
        private readonly WorkOrderService _workOrderService;
        private readonly CustomerService _customerService;
        private readonly DeviceService _deviceService;
        private readonly UserService _userService;
        private List<Customer> _customers = new();
        private List<User> _technicians = new();

        public AddWorkOrderWindow(WorkOrderService workOrderService, CustomerService customerService, DeviceService deviceService, UserService userService)
        {
            InitializeComponent();
            _workOrderService = workOrderService;
            _customerService = customerService;
            _deviceService = deviceService;
            _userService = userService;
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                _customers = await _customerService.GetAllCustomersAsync();
                _technicians = await _userService.GetUsersByRoleAsync("Technician");

                CustomerComboBox.ItemsSource = _customers;
                CustomerComboBox.DisplayMemberPath = "FullName";
                CustomerComboBox.SelectedValuePath = "Id";

                TechnicianComboBox.ItemsSource = _technicians;
                TechnicianComboBox.DisplayMemberPath = "FirstName";
                TechnicianComboBox.SelectedIndex = 0;

                // Wire up event handlers for cost calculation
                LaborCostBox.TextChanged += UpdateTotalCost;
                PartsCostBox.TextChanged += UpdateTotalCost;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotalCost(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(LaborCostBox.Text, out decimal laborCost) &&
                decimal.TryParse(PartsCostBox.Text, out decimal partsCost))
            {
                TotalCostBox.Text = (laborCost + partsCost).ToString("F2");
            }
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow numbers and decimal point
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    // Create or get the device
                    var device = new Device
                    {
                        CustomerId = (MongoDB.Bson.ObjectId)CustomerComboBox.SelectedValue,
                        DeviceType = DeviceTypeBox.Text,
                        Brand = BrandBox.Text,
                        SerialNumber = SerialNumberBox.Text
                    };

                    var deviceId = await _deviceService.AddDeviceAsync(device);

                    // Create work order with the new device
                    var workOrder = new WorkOrder
                    {
                        CustomerId = (MongoDB.Bson.ObjectId)CustomerComboBox.SelectedValue,
                        DeviceId = deviceId,
                        TechnicianId = TechnicianComboBox.SelectedItem is User tech ? tech.Id : null,
                        IssueDescription = IssueDescriptionBox.Text,
                        PartsRequired = PartsRequiredBox.Text,
                        LaborCost = decimal.Parse(LaborCostBox.Text),
                        PartsCost = decimal.Parse(PartsCostBox.Text),
                        Status = "Pending"
                    };

                    await _workOrderService.AddWorkOrderAsync(workOrder);
                    MessageBox.Show("Work order created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating work order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (CustomerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DeviceTypeBox.Text))
            {
                MessageBox.Show("Please enter device type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(BrandBox.Text))
            {
                MessageBox.Show("Please enter device brand.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(SerialNumberBox.Text))
            {
                MessageBox.Show("Please enter device serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(IssueDescriptionBox.Text))
            {
                MessageBox.Show("Please enter issue description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PartsRequiredBox.Text))
            {
                MessageBox.Show("Please enter parts required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(LaborCostBox.Text, out decimal laborCost) || laborCost < 0)
            {
                MessageBox.Show("Please enter a valid labor cost.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PartsCostBox.Text, out decimal partsCost) || partsCost < 0)
            {
                MessageBox.Show("Please enter a valid parts cost.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TechnicianComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a technician.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
