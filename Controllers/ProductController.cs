using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using MongoDB.Bson;
using FirebaseAdmin.Auth;

namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase {
    private readonly ProductService _productService;

    public ProductController(ProductService productService) {
      _productService = productService;
    }

    // Verifying Firebase ID Token
    [HttpPost("verify-token")]
    public async Task<FirebaseToken> VerifyFirebaseToken(string token) {
      return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
    }

    // Check if the user is the specific one (luxoemprata1@gmail.com)
    private async Task<bool> IsUserAuthorized(string token) {
      try {
        FirebaseToken firebaseToken = await VerifyFirebaseToken(token);
        return firebaseToken.Claims.TryGetValue("email", out object? email) && email != null && email.ToString() == Environment.GetEnvironmentVariable("USER_EMAIL"); ;
      } catch (Exception) {
        return false; // Return false if token verification fails
      }
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> Get([FromQuery] string? sortBy = null, [FromQuery] bool ascending = false) {
      var products = await _productService.GetAsync(sortBy, ascending);
      return Ok(products);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(string id) {
      var product = await _productService.GetByIdAsync(id);

      if (product == null)
        return NotFound();

      return product;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Product product) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can create)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to create a product.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);
        await _productService.CreateAsync(product);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Product updatedProduct) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can update)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to update this product.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var existingProduct = await _productService.GetByIdAsync(id);
        if (existingProduct == null)
          return NotFound();

        // ✅ Keep old image if no new image is provided
        if (string.IsNullOrEmpty(updatedProduct.Image)) {
          updatedProduct.Image = existingProduct.Image;
        }
        updatedProduct.Id = existingProduct.Id;

        await _productService.UpdateAsync(id, updatedProduct);
        return NoContent();
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromHeader(Name = "Authorization")] string? bearerToken) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can delete)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to delete this product.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var product = await _productService.GetByIdAsync(id);
        if (product == null)
          return NotFound();

        await _productService.DeleteAsync(id);
        return NoContent();
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }

    [HttpGet("has-category/{id}")]
    public async Task<ActionResult<bool>> HasCategory(string id) {
      bool exists = await _productService.AnyProductHasCategoryAsync(id);
      return Ok(exists);
    }

    [HttpGet("products-category/{id}")]
    public async Task<ActionResult<List<Product>>> GetProductsByCategory(string id) {
      return await _productService.GetProductsByCategory(id);
    }

    [HttpGet("product-search")]
    public async Task<ActionResult<List<Product>>> Search([FromQuery] string name) {
      var products = await _productService.SearchByName(name);
      return Ok(products);
    }

    [HttpGet("category-count/{id}")]
    public async Task<ActionResult<int>> GetProductCountByCategory(string id) {
      int count = await _productService.GetProductCountByCategory(id);
      return Ok(count);
    }

  }
}
