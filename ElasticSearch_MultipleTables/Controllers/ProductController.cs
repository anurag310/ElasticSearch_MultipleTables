using ElasticSearch_MultipleTables.Models;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("index")]
    public async Task<IActionResult> IndexProducts()
    {
        var indexResults = await _productService.IndexProductsAsync();
        return Ok(indexResults);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
    {
        var results = await _productService.SearchProductsAsync(searchTerm);
        return Ok(results);
    }
}
