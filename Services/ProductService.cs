using MongoDB.Driver;
using Newtonsoft.Json;
using YourProject.Models;
using MongoDB.Bson;

public class ProductService {
  private readonly IMongoCollection<Product> _products;

  public ProductService(IConfiguration config) {
    try {
      // Retrieve values from environment variables
      var mongoUsername = Environment.GetEnvironmentVariable("MONGO_USERNAME");
      var mongoPassword = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
      var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE");
      var mongoCollection = Environment.GetEnvironmentVariable("MONGO_COLLECTION");
      var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

      // Build MongoDB connection string using environment variables
      var connectionString = $"mongodb://{mongoUsername}:{mongoPassword}{mongoConnectionString}";

      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(mongoDatabase);
      _products = database.GetCollection<Product>(mongoCollection);

      Console.WriteLine("Connected to MongoDB successfully!");
    } catch (Exception ex) {
      Console.WriteLine($"Failed to connect to MongoDB: {ex.Message}");
    }
  }

  public async Task<List<Product>> GetAsync(string? sortBy = null, bool ascending = false) {
    var filter = Builders<Product>.Filter.Empty;

    if (!string.IsNullOrWhiteSpace(sortBy)) {
      var sortDefinitionBuilder = Builders<Product>.Sort;

      var sort = ascending ? sortDefinitionBuilder.Ascending(sortBy) : sortDefinitionBuilder.Descending(sortBy);

      return await _products.Find(filter).Sort(sort).ToListAsync();
    }

    return await _products.Find(filter).ToListAsync();
  }


  public async Task<Product?> GetByIdAsync(string id) =>
      await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Product product) {
    if (string.IsNullOrEmpty(product.Id)) {
      product.Id = ObjectId.GenerateNewId().ToString();
    }
    await _products.InsertOneAsync(product);
  }

  public async Task UpdateAsync(string id, Product updatedProduct) =>
      await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);

  public async Task DeleteAsync(string id) =>
      await _products.DeleteOneAsync(p => p.Id == id);

  public async Task<bool> AnyProductHasCategoryAsync(string categoryID) {
    var filter = Builders<Product>.Filter.AnyIn(p => p.Category, new List<string> { categoryID });
    return await _products.Find(filter).AnyAsync();
  }

  public async Task<List<Product>> GetProductsByCategory(string categoryID) {
    var filter = Builders<Product>.Filter.AnyIn(p => p.Category, new List<string> { categoryID });
    return await _products.Find(filter).ToListAsync();
  }

  public async Task<List<Product>> SearchByName(string name) {
    var filter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(name, "i"));
    return await _products.Find(filter).ToListAsync();
  }

  public async Task<int> GetProductCountByCategory(string categoryID) {
    var filter = Builders<Product>.Filter.AnyIn(p => p.Category, new List<string> { categoryID });
    long count = await _products.CountDocumentsAsync(filter);
    return (int)count;
  }
  public async Task<List<Product>> GetProductsByHighestDiscount() {
    var sort = Builders<Product>.Sort.Descending(p => p.Discount);
    return await _products.Find(_ => true).Sort(sort).ToListAsync();
  }
}
