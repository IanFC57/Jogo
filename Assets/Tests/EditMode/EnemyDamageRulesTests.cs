using NUnit.Framework;

public sealed class EnemyDamageRulesTests
{
    private const float ReferenceShotDamage = 45f;

    [Test]
    public void DefaultProfileMakesBodyKillInSixShots()
    {
        float maxHealth = EnemyDamageRules.CalculateMaxHealth(ReferenceShotDamage, bodyShotsToKill: 6f);

        Assert.AreEqual(6, EnemyDamageRules.CalculateShotsToKill(
            maxHealth,
            ReferenceShotDamage,
            EnemyHitZone.Body,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));
    }

    [Test]
    public void DefaultProfileMakesChestKillInThreeShots()
    {
        float maxHealth = EnemyDamageRules.CalculateMaxHealth(ReferenceShotDamage, bodyShotsToKill: 6f);

        Assert.AreEqual(3, EnemyDamageRules.CalculateShotsToKill(
            maxHealth,
            ReferenceShotDamage,
            EnemyHitZone.Chest,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));
    }

    [Test]
    public void DefaultProfileMakesHeadKillInOneShot()
    {
        float maxHealth = EnemyDamageRules.CalculateMaxHealth(ReferenceShotDamage, bodyShotsToKill: 6f);

        Assert.AreEqual(1, EnemyDamageRules.CalculateShotsToKill(
            maxHealth,
            ReferenceShotDamage,
            EnemyHitZone.Head,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));
    }

    [Test]
    public void HeadChestAndBodyDamageAreNotDuplicated()
    {
        Assert.AreEqual(45f, EnemyDamageRules.CalculateZoneDamage(
            ReferenceShotDamage,
            EnemyHitZone.Body,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));

        Assert.AreEqual(90f, EnemyDamageRules.CalculateZoneDamage(
            ReferenceShotDamage,
            EnemyHitZone.Chest,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));

        Assert.AreEqual(270f, EnemyDamageRules.CalculateZoneDamage(
            ReferenceShotDamage,
            EnemyHitZone.Head,
            EnemyDamageRules.DefaultHeadMultiplier,
            EnemyDamageRules.DefaultChestMultiplier));
    }
}
