using UnityEngine;

public static class PlayerHealthRules
{
    public const int DefaultRegenerationAmount = 10;
    public const float DefaultRegenerationIntervalSeconds = 10f;

    public static int ApplyDamage(int currentHealth, int damage)
    {
        return Mathf.Max(0, currentHealth - Mathf.Max(0, damage));
    }

    public static int ApplyRegeneration(int currentHealth, int maxHealth, int amount)
    {
        int safeMaxHealth = Mathf.Max(0, maxHealth);
        int safeCurrentHealth = Mathf.Clamp(currentHealth, 0, safeMaxHealth);
        return Mathf.Min(safeMaxHealth, safeCurrentHealth + Mathf.Max(0, amount));
    }

    public static bool ShouldRegenerate(float elapsedSeconds, float intervalSeconds, int currentHealth, int maxHealth)
    {
        return currentHealth > 0 &&
               currentHealth < Mathf.Max(0, maxHealth) &&
               elapsedSeconds >= SanitizeInterval(intervalSeconds);
    }

    public static float SanitizeInterval(float intervalSeconds)
    {
        return Mathf.Max(0.1f, intervalSeconds);
    }
}
