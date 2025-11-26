using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class MongoDbService
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
        public IMongoCollection<Device> Devices => _database.GetCollection<Device>("Devices");
        public IMongoCollection<WorkOrder> WorkOrders => _database.GetCollection<WorkOrder>("WorkOrders");

        public MongoDbService(string connectionString = "mongodb://localhost:27017", string databaseName = "WorkOrderManagementDB")
        {
            try
            {
                _client = new MongoClient(connectionString);
                _database = _client.GetDatabase(databaseName);
                InitializeCollections();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to MongoDB: {ex.Message}", ex);
            }
        }

        private void InitializeCollections()
        {
            // Create collections if they don't exist
            var collections = _database.ListCollectionNames().ToList();

            if (!collections.Contains("Users"))
                _database.CreateCollection("Users");

            if (!collections.Contains("Customers"))
                _database.CreateCollection("Customers");

            if (!collections.Contains("Devices"))
                _database.CreateCollection("Devices");

            if (!collections.Contains("WorkOrders"))
                _database.CreateCollection("WorkOrders");

            // Create indexes
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // Users indexes
            var userIndexOptions = new CreateIndexOptions { Unique = true };
            Users.Indexes.CreateOne(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username), userIndexOptions));

            // Customers indexes
            Customers.Indexes.CreateOne(new CreateIndexModel<Customer>(
                Builders<Customer>.IndexKeys.Ascending(c => c.Email)));
            Customers.Indexes.CreateOne(new CreateIndexModel<Customer>(
                Builders<Customer>.IndexKeys.Ascending(c => c.Phone)));

            // Devices indexes
            Devices.Indexes.CreateOne(new CreateIndexModel<Device>(
                Builders<Device>.IndexKeys.Ascending(d => d.CustomerId)));
            Devices.Indexes.CreateOne(new CreateIndexModel<Device>(
                Builders<Device>.IndexKeys.Ascending(d => d.SerialNumber)));

            // WorkOrders indexes
            WorkOrders.Indexes.CreateOne(new CreateIndexModel<WorkOrder>(
                Builders<WorkOrder>.IndexKeys.Ascending(w => w.CustomerId)));
            WorkOrders.Indexes.CreateOne(new CreateIndexModel<WorkOrder>(
                Builders<WorkOrder>.IndexKeys.Ascending(w => w.TechnicianId)));
            WorkOrders.Indexes.CreateOne(new CreateIndexModel<WorkOrder>(
                Builders<WorkOrder>.IndexKeys.Ascending(w => w.Status)));
            WorkOrders.Indexes.CreateOne(new CreateIndexModel<WorkOrder>(
                Builders<WorkOrder>.IndexKeys.Ascending(w => w.WorkOrderNumber)));
        }
    }
}
