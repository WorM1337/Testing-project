using Test.Core.Integration.Fixtures;

namespace Test.Core.Integration;

/// <summary>
/// Базовый класс для интеграционных тестов в НЕИЗОЛИРОВАННОЙ среде.
/// НЕ очищает данные между тестами!
/// </summary>
[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ApiTestFixture Fixture;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(ApiTestFixture fixture)
    {
        Fixture = fixture;
        Client = fixture.CreateClient();
    }

    public virtual Task InitializeAsync()
    {
        // НЕ очищаем БД перед тестом
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        // НЕ очищаем БД после теста
        return Task.CompletedTask;
    }
}