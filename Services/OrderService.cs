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
}
