using Core.Interfaces;
using Core.Models;
using Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class DishRepository(BookOfReceiptsDbContext context) : IDishRepository
{
    public async Task<Dish?> GetByIdAsync(int id)
        => await context.Dishes
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Dish>> GetAllAsync()
        => await context.Dishes.ToListAsync();

    public async Task<Dish> CreateAsync(Dish dish)
    {
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();
        return dish;
    }

    public async Task UpdateAsync(Dish dish)
    {
        context.Dishes.Update(dish);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Dish dish)
    {
        context.Dishes.Remove(dish);
        await context.SaveChangesAsync();
    }
}