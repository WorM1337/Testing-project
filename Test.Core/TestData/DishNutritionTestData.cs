using Core.Models;

namespace Test.Core.TestData;

public static class DishNutritionTestData
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new List<DishNutritionTestCase>();

        #region Один продукт (Картошка) с разным количеством

        // Стандартная порция (100г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 100.0)
            },
            ExpectedCalories = 100.0,
            ExpectedProteins = 2.0,
            ExpectedFats = 0.5,
            ExpectedCarbs = 4.0,
            ExpectedServingSize = 100.0
        });

        // Эквиваленты: мало (50г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 50.0)
            },
            ExpectedCalories = 50.0,
            ExpectedProteins = 1.0,
            ExpectedFats = 0.25,
            ExpectedCarbs = 2.0,
            ExpectedServingSize = 50.0
        });

        // Эквиваленты: много (500г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 500.0)
            },
            ExpectedCalories = 500.0,
            ExpectedProteins = 10.0,
            ExpectedFats = 2.5,
            ExpectedCarbs = 20.0,
            ExpectedServingSize = 500.0
        });

        // Граница: пустое блюдо (0г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 0.0)
            },
            ExpectedCalories = 0.0,
            ExpectedProteins = 0.0,
            ExpectedFats = 0.0,
            ExpectedCarbs = 0.0,
            ExpectedServingSize = 0.0
        });

        #endregion

        #region Несколько продуктов с одинаковым весом

        // 2 продукта: Картошка + Курица
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 100.0),
                (ProductHelper.ChickenBreast, 100.0)
            },
            ExpectedCalories = 265.0,
            ExpectedProteins = 33.0,
            ExpectedFats = 4.1,
            ExpectedCarbs = 4.0,
            ExpectedServingSize = 200.0
        });

        // 3 продукта: Картошка + Курица + Масло
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 10.0),
                (ProductHelper.ChickenBreast, 10.0),
                (ProductHelper.OliveOil, 10.0)
            },
            ExpectedCalories = 116.50,
            ExpectedProteins = 3.30,
            ExpectedFats = 10.41,
            ExpectedCarbs = 0.4,
            ExpectedServingSize = 30.0
        });

        // 5 продуктов: Картошка + Курица + Масло + Рис + Томаты
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 100.0),
                (ProductHelper.ChickenBreast, 100.0),
                (ProductHelper.OliveOil, 100.0),
                (ProductHelper.Rice, 100.0),
                (ProductHelper.Tomato, 100.0)
            },
            ExpectedCalories = 1313.0,
            ExpectedProteins = 36.6,
            ExpectedFats = 104.6,
            ExpectedCarbs = 35.9,
            ExpectedServingSize = 500.0
        });

        // Граница: все продукты по 0г (пустое блюдо)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 0.0),
                (ProductHelper.ChickenBreast, 0.0),
                (ProductHelper.OliveOil, 0.0)
            },
            ExpectedCalories = 0.0,
            ExpectedProteins = 0.0,
            ExpectedFats = 0.0,
            ExpectedCarbs = 0.0,
            ExpectedServingSize = 0.0
        });

        #endregion

        #region Несколько продуктов с разным весом

        // Сложное блюдо с разными порциями (Картошка 200г + Курица 150г + Масло 10г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 200.0),
                (ProductHelper.ChickenBreast, 150.0),
                (ProductHelper.OliveOil, 10.0)
            },
            ExpectedCalories = 537.5,
            ExpectedProteins = 50.5,
            ExpectedFats = 16.4,
            ExpectedCarbs = 8.0,
            ExpectedServingSize = 360.0
        });

        // Большой приём пищи (Картошка 300г + Курица 200г + Рис 150г + Томаты 100г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 300.0),
                (ProductHelper.ChickenBreast, 200.0),
                (ProductHelper.Rice, 150.0),
                (ProductHelper.Tomato, 100.0)
            },
            ExpectedCalories = 843.0,
            ExpectedProteins = 72.95,
            ExpectedFats = 9.35,
            ExpectedCarbs = 57.9,
            ExpectedServingSize = 750.0
        });

        // Блюдо с доминированием одного продукта (Масло 50г + Курица 100г + Томаты 20г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.OliveOil, 50.0),
                (ProductHelper.ChickenBreast, 100.0),
                (ProductHelper.Tomato, 20.0)
            },
            ExpectedCalories = 618.6,
            ExpectedProteins = 31.18,
            ExpectedFats = 53.64,
            ExpectedCarbs = 0.78,
            ExpectedServingSize = 170.0
        });

        // Граница: один продукт с весом, остальные 0г (Картошка 100г + Курица 0г + Масло 0г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Potato, 100.0),
                (ProductHelper.ChickenBreast, 0.0),
                (ProductHelper.OliveOil, 0.0)
            },
            ExpectedCalories = 100.0,
            ExpectedProteins = 2.0,
            ExpectedFats = 0.5,
            ExpectedCarbs = 4.0,
            ExpectedServingSize = 100.0
        });

        #endregion

        #region Граничные значения КБЖУ для продуктов

        // 0 калорий (Вода)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Water, 100.0)
            },
            ExpectedCalories = 0.0,
            ExpectedProteins = 0.0,
            ExpectedFats = 0.0,
            ExpectedCarbs = 0.0,
            ExpectedServingSize = 100.0
        });

        // Макс жиры (Масло), мало веса (10г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Butter, 10.0)
            },
            ExpectedCalories = 90.0,
            ExpectedProteins = 0.0,
            ExpectedFats = 10.0,
            ExpectedCarbs = 0.0,
            ExpectedServingSize = 10.0
        });

        // Макс белки (Протеин), стандартная порция (30г)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Protein, 30.0)
            },
            ExpectedCalories = 111.0,
            ExpectedProteins = 30.0,
            ExpectedFats = 0.3,
            ExpectedCarbs = 0.0,
            ExpectedServingSize = 30.0
        });

        // Макс углеводы (Сахар), мало веса (5г - чайная ложка)
        testCases.Add(new DishNutritionTestCase
        {
            Recipe = new List<(Product, double)>
            {
                (ProductHelper.Sugar, 5.0)
            },
            ExpectedCalories = 20.0,
            ExpectedProteins = 0.0,
            ExpectedFats = 0.0,
            ExpectedCarbs = 5.0,
            ExpectedServingSize = 5.0
        });

        #endregion

        return testCases.Select(tc => tc.ToObjectArray());
    }
}