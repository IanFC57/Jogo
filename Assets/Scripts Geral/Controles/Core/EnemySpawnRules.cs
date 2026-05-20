using UnityEngine;

public static class EnemySpawnRules
{
    public const float DefaultSpawnIntervalSeconds = 15f;
    public const float DefaultMinimumSpawnDistanceFromPlayer = 14f;
    public const float DefaultViewSafetyViewportMargin = 0.12f;

    public static bool CanSpawn(int aliveCount, int aliveLimit)
    {
        return aliveLimit > 0 && aliveCount < aliveLimit;
    }

    public static float SanitizeSpawnDelay(float configuredDelay)
    {
        return Mathf.Max(0.1f, configuredDelay);
    }

    public static float SanitizeMinimumSpawnDistance(float configuredDistance)
    {
        return Mathf.Max(DefaultMinimumSpawnDistanceFromPlayer, configuredDistance);
    }

    public static float SanitizeViewSafetyMargin(float configuredMargin)
    {
        return Mathf.Clamp(configuredMargin, 0f, 0.5f);
    }

    public static float GetFirstSpawnDelay(bool spawnImmediately, float configuredDelay)
    {
        return spawnImmediately ? 0f : SanitizeSpawnDelay(configuredDelay);
    }

    public static int SanitizePoolSize(int configuredPoolSize, int aliveLimit)
    {
        return Mathf.Max(0, Mathf.Max(configuredPoolSize, aliveLimit));
    }

    public static bool IsCandidateAllowed(
        bool isOnNavMesh,
        bool hasCompletePath,
        float distanceToPlayer,
        float minDistanceFromPlayer,
        bool requireCompletePath,
        bool isInsideForbiddenArea = false,
        bool isInsidePlayerView = false)
    {
        if (!isOnNavMesh)
            return false;

        if (isInsideForbiddenArea)
            return false;

        if (isInsidePlayerView)
            return false;

        if (requireCompletePath && !hasCompletePath)
            return false;

        return distanceToPlayer >= SanitizeMinimumSpawnDistance(minDistanceFromPlayer);
    }

    public static bool IsViewportPointInsidePlayerView(Vector3 viewportPoint, float viewportMargin)
    {
        if (viewportPoint.z <= 0f)
            return false;

        float margin = SanitizeViewSafetyMargin(viewportMargin);
        return viewportPoint.x >= -margin &&
               viewportPoint.x <= 1f + margin &&
               viewportPoint.y >= -margin &&
               viewportPoint.y <= 1f + margin;
    }
}
