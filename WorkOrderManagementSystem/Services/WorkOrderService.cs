using MongoDB.Bson;
using MongoDB.Driver;
using WorkOrderManagementSystem.Models;

namespace WorkOrderManagementSystem.Services
{
    public class WorkOrderService
    {
        private readonly MongoDbService _mongoDbService;
        private int _workOrderCounter = 1000;

        public WorkOrderService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task<List<WorkOrder>> GetAllWorkOrdersAsync()
        {
            try
            {
                return await _mongoDbService.WorkOrders.Find(_ => true).SortByDescending(w => w.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving work orders: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<List<WorkOrder>> GetActiveWorkOrdersAsync()
        {
            try
            {
                var oneDayAgo = DateTime.UtcNow.AddDays(-1);
                
                // Get all work orders that are either:
                // 1. Not completed
                // 2. Completed but within the last 1 day
                var filter = Builders<WorkOrder>.Filter.Or(
                    Builders<WorkOrder>.Filter.Ne(w => w.Status, "Completed"),
                    Builders<WorkOrder>.Filter.And(
                        Builders<WorkOrder>.Filter.Eq(w => w.Status, "Completed"),
                        Builders<WorkOrder>.Filter.Gte(w => w.CompletedAt, oneDayAgo)
                    )
                );

                return await _mongoDbService.WorkOrders
                    .Find(filter)
                    .SortByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving active work orders: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<WorkOrder?> GetWorkOrderByIdAsync(ObjectId id)
        {
            try
            {
                return await _mongoDbService.WorkOrders.Find(w => w.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving work order: {ex.Message}");
                return null;
            }
        }

        public async Task<List<WorkOrder>> GetWorkOrdersByCustomerAsync(ObjectId customerId)
        {
            try
            {
                return await _mongoDbService.WorkOrders
                    .Find(w => w.CustomerId == customerId)
                    .SortByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving work orders by customer: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<List<WorkOrder>> GetWorkOrdersByTechnicianAsync(ObjectId technicianId)
        {
            try
            {
                return await _mongoDbService.WorkOrders
                    .Find(w => w.TechnicianId == technicianId)
                    .SortByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving work orders by technician: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<List<WorkOrder>> GetWorkOrdersByStatusAsync(string status)
        {
            try
            {
                return await _mongoDbService.WorkOrders
                    .Find(w => w.Status == status)
                    .SortByDescending(w => w.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving work orders by status: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<List<WorkOrder>> GetActiveWorkOrdersByStatusAsync(string status)
        {
            try
            {
                var oneDayAgo = DateTime.UtcNow.AddDays(-1);
                
                if (status == "Completed")
                {
                    // For completed status, only show those within the last 1 day
                    return await _mongoDbService.WorkOrders
                        .Find(w => w.Status == status && w.CompletedAt >= oneDayAgo)
                        .SortByDescending(w => w.CreatedAt)
                        .ToListAsync();
                }
                else
                {
                    // For other statuses, show all
                    return await _mongoDbService.WorkOrders
                        .Find(w => w.Status == status)
                        .SortByDescending(w => w.CreatedAt)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving active work orders by status: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<List<WorkOrder>> SearchWorkOrdersAsync(string searchTerm)
        {
            try
            {
                var filter = Builders<WorkOrder>.Filter.Or(
                    Builders<WorkOrder>.Filter.Regex(w => w.WorkOrderNumber, new BsonRegularExpression(searchTerm, "i")),
                    Builders<WorkOrder>.Filter.Regex(w => w.IssueDescription, new BsonRegularExpression(searchTerm, "i"))
                );

                return await _mongoDbService.WorkOrders.Find(filter).SortByDescending(w => w.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching work orders: {ex.Message}");
                return new List<WorkOrder>();
            }
        }

        public async Task<ObjectId> AddWorkOrderAsync(WorkOrder workOrder)
        {
            try
            {
                workOrder.Id = ObjectId.GenerateNewId();
                workOrder.WorkOrderNumber = $"WO-{DateTime.Now:yyyyMMdd}-{++_workOrderCounter}";
                workOrder.CreatedAt = DateTime.UtcNow;
                await _mongoDbService.WorkOrders.InsertOneAsync(workOrder);
                return workOrder.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding work order: {ex.Message}");
                return ObjectId.Empty;
            }
        }

        public async Task<bool> UpdateWorkOrderAsync(WorkOrder workOrder)
        {
            try
            {
                var result = await _mongoDbService.WorkOrders.ReplaceOneAsync(w => w.Id == workOrder.Id, workOrder);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating work order: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateWorkOrderStatusAsync(ObjectId id, string status)
        {
            try
            {
                var update = Builders<WorkOrder>.Update.Set(w => w.Status, status);

                if (status == "In Progress")
                    update = update.Set(w => w.StartedAt, DateTime.UtcNow);

                if (status == "Completed")
                    update = update.Set(w => w.CompletedAt, DateTime.UtcNow);

                var result = await _mongoDbService.WorkOrders.UpdateOneAsync(w => w.Id == id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating work order status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AssignTechnicianAsync(ObjectId workOrderId, ObjectId technicianId)
        {
            try
            {
                var update = Builders<WorkOrder>.Update.Set(w => w.TechnicianId, technicianId);
                var result = await _mongoDbService.WorkOrders.UpdateOneAsync(w => w.Id == workOrderId, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error assigning technician: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteWorkOrderAsync(ObjectId id)
        {
            try
            {
                var result = await _mongoDbService.WorkOrders.DeleteOneAsync(w => w.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting work order: {ex.Message}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetAnalyticsAsync()
        {
            try
            {
                var allWorkOrders = await GetAllWorkOrdersAsync();
                var completedOrders = allWorkOrders.Where(w => w.Status == "Completed").ToList();

                var analytics = new Dictionary<string, object>
                {
                    { "TotalWorkOrders", allWorkOrders.Count },
                    { "CompletedOrders", completedOrders.Count },
                    { "PendingOrders", allWorkOrders.Count(w => w.Status == "Pending") },
                    { "InProgressOrders", allWorkOrders.Count(w => w.Status == "In Progress") },
                    { "TotalRevenue", completedOrders.Sum(w => w.TotalCost) },
                    { "AverageTurnaroundTime", completedOrders.Any() ? TimeSpan.FromSeconds(completedOrders.Average(w => (w.CompletedAt - w.CreatedAt)?.TotalSeconds ?? 0)) : TimeSpan.Zero },
                    { "CompletionRate", allWorkOrders.Any() ? (completedOrders.Count / (double)allWorkOrders.Count) * 100 : 0 }
                };

                return analytics;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating analytics: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }
    }
}
