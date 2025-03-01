using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using MongoDB.Bson;
using FirebaseAdmin.Auth;

namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class BannerController : ControllerBase {
    private readonly BannerService _bannerService;

    public BannerController(BannerService bannerService) {
      _bannerService = bannerService;
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
    public async Task<ActionResult<List<Banner>>> Get() {
      return await _bannerService.GetAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Banner>> Get(string id) {
      var banner = await _bannerService.GetByIdAsync(id);

      if (banner == null)
        return NotFound();

      return banner;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Banner banner) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can create)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to create a banner.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);
        await _bannerService.CreateAsync(banner);
        return CreatedAtAction(nameof(Get), new { id = banner.Id }, banner);
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Banner updatedBanner) {
      // Check for token presence and validate
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();

      // Verify the user's authorization (only luxoemprata1@gmail.com can update)
      if (!await IsUserAuthorized(token)) {
        return Unauthorized("You are not authorized to update this banner.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var existingBanner = await _bannerService.GetByIdAsync(id);
        if (existingBanner == null)
          return NotFound();

        if (string.IsNullOrEmpty(updatedBanner.Image)) {
          updatedBanner.Image = existingBanner.Image;
        }
        updatedBanner.Id = existingBanner.Id;

        await _bannerService.UpdateAsync(id, updatedBanner);
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
        return Unauthorized("You are not authorized to delete this banner.");
      }

      try {
        // Verify Firebase token
        await VerifyFirebaseToken(token);

        var banner = await _bannerService.GetByIdAsync(id);
        if (banner == null)
          return NotFound();

        await _bannerService.DeleteAsync(id);
        return NoContent();
      } catch (Exception ex) {
        return Unauthorized($"Invalid token: {ex.Message}");
      }
    }
  }
}