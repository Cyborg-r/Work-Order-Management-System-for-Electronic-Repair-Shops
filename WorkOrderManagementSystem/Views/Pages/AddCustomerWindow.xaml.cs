using System.Windows;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class AddCustomerWindow : Window
    {
        private readonly CustomerService _customerService;

        public AddCustomerWindow(CustomerService customerService)
        {
            InitializeComponent();
            _customerService = customerService;
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    var customer = new Customer
                    {
                        FirstName = FirstNameBox.Text,
                        LastName = LastNameBox.Text,
                        Email = EmailBox.Text,
                        Phone = PhoneBox.Text,
                        Address = AddressBox.Text
                    };

                    await _customerService.AddCustomerAsync(customer);
                    MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (string.IsNullOrWhiteSpace(EmailBox.Text) || string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Please enter email and phone.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }

    public partial class AddCustomerWindow : Window
    {
        // This partial class definition is here to allow for code modifications
        // in separate files, if needed, without modifying the auto-generated code.
    }
}
