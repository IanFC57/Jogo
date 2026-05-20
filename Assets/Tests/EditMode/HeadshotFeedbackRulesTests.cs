using NUnit.Framework;
using UnityEngine;

public sealed class HeadshotFeedbackRulesTests
{
    [Test]
    public void FallbackHeadshotFeedbackIsSpokenAnnouncerStyle()
    {
        Assert.AreEqual("HEADSHOT", HeadshotFeedbackRules.FallbackSpokenWord);
        Assert.AreEqual("Audio/HeadshotAnnouncer", HeadshotFeedbackRules.FallbackResourcePath);
        Assert.GreaterOrEqual(HeadshotFeedbackRules.MinimumAnnouncerClipDuration, 0.75f);
    }

    [Test]
    public void BundledHeadshotAnnouncerClipIsAvailableToRuntime()
    {
        AudioClip clip = Resources.Load<AudioClip>(HeadshotFeedbackRules.FallbackResourcePath);

        Assert.IsNotNull(clip);
        Assert.GreaterOrEqual(clip.length, HeadshotFeedbackRules.MinimumAnnouncerClipDuration);
    }

    [Test]
    public void FeedbackOnlyPlaysForAppliedHeadshotDamage()
    {
        Assert.IsTrue(HeadshotFeedbackRules.ShouldPlayHeadshotFeedback(EnemyHitZone.Head, damageApplied: true));
        Assert.IsFalse(HeadshotFeedbackRules.ShouldPlayHeadshotFeedback(EnemyHitZone.Chest, damageApplied: true));
        Assert.IsFalse(HeadshotFeedbackRules.ShouldPlayHeadshotFeedback(EnemyHitZone.Body, damageApplied: true));
        Assert.IsFalse(HeadshotFeedbackRules.ShouldPlayHeadshotFeedback(EnemyHitZone.Head, damageApplied: false));
    }
}
