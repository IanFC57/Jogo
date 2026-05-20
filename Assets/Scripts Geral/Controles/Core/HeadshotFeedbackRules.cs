public static class HeadshotFeedbackRules
{
    public const string FallbackSpokenWord = "HEADSHOT";
    public const string FallbackResourcePath = "Audio/HeadshotAnnouncer";
    public const float MinimumAnnouncerClipDuration = 0.75f;
    public const float DefaultAnnouncerPitch = 0.92f;

    public static bool ShouldPlayHeadshotFeedback(EnemyHitZone zone, bool damageApplied)
    {
        return damageApplied && zone == EnemyHitZone.Head;
    }
}
