using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class MonsterAudioRulesTests
{
    [Test]
    public void MonsterVolumeGetsLouderWhenCloserToPlayer()
    {
        float far = MonsterAudioRules.CalculateProximityVolume(
            distanceToPlayer: 24f,
            fullVolumeDistance: 2.5f,
            minimumAudibleDistance: 28f,
            minimumVolume: 0.03f,
            maximumVolume: 0.85f);
        float mid = MonsterAudioRules.CalculateProximityVolume(
            distanceToPlayer: 12f,
            fullVolumeDistance: 2.5f,
            minimumAudibleDistance: 28f,
            minimumVolume: 0.03f,
            maximumVolume: 0.85f);
        float close = MonsterAudioRules.CalculateProximityVolume(
            distanceToPlayer: 2f,
            fullVolumeDistance: 2.5f,
            minimumAudibleDistance: 28f,
            minimumVolume: 0.03f,
            maximumVolume: 0.85f);

        Assert.Greater(mid, far);
        Assert.Greater(close, mid);
    }

    [Test]
    public void MonsterIsSilentBeyondAudibleDistance()
    {
        Assert.AreEqual(0f, MonsterAudioRules.CalculateProximityVolume(
            distanceToPlayer: 40f,
            fullVolumeDistance: MonsterAudioRules.DefaultFullVolumeDistance,
            minimumAudibleDistance: MonsterAudioRules.DefaultMinimumAudibleDistance,
            minimumVolume: MonsterAudioRules.DefaultMinimumVolume,
            maximumVolume: MonsterAudioRules.DefaultMaximumVolume));
    }

    [Test]
    public void MonsterGrowlIsAudibleBeforeAttackRange()
    {
        float volume = MonsterAudioRules.CalculateProximityVolume(
            distanceToPlayer: 18f,
            fullVolumeDistance: MonsterAudioRules.DefaultFullVolumeDistance,
            minimumAudibleDistance: MonsterAudioRules.DefaultMinimumAudibleDistance,
            minimumVolume: MonsterAudioRules.DefaultMinimumVolume,
            maximumVolume: MonsterAudioRules.DefaultMaximumVolume);

        Assert.GreaterOrEqual(volume, 0.55f);
    }

    [Test]
    public void ClassicHorrorGrowlResourcesAreBundled()
    {
        AudioClip ambient = Resources.Load<AudioClip>(MonsterAudioRules.AmbientGrowlResourcePath);
        AudioClip attack = Resources.Load<AudioClip>(MonsterAudioRules.AttackGrowlResourcePath);
        AudioClip death = Resources.Load<AudioClip>(MonsterAudioRules.DeathGrowlResourcePath);

        Assert.IsNotNull(ambient);
        Assert.IsNotNull(attack);
        Assert.IsNotNull(death);
        Assert.AreNotEqual(ambient.name, attack.name);
        Assert.AreNotEqual(attack.name, death.name);
        Assert.GreaterOrEqual(ambient.length, MonsterAudioRules.MinimumAmbientGrowlDuration);
        Assert.LessOrEqual(ambient.length, MonsterAudioRules.MaximumAmbientGrowlDuration);
        Assert.GreaterOrEqual(attack.length, MonsterAudioRules.MinimumAttackGrowlDuration);
        Assert.LessOrEqual(attack.length, MonsterAudioRules.MaximumAttackGrowlDuration);
        Assert.GreaterOrEqual(death.length, MonsterAudioRules.MinimumDeathGrowlDuration);
        Assert.LessOrEqual(death.length, MonsterAudioRules.MaximumDeathGrowlDuration);
    }

    [Test]
    public void AttackSoundOnlyPlaysWhenMonsterHasTargetInAttackRange()
    {
        Assert.IsFalse(MonsterAudioRules.ShouldPlayAttackSound(hasTarget: false, distanceToPlayer: 1f, attackDistance: 2f));
        Assert.IsFalse(MonsterAudioRules.ShouldPlayAttackSound(hasTarget: true, distanceToPlayer: 3f, attackDistance: 2f));
        Assert.IsTrue(MonsterAudioRules.ShouldPlayAttackSound(hasTarget: true, distanceToPlayer: 1.5f, attackDistance: 2f));
    }

    [Test]
    public void EnemyDeathStopsAllMonsterAudioSources()
    {
        Type monsterFollowType = FindRuntimeType("MonsterFollow");
        Type enemyHealthType = FindRuntimeType("EnemyHealth");
        GameObject player = new GameObject("Player");
        GameObject monster = new GameObject("Monster");

        try
        {
            player.tag = "Player";
            monster.AddComponent<AudioSource>();
            monster.AddComponent<AudioSource>();
            MonoBehaviour follow = (MonoBehaviour)monster.AddComponent(monsterFollowType);
            Component health = monster.AddComponent(enemyHealthType);

            enemyHealthType
                .GetMethod("ResetarParaSpawn", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(health, new object[] { Vector3.zero, Quaternion.identity, null });
            AudioSource[] sources = monster.GetComponents<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].volume = 1f;
            }

            enemyHealthType
                .GetMethod("Damage", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(health, new object[] { 9999f });

            bool isDead = (bool)enemyHealthType
                .GetProperty("EstaMorto", BindingFlags.Instance | BindingFlags.Public)
                .GetValue(health);
            bool allSourcesStopped = (bool)monsterFollowType
                .GetMethod("TodasAsFontesDeAudioParadas", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(follow, null);

            Assert.IsTrue(isDead);
            Assert.IsFalse(follow.enabled);
            Assert.IsTrue(allSourcesStopped);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(monster);
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    private static Type FindRuntimeType(string typeName)
    {
        Type type = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(candidate => candidate != null);

        Assert.NotNull(type, $"Runtime type {typeName} must be available to validate monster audio lifecycle.");
        return type;
    }
}
