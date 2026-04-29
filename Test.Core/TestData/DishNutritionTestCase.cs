namespace Test.Core.TestData;

public class DishNutritionTestCase
{
    public double[] ProductIdsAndAmounts { get; set; }
    public double ExpectedCalories { get; set; }
    public double ExpectedProteins { get; set; }
    public double ExpectedFats { get; set; }
    public double ExpectedCarbs { get; set; }
    public double ExpectedServingSize { get; set; }

    public object[] ToObjectArray() => new object[]
    {
        ProductIdsAndAmounts,
        ExpectedCalories,
        ExpectedProteins,
        ExpectedFats,
        ExpectedCarbs,
        ExpectedServingSize
    };
}