using System.Net;
using System.Text.Json;

namespace Test.Core.Integration.Helpers;

public class ApiResponse<T> : ApiResponse
{
    public new T? Content { get; set; }
}

public class ApiResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
    public string? Error { get; set; }
    public bool IsSuccess => StatusCode >= HttpStatusCode.OK && StatusCode < HttpStatusCode.MultipleChoices;
    
    public string? GetValidationErrorMessage()
    {
        if (string.IsNullOrEmpty(Error))
            return null;
        
        try
        {
            using var doc = JsonDocument.Parse(Error);
            if (doc.RootElement.TryGetProperty("errors", out var errorsElement))
            {
                // errors - это объект с именами полей (FluentValidation)
                foreach (var property in errorsElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array && 
                        property.Value.GetArrayLength() > 0)
                    {
                        return property.Value[0].GetString();
                    }
                }
            }
            
            // Пробуем получить message напрямую (стандартный формат)
            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
            
            // Пробуем получить error (формат контроллера при удалении)
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString();
            }
        }
        catch
        {
            // Если не удалось распарсить, возвращаем null
        }
        
        return null;
    }
}
