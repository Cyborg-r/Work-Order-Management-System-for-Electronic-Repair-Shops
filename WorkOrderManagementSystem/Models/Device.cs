using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkOrderManagementSystem.Models
{
    [BsonIgnoreExtraElements]
    public class Device
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("customerId")]
        public ObjectId CustomerId { get; set; }

        [BsonElement("deviceType")]
        public string DeviceType { get; set; } = string.Empty; // Laptop, Smartphone, Printer, etc.

        [BsonElement("brand")]
        public string Brand { get; set; } = string.Empty;

        [BsonElement("serialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
