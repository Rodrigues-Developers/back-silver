using MongoDB.Driver;
using MongoDB.Bson;
using YourProject.Models;

public class OrderService {
  private readonly IMongoCollection<Order> _orders;

  public OrderService() {
    try {
      var mongoUsername = Environment.GetEnvironmentVariable("MONGO_USERNAME");
      var mongoPassword = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
      var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE");
      var mongoCollection = Environment.GetEnvironmentVariable("ORDER_MONGO_COLLECTION");
      var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

      var connectionString = $"mongodb://{mongoUsername}:{mongoPassword}{mongoConnectionString}";

      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(mongoDatabase);
      _orders = database.GetCollection<Order>(mongoCollection);

      Console.WriteLine("Connected to MongoDB (Order Collection) successfully!");
    } catch (Exception ex) {
      Console.WriteLine($"Failed to connect to MongoDB (Order Collection): {ex.Message}");
    }
  }

  public async Task<List<Order>> GetAsync() =>
      await _orders.Find(_ => true).ToListAsync();

  public async Task<List<Order>> GetByUserIdAsync(string userId) =>
      await _orders.Find(o => o.UserId == userId).ToListAsync();

  public async Task<Order?> GetByIdAsync(string id) =>
      await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Order order) {
    if (string.IsNullOrEmpty(order.Id)) {
      order.Id = ObjectId.GenerateNewId().ToString();
    }
    await _orders.InsertOneAsync(order);
  }

  public async Task UpdateAsync(string id, Order updatedOrder) =>
      await _orders.ReplaceOneAsync(o => o.Id == id, updatedOrder);

  public async Task DeleteAsync(string id) =>
      await _orders.DeleteOneAsync(o => o.Id == id);


  public async Task<List<BsonDocument>> GetTopSellingProducts(int limit = 10) {
    var pipeline = new[]
    {
        new BsonDocument("$unwind", "$cartItems"), // Flatten cartItems array
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$cartItems.product._id" }, // Group by product ID
            { "name", new BsonDocument("$first", "$cartItems.product.name") }, // Get product name
            { "image", new BsonDocument("$first", "$cartItems.product.image") }, // Get product image
            { "description", new BsonDocument("$first", "$cartItems.product.description") },
            { "availability", new BsonDocument("$first", "$cartItems.product.availability") },
            { "discount", new BsonDocument("$first", "$cartItems.product.discount") },
            { "price", new BsonDocument("$first", "$cartItems.product.price") },
            { "totalSold", new BsonDocument("$sum", "$cartItems.amount") } // Ensure sum of all amounts
        }),
        new BsonDocument("$sort", new BsonDocument("totalSold", -1)), // Sort by most sold
        new BsonDocument("$limit", limit), // Limit results
        new BsonDocument("$project", new BsonDocument // Return only necessary fields
        {
            { "_id", new BsonDocument("$toString", "$_id") },
            { "name", 1 },
            { "image", 1 },
            { "price", 1 },
            { "description", 1 },
            { "availability", 1 },
            { "discount", 1 },
            { "totalSold", 1 },
        })
    };

    var result = await _orders.Aggregate<BsonDocument>(pipeline).ToListAsync();
    return result;
  }


}
