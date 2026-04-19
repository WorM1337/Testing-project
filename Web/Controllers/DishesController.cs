using AutoMapper;
using Core.Interfaces;
using Core.Models;
using Core.Models.Enums;
using Core.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Testing_project.Dtos.Dish;

namespace Testing_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController(
    IDishService dishService,
    IDishRepository dishRepository,
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
    // GET: api/dishes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DishDto>>> GetDishes([FromQuery] DishQuery query)
    {
        var dishes = await dishService.GetDishesAsync(query);
        var dtos = mapper.Map<IEnumerable<DishDto>>(dishes);
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

        try
        {
            var created = await dishService.CreateDishAsync(dish);
            var resultDto = mapper.Map<DishDto>(created);

            return CreatedAtAction(nameof(GetDish), new { id = resultDto.Id }, resultDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PATCH: api/dishes/{id}
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateDish(int id, [FromBody] UpdateDishDto updateDto)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        // 1. Получаем текущее блюдо
        var dish = await dishRepository.GetByIdAsync(id);
        if (dish == null)
            return NotFound();

        // Parse flags from string if provided
        ExtraFlag? parsedFlags = null;
        if (updateDto.Flags != null)
        {
            parsedFlags = ParseFlags(updateDto.Flags);
        }

        // 2. Применяем обновления только для переданных полей
        dish.ApplyUpdate(updateDto, mapper, parsedFlags);

        // 3. Определяем, была ли категория явно установлена через форму
        // Если updateDto.Category имеет значение (не null), значит категория была явно установлена
        bool categoryWasExplicitlySet = updateDto.Category.HasValue;

        // 4. Определяем, какие поля КБЖУ были явно установлены пользователем
        // Это нужно для того, чтобы сервис знал, какие значения пересчитывать, а какие оставить
        bool caloriesWasExplicitlySet = updateDto.CaloriesPerServing.HasValue;
        bool proteinsWasExplicitlySet = updateDto.ProteinsPerServing.HasValue;
        bool fatsWasExplicitlySet = updateDto.FatsPerServing.HasValue;
        bool carbsWasExplicitlySet = updateDto.CarbsPerServing.HasValue;
        bool servingSizeWasExplicitlySet = updateDto.ServingSize.HasValue;

        // 5. Сохраняем (сервис обработает макросы и пересчитает КБЖУ и флаги)
        try
        {
            await dishService.UpdateDishAsync(dish, 
                categoryWasExplicitlySet,
                caloriesWasExplicitlySet,
                proteinsWasExplicitlySet,
                fatsWasExplicitlySet,
                carbsWasExplicitlySet,
                servingSizeWasExplicitlySet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

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
