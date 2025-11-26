using System.Windows;

namespace WorkOrderManagementSystem.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly Services.AuthenticationService _authService;

        public RegisterWindow(Services.AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string username = UsernameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Please fill in all fields.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage.Text = "Passwords do not match.";
                return;
            }

            if (password.Length < 6)
            {
                ErrorMessage.Text = "Password must be at least 6 characters long.";
                return;
            }

            if (_authService.Register(username, email, password, firstName, lastName))
            {
                MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoginWindow loginWindow = new(_authService);
                loginWindow.Show();
                Close();
            }
            else
            {
                ErrorMessage.Text = "Username already exists or registration failed.";
            }
        }

        private void BackLink_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new(_authService);
            loginWindow.Show();
            Close();
        }
    }
}
