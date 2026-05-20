using NUnit.Framework;

public sealed class MobileRuntimeInputRulesTests
{
    [Test]
    public void AndroidUsesMobileControlsInsteadOfLegacyDesktopInput()
    {
        Assert.IsFalse(MobileRuntimeInputRules.ShouldUseDesktopInput(isMobilePlatform: true));
        Assert.IsFalse(MobileRuntimeInputRules.ShouldUseMovementHeadBob(isMobilePlatform: true));
    }

    [Test]
    public void DesktopKeepsLegacyInputAndHeadBobAvailable()
    {
        Assert.IsTrue(MobileRuntimeInputRules.ShouldUseDesktopInput(isMobilePlatform: false));
        Assert.IsTrue(MobileRuntimeInputRules.ShouldUseMovementHeadBob(isMobilePlatform: false));
    }
}
