using MongoDB.Driver;
using Newtonsoft.Json;
using YourProject.Models;

public class ProductService {
  private readonly IMongoCollection<Product> _products;

  private readonly string authClientID = Environment.GetEnvironmentVariable("AUTH_CLIENT_ID");
  private readonly string authClientSecret = Environment.GetEnvironmentVariable("AUTH_CLIENT_SECRET");
  private readonly string authServiceTokenURL = Environment.GetEnvironmentVariable("AUTH_SERVICE_TOKEN_URL");
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

  public async Task<string> RequestAuthTokenAsync() {
    using var httpClient = new HttpClient();
    var request = new HttpRequestMessage(HttpMethod.Post, authServiceTokenURL) {
      Content = new FormUrlEncodedContent(new Dictionary<string, string> {
        { "audience", "silver-backend-ident" },
        { "grant_type", "client_credentials" },
        { "client_id", authClientID },
        { "client_secret", authClientSecret }
      })
    };

    var response = await httpClient.SendAsync(request);

    if (!response.IsSuccessStatusCode) {
      throw new Exception($"Failed to get auth token. Status: {response.StatusCode}");
    }

    var responseContent = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

    return tokenResponse?.AccessToken;
  }

  public async Task<List<Product>> GetAsync() =>
      await _products.Find(_ => true).ToListAsync();

  public async Task<Product?> GetByIdAsync(string id) =>
      await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

  public async Task CreateAsync(Product product) =>
      await _products.InsertOneAsync(product);

  public async Task UpdateAsync(string id, Product updatedProduct) =>
      await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);

  public async Task DeleteAsync(string id) =>
      await _products.DeleteOneAsync(p => p.Id == id);

}
