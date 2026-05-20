using UnityEngine;

public static class MonsterAudioRules
{
    public const string AmbientGrowlResourcePath = "Audio/MonsterAmbientGrowl";
    public const string AttackGrowlResourcePath = "Audio/MonsterAttackGrowl";
    public const string DeathGrowlResourcePath = "Audio/MonsterDeathGrowl";
    public const float DefaultMinimumAudibleDistance = 34f;
    public const float DefaultFullVolumeDistance = 4f;
    public const float DefaultMinimumVolume = 0.12f;
    public const float DefaultMaximumVolume = 1f;
    public const float MinimumAmbientGrowlDuration = 6f;
    public const float MaximumAmbientGrowlDuration = 10f;
    public const float MinimumAttackGrowlDuration = 1f;
    public const float MaximumAttackGrowlDuration = 2f;
    public const float MinimumDeathGrowlDuration = 1.5f;
    public const float MaximumDeathGrowlDuration = 3f;

    public static float CalculateProximityVolume(
        float distanceToPlayer,
        float fullVolumeDistance,
        float minimumAudibleDistance,
        float minimumVolume,
        float maximumVolume)
    {
        float nearDistance = Mathf.Max(0f, fullVolumeDistance);
        float farDistance = Mathf.Max(nearDistance + 0.01f, minimumAudibleDistance);
        float minVolume = Mathf.Clamp01(minimumVolume);
        float maxVolume = Mathf.Clamp(Mathf.Max(minVolume, maximumVolume), minVolume, 1f);

        if (distanceToPlayer >= farDistance)
            return 0f;

        float t = Mathf.InverseLerp(farDistance, nearDistance, Mathf.Max(0f, distanceToPlayer));
        float shaped = 1f - ((1f - t) * (1f - t));
        return Mathf.Lerp(minVolume, maxVolume, shaped);
    }

    public static bool ShouldPlayAttackSound(bool hasTarget, float distanceToPlayer, float attackDistance)
    {
        return hasTarget && distanceToPlayer <= Mathf.Max(0f, attackDistance);
    }
}
