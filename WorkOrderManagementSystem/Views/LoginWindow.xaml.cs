using System.Windows;

namespace WorkOrderManagementSystem.Views
{
    public partial class LoginWindow : Window
    {
        private readonly Services.AuthenticationService _authService;

        public LoginWindow(Services.AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Please enter username and password.";
                return;
            }

            if (_authService.Login(username, password))
            {
                MainWindow mainWindow = new();
                mainWindow.Show();
                Close();
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password.";
                PasswordBox.Clear();
            }
        }

        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Your password reset request has been submitted to the system administrator.\n\n" +
                "The administrator will review your request and send you a new password via email shortly.",
                "Password Reset Request Submitted",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
