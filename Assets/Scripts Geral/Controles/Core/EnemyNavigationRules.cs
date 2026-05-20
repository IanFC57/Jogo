using UnityEngine;

public static class EnemyNavigationRules
{
    public static bool ShouldRepath(
        float now,
        float nextAllowedRepathTime,
        bool targetMovedEnough,
        bool hasNoPath,
        bool pathInvalid)
    {
        if (hasNoPath || pathInvalid)
            return true;

        return now >= nextAllowedRepathTime && targetMovedEnough;
    }

    public static bool IsStuck(
        float movedDistance,
        float minMoveDistance,
        float velocity,
        float velocityThreshold,
        float elapsed)
    {
        return elapsed > 0f &&
               movedDistance < Mathf.Max(0f, minMoveDistance) &&
               velocity <= Mathf.Max(0f, velocityThreshold);
    }

    public static bool ShouldRecoverFromStuck(float stuckSeconds, float threshold)
    {
        return stuckSeconds >= Mathf.Max(0.1f, threshold);
    }
}
