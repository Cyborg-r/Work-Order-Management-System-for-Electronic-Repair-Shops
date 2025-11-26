using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkOrderManagementSystem.Models
{
    [BsonIgnoreExtraElements]
    public class WorkOrder
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("workOrderNumber")]
        public string WorkOrderNumber { get; set; } = string.Empty;

        [BsonElement("customerId")]
        public ObjectId CustomerId { get; set; }

        [BsonElement("deviceId")]
        public ObjectId DeviceId { get; set; }

        [BsonElement("technicianId")]
        public ObjectId? TechnicianId { get; set; }

        [BsonElement("issueDescription")]
        public string IssueDescription { get; set; } = string.Empty;

        [BsonElement("partsRequired")]
        public string PartsRequired { get; set; } = string.Empty;

        [BsonElement("laborCost")]
        public decimal LaborCost { get; set; }

        [BsonElement("partsCost")]
        public decimal PartsCost { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, On Hold

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("startedAt")]
        public DateTime? StartedAt { get; set; }

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        // Calculated property - not stored in database
        public decimal TotalCost => LaborCost + PartsCost;

        public TimeSpan? Turnaround => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : null;
    }
}
