using MongoDB.Driver;
using System.Configuration;
using System.Data;
using System.Windows;
using WorkOrderManagementSystem.Services;
using WorkOrderManagementSystem.Views;

namespace WorkOrderManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AuthenticationService? AuthService { get; private set; }
        private static MongoDbService? _mongoService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialize MongoDB Service
                _mongoService = new MongoDbService();

                // Initialize Authentication Service
                AuthService = new AuthenticationService(_mongoService);

                // Create admin user if no users exist
                var users = _mongoService.Users.Find(_ => true).ToList();
                if (users.Count == 0)
                {
                    var adminUser = new Models.User
                    {
                        Username = "admin",
                        Email = "admin@workorder.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FirstName = "Admin",
                        LastName = "User",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _mongoService.Users.InsertOne(adminUser);
                }

                // Show login window
                LoginWindow loginWindow = new(AuthService);
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}\n\nPlease ensure MongoDB is running on localhost:27017", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
