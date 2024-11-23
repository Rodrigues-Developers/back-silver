using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Models {
  public class Product {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] // Map MongoDB ObjectId to C# string
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("availability")]
    public bool Availability { get; set; }

    [BsonElement("price")]
    public int Price { get; set; }

    [BsonElement("category")]
    public required List<string> Category { get; set; }
  }
}
