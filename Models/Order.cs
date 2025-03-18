using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Models {
  public class Order {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("cartItems")]
    public required List<CartItem> CartItems { get; set; }

    [BsonElement("status")]
    public string? Status { get; set; } = "Pending";

    [BsonElement("createdAt")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;


    [BsonElement("total")]
    public int Total { get; set; }
  }

  public class CartItem {

    [BsonElement("amount")]
    public int Amount { get; set; }

    [BsonElement("url")]
    public string Url { get; set; }

    [BsonElement("product")]
    public required Product Product { get; set; }
  }

}
