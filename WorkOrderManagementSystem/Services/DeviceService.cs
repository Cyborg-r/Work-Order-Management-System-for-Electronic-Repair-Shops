using MongoDB.Bson;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class DeviceService
    {
        private readonly MongoDbService _mongoDbService;

        public DeviceService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            try
            {
                return await _mongoDbService.Devices.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving devices: {ex.Message}");
                return new List<Device>();
            }
        }

        public async Task<Device?> GetDeviceByIdAsync(ObjectId id)
        {
            try
            {
                return await _mongoDbService.Devices.Find(d => d.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving device: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Device>> GetDevicesByCustomerAsync(ObjectId customerId)
        {
            try
            {
                return await _mongoDbService.Devices.Find(d => d.CustomerId == customerId).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving devices by customer: {ex.Message}");
                return new List<Device>();
            }
        }

        public async Task<List<Device>> SearchDevicesAsync(string searchTerm)
        {
            try
            {
                var filter = Builders<Device>.Filter.Or(
                    Builders<Device>.Filter.Regex(d => d.Brand, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Device>.Filter.Regex(d => d.SerialNumber, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Device>.Filter.Regex(d => d.DeviceType, new BsonRegularExpression(searchTerm, "i"))
                );

                return await _mongoDbService.Devices.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching devices: {ex.Message}");
                return new List<Device>();
            }
        }

        public async Task<ObjectId> AddDeviceAsync(Device device)
        {
            try
            {
                device.Id = ObjectId.GenerateNewId();
                device.CreatedAt = DateTime.UtcNow;
                device.LastModified = DateTime.UtcNow;
                await _mongoDbService.Devices.InsertOneAsync(device);
                return device.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding device: {ex.Message}");
                return ObjectId.Empty;
            }
        }

        public async Task<bool> UpdateDeviceAsync(Device device)
        {
            try
            {
                device.LastModified = DateTime.UtcNow;
                var result = await _mongoDbService.Devices.ReplaceOneAsync(d => d.Id == device.Id, device);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating device: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDeviceAsync(ObjectId id)
        {
            try
            {
                var result = await _mongoDbService.Devices.DeleteOneAsync(d => d.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting device: {ex.Message}");
                return false;
            }
        }
    }
}
