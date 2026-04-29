using Test.Core.Integration.Fixtures;

namespace Test.Core.Integration.Collections;

/// <summary>
/// Коллекция для интеграционных тестов в НЕИЗОЛИРОВАННОЙ среде.
/// Все тесты используют ОДНУ реальную БД.
/// Данные НЕ очищаются между тестами - тесты могут влиять друг на друга!
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<ApiTestFixture>
{
    // Этот класс не содержит кода, он просто определяет коллекцию
}