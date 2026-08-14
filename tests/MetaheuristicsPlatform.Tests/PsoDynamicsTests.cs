using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoDynamicsTests
{
    [Fact]
    public void ClercKennedy_DefaultPhi_HasExpectedConstriction()
    {
        double chi =
            PsoConstrictionFactor.Compute(4.10);

        Assert.Equal(
            0.7298437881283576,
            chi,
            12);
    }

    [Fact]
    public void ConstantInertia_ReturnsExpectedMultipliers()
    {
        var dynamics =
            new ConstantInertiaDynamics(0.7);

        PsoVelocityCoefficients coefficients =
            dynamics.GetCoefficients(100);

        Assert.Equal(
            0.7,
            coefficients.PreviousVelocityMultiplier);

        Assert.Equal(
            1.0,
            coefficients.AttractionMultiplier);
    }

    [Fact]
    public void LinearInertia_InterpolatesAndClamps()
    {
        var dynamics =
            new LinearInertiaDynamics(
                0.9,
                0.4,
                100);

        Assert.Equal(
            0.9,
            dynamics.GetCoefficients(0)
                .PreviousVelocityMultiplier,
            12);

        Assert.Equal(
            0.65,
            dynamics.GetCoefficients(50)
                .PreviousVelocityMultiplier,
            12);

        Assert.Equal(
            0.4,
            dynamics.GetCoefficients(200)
                .PreviousVelocityMultiplier,
            12);
    }
}