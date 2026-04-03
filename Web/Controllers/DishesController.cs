using AutoMapper;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Testing_project.Dtos.Dish;

namespace Testing_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController(
    IDishService dishService,
    IDishRepository dishRepository,
    IMapper mapper) : ControllerBase
{
    // GET: api/dishes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DishDto>>> GetDishes(
        [FromQuery] string? search,
        [FromQuery] DishCategory? category,
        [FromQuery] List<ExtraFlag>? flags,
        [FromQuery] string? sort = "name",
        [FromQuery] bool ascending = true)
    {
        var query = await dishRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (category.HasValue && category != DishCategory.None)
            query = query.Where(d => d.Category == category.Value);

        if (flags?.Any() == true)
            query = query.Where(d => flags.All(f => d.Flags.HasFlag(f)));

        var sortedList = sort?.ToLower() switch
        {
            "calories" => ascending ? query.OrderBy(d => d.CaloriesPerServing) : query.OrderByDescending(d => d.CaloriesPerServing),
            "proteins" => ascending ? query.OrderBy(d => d.ProteinsPerServing) : query.OrderByDescending(d => d.ProteinsPerServing),
            "fats" => ascending ? query.OrderBy(d => d.FatsPerServing) : query.OrderByDescending(d => d.FatsPerServing),
            "carbs" => ascending ? query.OrderBy(d => d.CarbsPerServing) : query.OrderByDescending(d => d.CarbsPerServing),
            _ => ascending ? query.OrderBy(d => d.Name) : query.OrderByDescending(d => d.Name)
        };

        var dtos = mapper.Map<IEnumerable<DishDto>>(sortedList);
        return Ok(dtos);
    }

    // GET: api/dishes/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DishDto>> GetDish(int id)
    {
        var dish = await dishRepository.GetByIdAsync(id);
        if (dish == null) return NotFound();
        var dto = mapper.Map<DishDto>(dish);
        return Ok(dto);
    }

    // POST: api/dishes
    [HttpPost]
    public async Task<ActionResult<DishDto>> CreateDish([FromBody] CreateDishDto createDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var dish = mapper.Map<Dish>(createDto);
        dish.Photos ??= new List<string>();

        var created = await dishService.CreateDishAsync(dish);
        var resultDto = mapper.Map<DishDto>(created);

        return CreatedAtAction(nameof(GetDish), new { id = resultDto.Id }, resultDto);
    }

    // PATCH: api/dishes/{id}
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateDish(int id, [FromBody] UpdateDishDto updateDto)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        // 1. Получаем текущее блюдо
        var dish = await dishRepository.GetByIdAsync(id); // Предполагаем, что метод есть
        if (dish == null)
            return NotFound();

        // 2. Применяем обновления только для переданных полей
        // Передаем маппер, если он нужен внутри для ингредиентов
        dish.ApplyUpdate(updateDto, mapper);

        // 3. Сохраняем (сервис пересчитает КБЖУ и флаги)
        await dishService.UpdateDishAsync(dish);

        return NoContent();
    }

    // DELETE: api/dishes/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDish(int id)
    {
        var result = await dishService.DeleteDishAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}