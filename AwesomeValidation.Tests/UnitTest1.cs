using AwesomeValidation;

namespace AwesomeValidations.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}


public class ExampleClass
{
    [ValidationDefinition]
    public void Test()
    {
    }
}
