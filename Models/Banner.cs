using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Models {
  public class Banner {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
  }
}

