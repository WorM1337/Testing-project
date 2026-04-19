using AutoMapper;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// <summary>
    /// Parses comma-separated flags string into ExtraFlag enum
    /// </summary>
    private static ExtraFlag? ParseFlags(string? flagsString)
    {
        if (string.IsNullOrWhiteSpace(flagsString))
            return null;

        var flags = ExtraFlag.None;
        var parts = flagsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (Enum.TryParse<ExtraFlag>(part, true, out var flag))
            {
                flags |= flag;
            }
        }

        return flags == ExtraFlag.None ? null : flags;
    }
    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductQuery query)
    {
        // Модель-байндер сам заполнит query из строки запроса ?search=...&sort=Calories...
        var products = await productService.GetProductsAsync(query);
        var dtos = mapper.Map<IEnumerable<ProductDto>>(products);
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

        // Parse flags from string if provided
        ExtraFlag? parsedFlags = null;
        if (updateDto.Flags != null)
        {
            parsedFlags = ParseFlags(updateDto.Flags);
        }

        // Вся магия здесь, одна строка
        product.ApplyUpdate(updateDto, parsedFlags);

        await productService.UpdateProductAsync(product);
        return NoContent();
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await productService.DeleteProductAsync(id);
            if (!result)
                return BadRequest(new { error = result });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Product is used in dishes - return 400 with details
            return BadRequest(new { 
                error = ex.Message,
                type = "ProductInUse"
            });
        }
    }
}
