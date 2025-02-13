using MongoDB.Driver;
using MongoDB.Bson;
using YourProject.Models;


public class CategoryService {
  private readonly IMongoCollection<Category> _categories;

  public CategoryService() {
    try {
      // Retrieve values from environment variables with CATEGORY_ prefix
      var mongoUsername = Environment.GetEnvironmentVariable("MONGO_USERNAME");
      var mongoPassword = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
      var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE");
      var mongoCollection = Environment.GetEnvironmentVariable("CATEGORY_MONGO_COLLECTION");
      var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

      // Build MongoDB connection string
      var connectionString = $"mongodb://{mongoUsername}:{mongoPassword}{mongoConnectionString}";

      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(mongoDatabase);
      _categories = database.GetCollection<Category>(mongoCollection);

      Console.WriteLine("Connected to MongoDB (Category Collection) successfully!");
    } catch (Exception ex) {
      Console.WriteLine($"Failed to connect to MongoDB (Category Collection): {ex.Message}");
    }
  }

  public async Task<List<Category>> GetAsync() =>
      await _categories.Find(_ => true).ToListAsync();

  public async Task<Category?> GetByIdAsync(string id) =>
      await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Category category) {
    if (string.IsNullOrEmpty(category.Id)) {
      category.Id = ObjectId.GenerateNewId().ToString();
    }
    await _categories.InsertOneAsync(category);
  }

  public async Task UpdateAsync(string id, Category updatedCategory) =>
      await _categories.ReplaceOneAsync(c => c.Id == id, updatedCategory);

  public async Task DeleteAsync(string id) =>
      await _categories.DeleteOneAsync(c => c.Id == id);
}
