using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Testing_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository productRepository) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }
}