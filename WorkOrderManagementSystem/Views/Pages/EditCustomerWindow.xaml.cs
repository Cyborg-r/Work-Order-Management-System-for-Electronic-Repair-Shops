using System.Windows;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class EditCustomerWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly Customer _customer;

        public EditCustomerWindow(CustomerService customerService, Customer customer)
        {
            InitializeComponent();
            _customerService = customerService;
            _customer = customer;
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            FirstNameBox.Text = _customer.FirstName;
            LastNameBox.Text = _customer.LastName;
            EmailBox.Text = _customer.Email;
            PhoneBox.Text = _customer.Phone;
            AddressBox.Text = _customer.Address;
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    _customer.FirstName = FirstNameBox.Text;
                    _customer.LastName = LastNameBox.Text;
                    _customer.Email = EmailBox.Text;
                    _customer.Phone = PhoneBox.Text;
                    _customer.Address = AddressBox.Text;

                    await _customerService.UpdateCustomerAsync(_customer);
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(_customer.Id);
                    MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show("Please enter first and last name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }

    public partial class EditCustomerWindow : Window
    {
        // This partial class declaration is added to fix the issue
    }
}
