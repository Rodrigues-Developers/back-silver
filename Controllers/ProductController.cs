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
    public async Task<ActionResult<List<Product>>> Get() {
      return await _productService.GetAsync();
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

        updatedProduct.Id = id;  // Ensure the correct product Id is retained

        var product = await _productService.GetByIdAsync(id);
        if (product == null)
          return NotFound();

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
  }

}
