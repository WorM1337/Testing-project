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