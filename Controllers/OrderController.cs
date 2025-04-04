using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using FirebaseAdmin.Auth;

namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class OrderController : ControllerBase {
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService) {
      _orderService = orderService;
    }

    [HttpPost("verify-token")]
    public async Task<FirebaseToken> VerifyFirebaseToken(string token) {
      return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
    }

    private async Task<bool> IsUserAuthorized(string token, string userId) {
      try {
        FirebaseToken firebaseToken = await VerifyFirebaseToken(token);

        // Check if user is admin
        if (firebaseToken.Claims.TryGetValue("email", out object? email) &&
            email != null && email.ToString() == Environment.GetEnvironmentVariable("USER_EMAIL")) {
          return true; // Admin access
        }

        // Check if user owns the order
        return firebaseToken.Uid == userId;
      } catch (Exception) {
        return false;
      }
    }


    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll([FromHeader(Name = "Authorization")] string? bearerToken, [FromQuery] string? sortBy = null, [FromQuery] bool ascending = false) {
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();
      FirebaseToken firebaseToken = await VerifyFirebaseToken(token);
      string userId = firebaseToken.Uid;

      bool isAdmin = firebaseToken.Claims.TryGetValue("email", out object? email) &&
                     email != null && email.ToString() == Environment.GetEnvironmentVariable("USER_EMAIL");

      List<Order> orders;

      if (isAdmin) {
        // Admin sees all orders
        orders = await _orderService.GetAsync(sortBy, ascending);
      } else {
        // Regular user only sees their own orders
        orders = await _orderService.GetByUserIdAsync(userId, sortBy, ascending);
      }

      return Ok(orders);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> Get(string id, [FromHeader(Name = "Authorization")] string? bearerToken) {
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();
      var order = await _orderService.GetByIdAsync(id);

      if (order == null)
        return NotFound();

      if (!await IsUserAuthorized(token, order.UserId)) {
        return Unauthorized("You are not allowed to access this order.");
      }

      return order;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Order order) {
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();
      FirebaseToken firebaseToken = await VerifyFirebaseToken(token);
      order.UserId = firebaseToken.Uid;

      await _orderService.CreateAsync(order);
      return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromHeader(Name = "Authorization")] string? bearerToken, [FromBody] Order updatedOrder) {
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();
      var existingOrder = await _orderService.GetByIdAsync(id);

      if (existingOrder == null)
        return NotFound();

      if (!await IsUserAuthorized(token, existingOrder.UserId)) {
        return Unauthorized("You are not authorized to update this order.");
      }

      updatedOrder.Id = existingOrder.Id;
      await _orderService.UpdateAsync(id, updatedOrder);
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromHeader(Name = "Authorization")] string? bearerToken) {
      if (bearerToken == null || !bearerToken.StartsWith("Bearer ")) {
        return Unauthorized("No token provided");
      }

      string token = bearerToken.Substring("Bearer ".Length).Trim();
      var order = await _orderService.GetByIdAsync(id);

      if (order == null)
        return NotFound();

      if (!await IsUserAuthorized(token, order.UserId)) {
        return Unauthorized("You are not authorized to delete this order.");
      }

      await _orderService.DeleteAsync(id);
      return NoContent();
    }
    [HttpGet("top-selling")]
    public async Task<ActionResult<List<Dictionary<string, object>>>> GetTopSellingProducts(
     [FromQuery] int limit = 10) {

      var topSellingProducts = await _orderService.GetTopSellingProducts(limit);

      // Convert BsonDocument to Dictionary<string, object> for Swagger compatibility
      var formattedProducts = topSellingProducts.Select(doc => doc.ToDictionary()).ToList();

      return Ok(formattedProducts);
    }

  }
}
