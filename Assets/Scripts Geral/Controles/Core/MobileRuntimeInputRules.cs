public static class MobileRuntimeInputRules
{
    public static bool ShouldUseDesktopInput(bool isMobilePlatform)
    {
        return !isMobilePlatform;
    }

    public static bool ShouldUseMovementHeadBob(bool isMobilePlatform)
    {
        return !isMobilePlatform;
    }
}
