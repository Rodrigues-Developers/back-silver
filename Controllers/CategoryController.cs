using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using MongoDB.Bson;
using FirebaseAdmin.Auth;


namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class CategoryController : ControllerBase {
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService) {
      _categoryService = categoryService;
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
        return firebaseToken.Claims.TryGetValue("email", out object? email) && email != null && email.ToString() == Environment.GetEnvironmentVariable("USER_EMAIL");
      } catch (Exception) {
        return false; // Return false if token verification fails
      }
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> Get() {
      return await _categoryService.GetAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> Get(string id) {
      var category = await _categoryService.GetByIdAsync(id);

      if (category == null)
        return NotFound();

      return category;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Category category) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can create)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to create a category.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);
        await _categoryService.CreateAsync(category);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Category updatedCategory) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can update)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to update this category.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var existingCategory = await _categoryService.GetByIdAsync(id);
        if (existingCategory == null)
          return NotFound();

        if (string.IsNullOrEmpty(updatedCategory.Image)) {
          updatedCategory.Image = existingCategory.Image;
        }
        updatedCategory.Id = existingCategory.Id;

        await _categoryService.UpdateAsync(id, updatedCategory);
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
        return Unauthorized("You are not authorized to delete this category.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var category = await _categoryService.GetByIdAsync(id);
        if (category == null)
          return NotFound();

        await _categoryService.DeleteAsync(id);
        return NoContent();
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }
  }
}
