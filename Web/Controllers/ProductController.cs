using AutoMapper;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Testing_project.Dtos;
using Testing_project.Extensions;

namespace Testing_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductService productService,
    IProductRepository productRepository,
    IMapper mapper) : ControllerBase
{
    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] ProductCategory? category,
        [FromQuery] CookingRequirement? cookingNeeded,
        [FromQuery] List<ExtraFlag>? flags,
        [FromQuery] string? sort = "name",
        [FromQuery] bool ascending = true)
    {
        var query = await productRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (category.HasValue)
            query = query.Where(p => p.Category == category.Value);

        if (cookingNeeded.HasValue)
            query = query.Where(p => p.CookingRequirement == cookingNeeded.Value);

        if (flags?.Any() == true)
            query = query.Where(p => flags.All(f => p.Flags.HasFlag(f)));

        var sortedList = sort?.ToLower() switch
        {
            "calories" => ascending ? query.OrderBy(p => p.CaloriesPer100g) : query.OrderByDescending(p => p.CaloriesPer100g),
            "proteins" => ascending ? query.OrderBy(p => p.ProteinsPer100g) : query.OrderByDescending(p => p.ProteinsPer100g),
            "fats" => ascending ? query.OrderBy(p => p.FatsPer100g) : query.OrderByDescending(p => p.FatsPer100g),
            "carbs" => ascending ? query.OrderBy(p => p.CarbsPer100g) : query.OrderByDescending(p => p.CarbsPer100g),
            _ => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name)
        };

        var dtos = mapper.Map<IEnumerable<ProductDto>>(sortedList);
        return Ok(dtos);
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return NotFound();
        var dto = mapper.Map<ProductDto>(product);
        return Ok(dto);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = mapper.Map<Product>(createDto);
        product.Photos ??= new List<string>();

        var created = await productService.CreateProductAsync(product);
        var resultDto = mapper.Map<ProductDto>(created);

        return CreatedAtAction(nameof(GetProduct), new { id = resultDto.Id }, resultDto);
    }

    // PATCH: api/products/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        var product = await productRepository.GetByIdAsync(id);
        if (product == null) 
            return NotFound();

        // Вся магия здесь, одна строка
        product.ApplyUpdate(updateDto);

        await productService.UpdateProductAsync(product);
        return NoContent();
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await productService.DeleteProductAsync(id);
        if (!result)
            return BadRequest(new { error = result });

        return NoContent();
    }
}