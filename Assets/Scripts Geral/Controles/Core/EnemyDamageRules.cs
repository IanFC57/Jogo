using UnityEngine;

public static class EnemyDamageRules
{
    public const float DefaultReferenceShotDamage = 45f;
    public const float DefaultBodyShotsToKill = 6f;
    public const float DefaultChestMultiplier = 2f;
    public const float DefaultHeadMultiplier = 6f;
    public const float DefaultBodyMultiplier = 1f;

    public static float CalculateMaxHealth(float referenceShotDamage, float bodyShotsToKill)
    {
        float safeReferenceDamage = Mathf.Max(1f, referenceShotDamage);
        float safeBodyShots = Mathf.Max(1f, bodyShotsToKill);
        return safeReferenceDamage * safeBodyShots;
    }

    public static float GetZoneMultiplier(
        EnemyHitZone zone,
        float headMultiplier,
        float chestMultiplier,
        float bodyMultiplier = DefaultBodyMultiplier)
    {
        switch (zone)
        {
            case EnemyHitZone.Head:
                return Mathf.Max(0f, headMultiplier);
            case EnemyHitZone.Chest:
                return Mathf.Max(0f, chestMultiplier);
            default:
                return Mathf.Max(0f, bodyMultiplier);
        }
    }

    public static float CalculateZoneDamage(
        float rawWeaponDamage,
        EnemyHitZone zone,
        float headMultiplier,
        float chestMultiplier,
        float bodyMultiplier = DefaultBodyMultiplier)
    {
        return Mathf.Max(0f, rawWeaponDamage) *
               GetZoneMultiplier(zone, headMultiplier, chestMultiplier, bodyMultiplier);
    }

    public static int CalculateShotsToKill(
        float maxHealth,
        float rawWeaponDamage,
        EnemyHitZone zone,
        float headMultiplier,
        float chestMultiplier,
        float bodyMultiplier = DefaultBodyMultiplier)
    {
        float damagePerShot = CalculateZoneDamage(rawWeaponDamage, zone, headMultiplier, chestMultiplier, bodyMultiplier);
        if (damagePerShot <= 0f)
            return int.MaxValue;

        return Mathf.CeilToInt(Mathf.Max(0f, maxHealth) / damagePerShot);
    }
}
