using Core.Models;

namespace Test.Core.TestData;

public class DishNutritionTestCase
{
    public List<(Product Product, double Amount)> Recipe { get; set; }
    public double ExpectedCalories { get; set; }
    public double ExpectedProteins { get; set; }
    public double ExpectedFats { get; set; }
    public double ExpectedCarbs { get; set; }
    public double ExpectedServingSize { get; set; }

    public object[] ToObjectArray() => new object[]
    {
        Recipe,
        ExpectedCalories,
        ExpectedProteins,
        ExpectedFats,
        ExpectedCarbs,
        ExpectedServingSize
    };
}