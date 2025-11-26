using MongoDB.Bson;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class UserService
    {
        private readonly MongoDbService _mongoDbService;

        public UserService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _mongoDbService.Users.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(ObjectId id)
        {
            try
            {
                return await _mongoDbService.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving user: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            try
            {
                return await _mongoDbService.Users.Find(u => u.Role == role && u.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving users by role: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _mongoDbService.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(ObjectId id)
        {
            try
            {
                var update = Builders<User>.Update.Set(u => u.IsActive, false);
                var result = await _mongoDbService.Users.UpdateOneAsync(u => u.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deactivating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(ObjectId id)
        {
            try
            {
                var result = await _mongoDbService.Users.DeleteOneAsync(u => u.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(ObjectId userId, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                    return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                return await UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing password: {ex.Message}");
                return false;
            }
        }
    }
}
