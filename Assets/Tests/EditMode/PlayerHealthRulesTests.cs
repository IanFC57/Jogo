using NUnit.Framework;

public sealed class PlayerHealthRulesTests
{
    [Test]
    public void DefaultRegenerationIsTenEveryTenSeconds()
    {
        Assert.AreEqual(10, PlayerHealthRules.DefaultRegenerationAmount);
        Assert.AreEqual(10f, PlayerHealthRules.DefaultRegenerationIntervalSeconds);
    }

    [Test]
    public void RegenerationRestoresHealthWithoutPassingMaximum()
    {
        Assert.AreEqual(60, PlayerHealthRules.ApplyRegeneration(50, 100, 10));
        Assert.AreEqual(100, PlayerHealthRules.ApplyRegeneration(95, 100, 10));
    }

    [Test]
    public void RegenerationOnlyTicksWhenAliveBelowMaximumAndIntervalElapsed()
    {
        Assert.IsFalse(PlayerHealthRules.ShouldRegenerate(9.9f, 10f, 50, 100));
        Assert.IsFalse(PlayerHealthRules.ShouldRegenerate(10f, 10f, 0, 100));
        Assert.IsFalse(PlayerHealthRules.ShouldRegenerate(10f, 10f, 100, 100));
        Assert.IsTrue(PlayerHealthRules.ShouldRegenerate(10f, 10f, 50, 100));
    }

    [Test]
    public void DamageRestartsTheTenSecondRegenerationWindow()
    {
        int healthAfterDamage = PlayerHealthRules.ApplyDamage(100, 25);

        Assert.AreEqual(75, healthAfterDamage);
        Assert.IsFalse(PlayerHealthRules.ShouldRegenerate(9.99f, 10f, healthAfterDamage, 100));
        Assert.IsTrue(PlayerHealthRules.ShouldRegenerate(10f, 10f, healthAfterDamage, 100));
        Assert.AreEqual(85, PlayerHealthRules.ApplyRegeneration(healthAfterDamage, 100, PlayerHealthRules.DefaultRegenerationAmount));
    }
}
