using System.Windows;
using System.Windows.Controls;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class EditUserWindow : Window
    {
        private readonly UserService _userService;
        private readonly User _user;

        public EditUserWindow(UserService userService, User user)
        {
            InitializeComponent();
            _userService = userService;
            _user = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            UsernameBox.Text = _user.Username;
            FirstNameBox.Text = _user.FirstName;
            LastNameBox.Text = _user.LastName;
            EmailBox.Text = _user.Email;
            RoleComboBox.SelectedItem = _user.Role;
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text))
                {
                    MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (RoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a role.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update user data
                _user.FirstName = FirstNameBox.Text.Trim();
                _user.LastName = LastNameBox.Text.Trim();
                _user.Email = EmailBox.Text.Trim();
                _user.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Staff";

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordBox.Password);
                }

                await _userService.UpdateUserAsync(_user);
                MessageBox.Show("User updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to delete the user '{_user.FirstName} {_user.LastName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _userService.DeleteUserAsync(_user.Id);
                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
