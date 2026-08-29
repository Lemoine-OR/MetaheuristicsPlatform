using MetaheuristicsPlatform.ReferenceGrade;

namespace MetaheuristicsPlatform.Tests;

public sealed class ReferenceRandomStreamTraceTests
{
    [Fact]
    public void DeriveSeed_IsDeterministicAcrossEquivalentTraces()
    {
        ReferenceRandomStreamTrace first = new(123456UL);
        ReferenceRandomStreamTrace second = new(123456UL);

        ulong firstSeed = first.DeriveSeed("mutation", 0);
        ulong secondSeed = second.DeriveSeed("mutation", 0);

        Assert.Equal(firstSeed, secondSeed);
        Assert.Throws<InvalidOperationException>(() => first.DeriveSeed("mutation", 0));
    }
}
