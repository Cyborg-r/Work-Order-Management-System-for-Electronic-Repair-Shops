using MongoDB.Bson;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class CustomerService
    {
        private readonly MongoDbService _mongoDbService;

        public CustomerService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            try
            {
                return await _mongoDbService.Customers.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<Customer?> GetCustomerByIdAsync(ObjectId id)
        {
            try
            {
                return await _mongoDbService.Customers.Find(c => c.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving customer: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            try
            {
                var filter = Builders<Customer>.Filter.Or(
                    Builders<Customer>.Filter.Regex(c => c.FirstName, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Customer>.Filter.Regex(c => c.LastName, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Customer>.Filter.Regex(c => c.Email, new BsonRegularExpression(searchTerm, "i")),
                    Builders<Customer>.Filter.Regex(c => c.Phone, new BsonRegularExpression(searchTerm, "i"))
                );

                return await _mongoDbService.Customers.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<ObjectId> AddCustomerAsync(Customer customer)
        {
            try
            {
                customer.Id = ObjectId.GenerateNewId();
                customer.CreatedAt = DateTime.UtcNow;
                customer.LastModified = DateTime.UtcNow;
                await _mongoDbService.Customers.InsertOneAsync(customer);
                return customer.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding customer: {ex.Message}");
                return ObjectId.Empty;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.LastModified = DateTime.UtcNow;
                var result = await _mongoDbService.Customers.ReplaceOneAsync(c => c.Id == customer.Id, customer);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating customer: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(ObjectId id)
        {
            try
            {
                var result = await _mongoDbService.Customers.DeleteOneAsync(c => c.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting customer: {ex.Message}");
                return false;
            }
        }
    }
}
