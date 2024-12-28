using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Models {
  public class Product {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] // Map MongoDB ObjectId to C# string
    public string? Id { get; set; } // Nullable to let MongoDB generate the Id

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("availability")]
    public bool Availability { get; set; }

    [BsonElement("price")]
    public float Price { get; set; }

    [BsonElement("category")]
    public required List<string> Category { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
  }
}
