using MongoDB.Driver;
using MongoDB.Bson;
using YourProject.Models;

public class BannerService {
  private readonly IMongoCollection<Banner> _banners;

  public BannerService() {
    try {
      // Retrieve values from environment variables with BANNER_ prefix
      var mongoUsername = Environment.GetEnvironmentVariable("MONGO_USERNAME");
      var mongoPassword = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
      var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE");
      var mongoCollection = Environment.GetEnvironmentVariable("BANNER_MONGO_COLLECTION");
      var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

      // Build MongoDB connection string
      var connectionString = $"mongodb://{mongoUsername}:{mongoPassword}{mongoConnectionString}";

      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(mongoDatabase);
      _banners = database.GetCollection<Banner>(mongoCollection);

      Console.WriteLine("Connected to MongoDB (Banner Collection) successfully!");
    } catch (Exception ex) {
      Console.WriteLine($"Failed to connect to MongoDB (Banner Collection): {ex.Message}");
    }
  }

  public async Task<List<Banner>> GetAsync() =>
      await _banners.Find(_ => true).ToListAsync();

  public async Task<Banner?> GetByIdAsync(string id) =>
      await _banners.Find(b => b.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Banner banner) {
    if (string.IsNullOrEmpty(banner.Id)) {
      banner.Id = ObjectId.GenerateNewId().ToString();
    }
    await _banners.InsertOneAsync(banner);
  }

  public async Task UpdateAsync(string id, Banner updatedBanner) =>
      await _banners.ReplaceOneAsync(b => b.Id == id, updatedBanner);

  public async Task DeleteAsync(string id) =>
      await _banners.DeleteOneAsync(b => b.Id == id);
}