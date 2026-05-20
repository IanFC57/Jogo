using NUnit.Framework;

public sealed class AmmoReloadMathTests
{
    [Test]
    public void ReloadUsesOnlyMissingAmmoWhenReserveIsEnough()
    {
        Assert.AreEqual(5, AmmoReloadMath.GetReloadAmount(currentAmmo: 7, ammoCapacity: 12, reserveAmmo: 30));
    }

    [Test]
    public void ReloadUsesAllReserveWhenReserveIsSmallerThanMissingAmmo()
    {
        Assert.AreEqual(3, AmmoReloadMath.GetReloadAmount(currentAmmo: 4, ammoCapacity: 12, reserveAmmo: 3));
    }

    [Test]
    public void FullMagazineConsumesNoReserve()
    {
        Assert.AreEqual(0, AmmoReloadMath.GetReloadAmount(currentAmmo: 12, ammoCapacity: 12, reserveAmmo: 30));
    }
}
