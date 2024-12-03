using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase {
    private readonly ProductService _productService;

    public ProductController(ProductService productService) {
      _productService = productService;
    }

    [HttpGet]
    [Authorize] // This secures the endpoint
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
    [Authorize]
    public async Task<IActionResult> Create(Product product) {
      await _productService.CreateAsync(product);
      return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    // Token is already generated once the user logs in. So we don't need this post request
    // [HttpPost("auth/token")]
    // [AllowAnonymous] // Typically, requesting a token doesn't require prior authentication.
    // public async Task<IActionResult> RequestAuthToken() {
    //   try {
    //     var token = await _productService.RequestAuthTokenAsync();
    //     return Ok(new { Token = token });
    //   } catch (Exception ex) {
    //     return StatusCode(500, new { Error = ex.Message });
    //   }
    // }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(string id, Product updatedProduct) {
      var product = await _productService.GetByIdAsync(id);

      if (product == null)
        return NotFound();

      await _productService.UpdateAsync(id, updatedProduct);

      return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id) {
      var product = await _productService.GetByIdAsync(id);

      if (product == null)
        return NotFound();

      await _productService.DeleteAsync(id);

      return NoContent();
    }
  }
}
