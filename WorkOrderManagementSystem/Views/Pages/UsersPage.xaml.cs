using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WorkOrderManagementSystem.Models;
using WorkOrderManagementSystem.Services;

namespace WorkOrderManagementSystem.Views.Pages
{
    public partial class UsersPage : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly UserService _userService;
        private ObservableCollection<dynamic> _users = new();
        private int _userIdCounter = 1;

        public UsersPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
            _userService = new UserService(_mongoDbService);
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _users.Clear();
                _userIdCounter = 1;

                foreach (var user in users.OrderByDescending(u => u.CreatedAt))
                {
                    var userId = $"U-{_userIdCounter:D3}";
                    _userIdCounter++;

                    _users.Add(new
                    {
                        Id = user.Id,
                        UserId = userId,
                        Username = user.Username,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Role = user.Role,
                        Status = user.IsActive ? "Active" : "Inactive",
                        CreatedDate = user.CreatedAt.ToString("yyyy-MM-dd"),
                        Model = user
                    });
                }

                UsersGrid.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow window = new(_userService);
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                dynamic item = btn.Tag;
                User user = item.Model;
                EditUserWindow window = new(_userService, user);
                if (window.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadUsers();
            }
            else
            {
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    var results = users.Where(u => 
                        u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    _users.Clear();
                    _userIdCounter = 1;

                    foreach (var user in results.OrderByDescending(u => u.CreatedAt))
                    {
                        var userId = $"U-{_userIdCounter:D3}";
                        _userIdCounter++;

                        _users.Add(new
                        {
                            Id = user.Id,
                            UserId = userId,
                            Username = user.Username,
                            FullName = $"{user.FirstName} {user.LastName}",
                            Email = user.Email,
                            Role = user.Role,
                            Status = user.IsActive ? "Active" : "Inactive",
                            CreatedDate = user.CreatedAt.ToString("yyyy-MM-dd"),
                            Model = user
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
