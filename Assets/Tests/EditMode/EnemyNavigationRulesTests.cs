using NUnit.Framework;

public sealed class EnemyNavigationRulesTests
{
    [Test]
    public void RepathHappensImmediatelyWhenThereIsNoPath()
    {
        Assert.IsTrue(EnemyNavigationRules.ShouldRepath(
            now: 0f,
            nextAllowedRepathTime: 10f,
            targetMovedEnough: false,
            hasNoPath: true,
            pathInvalid: false));
    }

    [Test]
    public void RepathHappensImmediatelyWhenPathIsInvalid()
    {
        Assert.IsTrue(EnemyNavigationRules.ShouldRepath(
            now: 0f,
            nextAllowedRepathTime: 10f,
            targetMovedEnough: false,
            hasNoPath: false,
            pathInvalid: true));
    }

    [Test]
    public void RepathWaitsForIntervalWhenTargetMoved()
    {
        Assert.IsFalse(EnemyNavigationRules.ShouldRepath(
            now: 0.1f,
            nextAllowedRepathTime: 0.2f,
            targetMovedEnough: true,
            hasNoPath: false,
            pathInvalid: false));

        Assert.IsTrue(EnemyNavigationRules.ShouldRepath(
            now: 0.2f,
            nextAllowedRepathTime: 0.2f,
            targetMovedEnough: true,
            hasNoPath: false,
            pathInvalid: false));
    }

    [Test]
    public void StuckRequiresLowMovementAndLowVelocity()
    {
        Assert.IsTrue(EnemyNavigationRules.IsStuck(
            movedDistance: 0.02f,
            minMoveDistance: 0.15f,
            velocity: 0.01f,
            velocityThreshold: 0.05f,
            elapsed: 0.5f));

        Assert.IsFalse(EnemyNavigationRules.IsStuck(
            movedDistance: 0.3f,
            minMoveDistance: 0.15f,
            velocity: 0.01f,
            velocityThreshold: 0.05f,
            elapsed: 0.5f));
    }

    [Test]
    public void RecoveryStartsOnlyAfterThreshold()
    {
        Assert.IsFalse(EnemyNavigationRules.ShouldRecoverFromStuck(stuckSeconds: 1.0f, threshold: 1.5f));
        Assert.IsTrue(EnemyNavigationRules.ShouldRecoverFromStuck(stuckSeconds: 1.5f, threshold: 1.5f));
    }
}
