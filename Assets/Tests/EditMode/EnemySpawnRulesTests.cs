using NUnit.Framework;
using UnityEngine;

public sealed class EnemySpawnRulesTests
{
    [Test]
    public void SpawnIntervalDefaultIsFifteenSeconds()
    {
        Assert.AreEqual(15f, EnemySpawnRules.DefaultSpawnIntervalSeconds);
    }

    [Test]
    public void MinimumSpawnDistanceDefaultIsFarEnoughForMobile()
    {
        Assert.AreEqual(14f, EnemySpawnRules.DefaultMinimumSpawnDistanceFromPlayer);
        Assert.AreEqual(14f, EnemySpawnRules.SanitizeMinimumSpawnDistance(8f));
        Assert.AreEqual(18f, EnemySpawnRules.SanitizeMinimumSpawnDistance(18f));
    }

    [Test]
    public void SpawnerCreatesMonsterOnlyUnderAliveLimit()
    {
        Assert.IsTrue(EnemySpawnRules.CanSpawn(aliveCount: 0, aliveLimit: 3));
        Assert.IsTrue(EnemySpawnRules.CanSpawn(aliveCount: 2, aliveLimit: 3));
        Assert.IsFalse(EnemySpawnRules.CanSpawn(aliveCount: 3, aliveLimit: 3));
        Assert.IsFalse(EnemySpawnRules.CanSpawn(aliveCount: 4, aliveLimit: 3));
    }

    [Test]
    public void PoolPrewarmsAtLeastAliveLimit()
    {
        Assert.AreEqual(3, EnemySpawnRules.SanitizePoolSize(configuredPoolSize: 0, aliveLimit: 3));
        Assert.AreEqual(5, EnemySpawnRules.SanitizePoolSize(configuredPoolSize: 5, aliveLimit: 3));
    }

    [Test]
    public void ImmediateFirstSpawnUsesZeroDelay()
    {
        Assert.AreEqual(0f, EnemySpawnRules.GetFirstSpawnDelay(spawnImmediately: true, configuredDelay: 15f));
    }

    [Test]
    public void DelayedFirstSpawnSanitizesDelay()
    {
        Assert.AreEqual(0.1f, EnemySpawnRules.GetFirstSpawnDelay(spawnImmediately: false, configuredDelay: 0f));
        Assert.AreEqual(15f, EnemySpawnRules.GetFirstSpawnDelay(spawnImmediately: false, configuredDelay: 15f));
    }

    [Test]
    public void SpawnCandidateMustBeOnNavMesh()
    {
        Assert.IsFalse(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: false,
            hasCompletePath: true,
            distanceToPlayer: 100f,
            minDistanceFromPlayer: 0f,
            requireCompletePath: true));
    }

    [Test]
    public void SpawnCandidateMustHaveCompletePathWhenRequired()
    {
        Assert.IsFalse(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: true,
            hasCompletePath: false,
            distanceToPlayer: 100f,
            minDistanceFromPlayer: 0f,
            requireCompletePath: true));
    }

    [Test]
    public void SpawnCandidateMustRespectMinimumDistance()
    {
        Assert.IsFalse(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: true,
            hasCompletePath: true,
            distanceToPlayer: 2f,
            minDistanceFromPlayer: 8f,
            requireCompletePath: true));
    }

    [Test]
    public void ReachableCandidateFarFromPlayerIsAccepted()
    {
        Assert.IsTrue(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: true,
            hasCompletePath: true,
            distanceToPlayer: 16f,
            minDistanceFromPlayer: 14f,
            requireCompletePath: true));
    }

    [Test]
    public void CandidateInsideForbiddenAreaIsRejected()
    {
        Assert.IsFalse(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: true,
            hasCompletePath: true,
            distanceToPlayer: 20f,
            minDistanceFromPlayer: 8f,
            requireCompletePath: true,
            isInsideForbiddenArea: true));
    }

    [Test]
    public void CandidateInsidePlayerViewIsRejected()
    {
        Assert.IsFalse(EnemySpawnRules.IsCandidateAllowed(
            isOnNavMesh: true,
            hasCompletePath: true,
            distanceToPlayer: 30f,
            minDistanceFromPlayer: 14f,
            requireCompletePath: true,
            isInsidePlayerView: true));
    }

    [Test]
    public void ViewportPointsInsidePaddedViewAreBlocked()
    {
        Assert.IsTrue(EnemySpawnRules.IsViewportPointInsidePlayerView(new Vector3(0.5f, 0.5f, 10f), 0.12f));
        Assert.IsTrue(EnemySpawnRules.IsViewportPointInsidePlayerView(new Vector3(-0.05f, 0.5f, 10f), 0.12f));
        Assert.IsFalse(EnemySpawnRules.IsViewportPointInsidePlayerView(new Vector3(-0.2f, 0.5f, 10f), 0.12f));
        Assert.IsFalse(EnemySpawnRules.IsViewportPointInsidePlayerView(new Vector3(0.5f, 0.5f, -1f), 0.12f));
    }
}
