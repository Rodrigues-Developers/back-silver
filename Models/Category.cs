using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Models {
  public class Category {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; } = string.Empty;

    [BsonElement("image")]
    public string? Image { get; set; }
  }
}

