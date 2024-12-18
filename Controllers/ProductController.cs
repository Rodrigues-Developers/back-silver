using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;

namespace YourProject.Controllers {
  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase {
    private readonly ProductService _productService;

    public ProductController(ProductService productService) {
      _productService = productService;
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
    [Authorize]
    public async Task<IActionResult> Create([FromBody] Product product) {
      // Ensure MongoDB generates a unique Id if it's not provided in the request
      if (string.IsNullOrEmpty(product.Id)) {
        product.Id = ObjectId.GenerateNewId().ToString();
      }

      await _productService.CreateAsync(product);
      return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

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
