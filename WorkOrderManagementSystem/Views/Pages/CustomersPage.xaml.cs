using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class CustomersPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly CustomerService _customerService;
        private readonly WorkOrderService _workOrderService;
        private ObservableCollection<dynamic> _customers = new();
        private Customer? _selectedCustomer;

        public CustomersPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _customerService = new CustomerService(_mongoDbService);
            _workOrderService = new WorkOrderService(_mongoDbService);
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var workOrderService = _workOrderService;

                _customers.Clear();
                foreach (var customer in customers.OrderByDescending(c => c.CreatedAt))
                {
                    // Get total jobs for this customer
                    var customerWorkOrders = await workOrderService.GetWorkOrdersByCustomerAsync(customer.Id);
                    var lastVisit = customerWorkOrders.OrderByDescending(w => w.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("yyyy-MM-dd") ?? "N/A";

                    _customers.Add(new
                    {
                        Id = customer.Id,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        TotalJobs = customerWorkOrders.Count,
                        LastVisit = lastVisit,
                        Model = customer
                    });
                }

                CustomersGrid.ItemsSource = _customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow window = new(_customerService);
            if (window.ShowDialog() == true)
            {
                LoadCustomers();
            }
        }

        private void CustomersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dynamic? item = CustomersGrid.SelectedItem as dynamic;
            if (item != null)
            {
                Customer customer = item.Model;
                EditCustomerWindow window = new(_customerService, customer);
                if (window.ShowDialog() == true)
                {
                    LoadCustomers();
                }
            }
        }

        private async void CustomersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic? item = CustomersGrid.SelectedItem as dynamic;
            if (item != null)
            {
                Customer customer = item.Model;
                _selectedCustomer = customer;
                await DisplayCustomerHistory(customer);
            }
        }

        private async Task DisplayCustomerHistory(Customer customer)
        {
            try
            {
                // Update title
                HistoryTitle.Text = $"{customer.FullName} - History";

                // Show contact information
                HistoryPhone.Text = customer.Phone;
                HistoryEmail.Text = customer.Email;
                HistoryAddress.Text = customer.Address;
                ContactInfoPanel.Visibility = Visibility.Visible;

                // Get customer's repair history
                var workOrders = await _workOrderService.GetWorkOrdersByCustomerAsync(customer.Id);
                var completedOrders = workOrders.Where(w => w.Status == "Completed").OrderByDescending(w => w.CompletedAt).ToList();

                // Populate repair history
                var historyItems = new ObservableCollection<dynamic>();

                foreach (var order in completedOrders)
                {
                    var device = await _mongoDbService.Devices.Find(d => d.Id == order.DeviceId).FirstOrDefaultAsync();

                    historyItems.Add(new
                    {
                        WorkOrderNumber = order.WorkOrderNumber,
                        DeviceModel = device?.DeviceType ?? "Unknown Device",
                        Diagnosis = order.IssueDescription,
                        Status = order.Status,
                        CompletedAt = order.CompletedAt?.ToString("yyyy-MM-dd") ?? "Pending",
                        TotalCost = $"${order.TotalCost:N2}"
                    });
                }

                RepairHistoryItems.ItemsSource = historyItems;
                RepairHistoryPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is MongoDB.Bson.ObjectId customerId)
            {
                // Find and select the customer in the grid
                var customerItem = _customers.FirstOrDefault(c => (MongoDB.Bson.ObjectId)c.Id == customerId);
                if (customerItem != null)
                {
                    CustomersGrid.SelectedItem = customerItem;
                    CustomersGrid.ScrollIntoView(customerItem);
                }
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadCustomers();
            }
            else
            {
                try
                {
                    var results = await _customerService.SearchCustomersAsync(searchTerm);
                    var workOrderService = _workOrderService;

                    _customers.Clear();
                    foreach (var customer in results.OrderByDescending(c => c.CreatedAt))
                    {
                        var customerWorkOrders = await workOrderService.GetWorkOrdersByCustomerAsync(customer.Id);
                        var lastVisit = customerWorkOrders.OrderByDescending(w => w.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("yyyy-MM-dd") ?? "N/A";

                        _customers.Add(new
                        {
                            Id = customer.Id,
                            FullName = customer.FullName,
                            Email = customer.Email,
                            Phone = customer.Phone,
                            Address = customer.Address,
                            TotalJobs = customerWorkOrders.Count,
                            LastVisit = lastVisit,
                            Model = customer
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
