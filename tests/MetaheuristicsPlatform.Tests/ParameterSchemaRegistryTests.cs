using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ParameterSchemaRegistryTests
{
    [Fact]
    public void Register_RejectsDuplicateParameterNames_AndDuplicateAlgorithmIds()
    {
        ParameterSchemaRegistry registry = new();
        var schema = new[]
        {
            new ReferenceParameterDescriptor("iterations", "System.Int32", "100", "Maximum iterations.", 1, 10000)
        };

        registry.Register("algorithm-id", schema);
        Assert.Single(registry.Get("algorithm-id"));
        Assert.Throws<InvalidOperationException>(() => registry.Register("algorithm-id", schema));
        Assert.Throws<ArgumentException>(() => registry.Register(
            "other-id",
            new[] { schema[0], schema[0] }));
    }
}
