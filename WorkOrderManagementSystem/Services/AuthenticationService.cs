using MongoDB.Bson;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class AuthenticationService
    {
        private readonly MongoDbService _mongoDbService;
        private User? _currentUser;

        public User? CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        public bool IsAuthenticated => _currentUser != null;

        public AuthenticationService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public bool Login(string username, string password)
        {
            try
            {
                var user = _mongoDbService.Users.Find(u => u.Username == username && u.IsActive).FirstOrDefault();

                if (user == null)
                    return false;

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return false;

                _currentUser = user;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool Register(string username, string email, string password, string firstName, string lastName)
        {
            try
            {
                // Check if user already exists
                var existingUser = _mongoDbService.Users.Find(u => u.Username == username).FirstOrDefault();
                if (existingUser != null)
                    return false;

                var newUser = new User
                {
                    Id = ObjectId.GenerateNewId(),
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _mongoDbService.Users.InsertOne(newUser);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public bool CreateUserAsAdmin(string username, string email, string password, string firstName, string lastName, string role)
        {
            if (_currentUser?.Role != "Admin")
                return false;

            try
            {
                var existingUser = _mongoDbService.Users.Find(u => u.Username == username).FirstOrDefault();
                if (existingUser != null)
                    return false;

                var newUser = new User
                {
                    Id = ObjectId.GenerateNewId(),
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _mongoDbService.Users.InsertOne(newUser);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin user creation error: {ex.Message}");
                return false;
            }
        }

        public bool HasPermission(string requiredRole)
        {
            if (!IsAuthenticated || _currentUser == null)
                return false;

            if (_currentUser.Role == "Admin")
                return true; // Admins have all permissions

            return _currentUser.Role == requiredRole;
        }
    }
}
