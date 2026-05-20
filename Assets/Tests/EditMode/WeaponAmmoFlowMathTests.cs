using NUnit.Framework;

public sealed class WeaponAmmoFlowMathTests
{
    [Test]
    public void FireConsumesExactlyOneRound()
    {
        Assert.AreEqual(15, WeaponAmmoFlow.ConsumeRound(currentAmmo: 16, infiniteAmmo: false));
    }

    [Test]
    public void FireNeverMakesAmmoNegative()
    {
        Assert.AreEqual(0, WeaponAmmoFlow.ConsumeRound(currentAmmo: 0, infiniteAmmo: false));
    }

    [Test]
    public void InfiniteAmmoDoesNotConsumeMagazine()
    {
        Assert.AreEqual(16, WeaponAmmoFlow.ConsumeRound(currentAmmo: 16, infiniteAmmo: true));
    }

    [Test]
    public void RemoteFireReleasesSemiAutoGateForEachMobileTap()
    {
        Assert.IsTrue(WeaponRemoteFirePolicy.ShouldReleaseSemiAutoGate(isSemiAutomatic: true));
    }

    [Test]
    public void RemoteFireDoesNotNeedToReleaseFullAutoGate()
    {
        Assert.IsFalse(WeaponRemoteFirePolicy.ShouldReleaseSemiAutoGate(isSemiAutomatic: false));
    }
}
