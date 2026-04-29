using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Test.Core.Integration.Helpers;

/// <summary>
/// Вспомогательные методы для работы с API в тестах
/// </summary>
public static class ApiHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Отправляет POST запрос и десериализует ответ
    /// </summary>
    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(
        this HttpClient client, 
        string url, 
        TRequest request)
    {
        var response = await client.PostAsJsonAsync(url, request, JsonOptions);
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    /// <summary>
    /// Отправляет POST запрос (без десериализации ответа)
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonAsync<TRequest>(
        this HttpClient client,
        string url,
        TRequest request)
    {
        return await client.PostAsJsonAsync(url, request, JsonOptions);
    }

    /// <summary>
    /// Отправляет PATCH запрос
    /// </summary>
    public static async Task<HttpResponseMessage> PatchAsync<TRequest>(
        this HttpClient client,
        string url,
        TRequest request)
    {
        return await client.PatchAsJsonAsync(url, request, JsonOptions);
    }

    /// <summary>
    /// Получает объект из ответа
    /// </summary>
    public static new async Task<T?> GetFromJsonAsync<T>(
        this HttpClient client,
        string url)
    {
        return await client.GetFromJsonAsync<T>(url, JsonOptions);
    }

    /// <summary>
    /// Проверяет, что ответ содержит ошибку валидации с определенным сообщением
    /// </summary>
    public static async Task ShouldHaveValidationError(
        this HttpResponseMessage response,
        string expectedMessage)
    {
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain(expectedMessage);
        }
    }
}