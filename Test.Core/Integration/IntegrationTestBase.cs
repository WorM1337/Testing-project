using System.Net;
using System.Net.Http.Json;
using Core.Models.Enums;
using Test.Core.Integration.Helpers;
using Testing_project.Dtos;
using Testing_project.Dtos.Dish;
using Testing_project.Dtos.Ingredient;

namespace Test.Core.Integration;

[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client = null!;
    private readonly string _baseUrl;
    
    private readonly List<int> _createdDishIds = new();
    
    private readonly List<int> _createdProductIds = new();

    protected IntegrationTestBase()
    {
        _baseUrl = "http://localhost:5099";
    }

    public virtual async Task InitializeAsync()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await DeleteTrackedEntitiesAsync();
        
        Client?.Dispose();
        await Task.CompletedTask;
    }
    

    private async Task DeleteTrackedEntitiesAsync()
    {
        var dishTasks = _createdDishIds.Select(id => 
            Client.DeleteAsync($"/api/dishes/{id}"));
        await Task.WhenAll(dishTasks);

        var productTasks = _createdProductIds.Select(id => 
            Client.DeleteAsync($"/api/products/{id}"));
        await Task.WhenAll(productTasks);
    }
    
    #region Helper Methods для создания продуктов
    
    protected async Task<ApiResponse<ProductDto>> CreateProductAsync(
        string name = "Тестовый продукт",
        double calories = 100,
        double proteins = 10,
        double fats = 5,
        double carbs = 15,
        ProductCategory category = ProductCategory.Vegetables,
        CookingRequirement cookingRequirement = CookingRequirement.ReadyToUse,
        ExtraFlag flags = ExtraFlag.None,
        string? composition = null,
        List<string>? photos = null)
    {
        var createDto = new CreateProductDto
        {
            Name = name,
            CaloriesPer100g = calories,
            ProteinsPer100g = proteins,
            FatsPer100g = fats,
            CarbsPer100g = carbs,
            Category = category,
            CookingRequirement = cookingRequirement,
            Flags = flags,
            Composition = composition,
            Photos = photos ?? new List<string>()
        };
        
        return await CreateProductAsync(createDto);
    }
    
    protected async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createDto)
    {
        var response = await Client.PostAsJsonAsync("/api/products", createDto, ApiHelpers.JsonOptions);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<ProductDto>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(ApiHelpers.JsonOptions);
        
        if (product != null)
        {
            _createdProductIds.Add(product.Id);
        }

        return new ApiResponse<ProductDto>
        {
            StatusCode = response.StatusCode,
            Content = product
        };
    }
    
    #endregion
    
    #region Helper Methods для создания блюд
    
    protected async Task<ApiResponse<DishDto>> CreateDishAsync(
        string name,
        List<CreateIngredientDto> ingredients,
        double? caloriesPerServing = null,
        double? proteinsPerServing = null,
        double? fatsPerServing = null,
        double? carbsPerServing = null,
        double? servingSize = null,
        List<string>? photos = null)
    {
        var createDto = new CreateDishDto
        {
            Name = name,
            Ingredients = ingredients,
            CaloriesPerServing = caloriesPerServing,
            ProteinsPerServing = proteinsPerServing,
            FatsPerServing = fatsPerServing,
            CarbsPerServing = carbsPerServing,
            ServingSize = servingSize,
            Photos = photos ?? new List<string>()
        };
        
        return await CreateDishAsync(createDto);
    }
    
    protected async Task<ApiResponse<DishDto>> CreateDishAsync(CreateDishDto createDto)
    {
        var response = await Client.PostAsJsonAsync("/api/dishes", createDto, ApiHelpers.JsonOptions);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<DishDto>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        var dish = await response.Content.ReadFromJsonAsync<DishDto>(ApiHelpers.JsonOptions);
        
        if (dish != null)
        {
            _createdDishIds.Add(dish.Id);
        }

        return new ApiResponse<DishDto>
        {
            StatusCode = response.StatusCode,
            Content = dish
        };
    }
    
    #endregion
    
    #region Helper Methods для получения данных
    
    protected async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string? query = null)
    {
        var url = string.IsNullOrEmpty(query) ? "/api/products" : $"/api/products{query}";
        var response = await Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<List<ProductDto>>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(ApiHelpers.JsonOptions);
        
        return new ApiResponse<List<ProductDto>>
        {
            StatusCode = response.StatusCode,
            Content = products ?? new List<ProductDto>()
        };
    }
    
    protected async Task<ApiResponse<ProductDto>> GetProductAsync(int id)
    {
        var response = await Client.GetAsync($"/api/products/{id}");
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<ProductDto>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(ApiHelpers.JsonOptions);
        
        return new ApiResponse<ProductDto>
        {
            StatusCode = response.StatusCode,
            Content = product
        };
    }
    
    protected async Task<ApiResponse<DishDto>> GetDishAsync(int id)
    {
        var response = await Client.GetAsync($"/api/dishes/{id}");
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<DishDto>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        var dish = await response.Content.ReadFromJsonAsync<DishDto>(ApiHelpers.JsonOptions);
        
        return new ApiResponse<DishDto>
        {
            StatusCode = response.StatusCode,
            Content = dish
        };
    }
    
    #endregion
    
    #region Helper Methods для обновления
    
    protected async Task<ApiResponse<DishDto>> UpdateDishAsync(int id, UpdateDishDto updateDto)
    {
        var response = await Client.PatchAsJsonAsync($"/api/dishes/{id}", updateDto, ApiHelpers.JsonOptions);
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResponse<DishDto>
            {
                StatusCode = response.StatusCode,
                Content = null,
                Error = content
            };
        }

        // 204 No Content не имеет тела ответа
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new ApiResponse<DishDto>
            {
                StatusCode = response.StatusCode,
                Content = null
            };
        }

        var dish = await response.Content.ReadFromJsonAsync<DishDto>(ApiHelpers.JsonOptions);
        
        return new ApiResponse<DishDto>
        {
            StatusCode = response.StatusCode,
            Content = dish
        };
    }
    
    #endregion
    
    #region Helper Methods для удаления
    
    protected async Task<ApiResponse> DeleteProductAsync(int id)
    {
        var response = await Client.DeleteAsync($"/api/products/{id}");
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            _createdProductIds.Remove(id);
            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                Content = content
            };
        }

        return new ApiResponse
        {
            StatusCode = response.StatusCode,
            Content = content,
            Error = content
        };
    }
    
    protected async Task<ApiResponse> DeleteDishAsync(int id)
    {
        var response = await Client.DeleteAsync($"/api/dishes/{id}");
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            _createdDishIds.Remove(id);
            return new ApiResponse
            {
                StatusCode = response.StatusCode,
                Content = content
            };
        }

        return new ApiResponse
        {
            StatusCode = response.StatusCode,
            Content = content,
            Error = content
        };
    }
    
    #endregion
}
