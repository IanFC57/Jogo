using NUnit.Framework;
using UnityEngine;

public sealed class MobileTouchZonesTests
{
    private static readonly Vector2 ScreenSize = new Vector2(2400f, 1080f);
    private static readonly Rect SafeArea = new Rect(88f, 0f, 2312f, 1080f);

    [Test]
    public void BottomLeftJoystickAreaIsReservedForMovement()
    {
        Assert.IsTrue(MobileTouchZones.IsMovementZone(new Vector2(260f, 260f), ScreenSize, SafeArea));
        Assert.IsTrue(MobileTouchZones.IsReservedControlZone(new Vector2(260f, 260f), ScreenSize, SafeArea));
    }

    [Test]
    public void LeftSideAboveJoystickIsFreeCameraArea()
    {
        Vector2 aboveJoystick = new Vector2(300f, 930f);

        Assert.IsFalse(MobileTouchZones.IsMovementZone(aboveJoystick, ScreenSize, SafeArea));
        Assert.IsFalse(MobileTouchZones.IsReservedControlZone(aboveJoystick, ScreenSize, SafeArea));
        Assert.IsTrue(MobileTouchZones.IsCameraZone(aboveJoystick, ScreenSize, SafeArea, new Rect(0f, 0f, 1f, 1f)));
    }

    [Test]
    public void FireAndReloadAreasAreReservedButNotMovement()
    {
        Vector2 reload = new Vector2(1600f, 240f);
        Vector2 fire = new Vector2(2200f, 240f);

        Assert.IsTrue(MobileTouchZones.IsReloadZone(reload, ScreenSize, SafeArea));
        Assert.IsTrue(MobileTouchZones.IsFireZone(fire, ScreenSize, SafeArea));
        Assert.IsFalse(MobileTouchZones.IsMovementZone(reload, ScreenSize, SafeArea));
        Assert.IsFalse(MobileTouchZones.IsMovementZone(fire, ScreenSize, SafeArea));
        Assert.IsTrue(MobileTouchZones.IsReservedControlZone(reload, ScreenSize, SafeArea));
        Assert.IsTrue(MobileTouchZones.IsReservedControlZone(fire, ScreenSize, SafeArea));
    }

    [Test]
    public void RightMiddleScreenIsFreeForCameraAfterButtonsMoveDown()
    {
        Vector2 oldButtonHeight = new Vector2(2100f, 540f);

        Assert.IsFalse(MobileTouchZones.IsFireZone(oldButtonHeight, ScreenSize, SafeArea));
        Assert.IsFalse(MobileTouchZones.IsReloadZone(oldButtonHeight, ScreenSize, SafeArea));
        Assert.IsFalse(MobileTouchZones.IsReservedControlZone(oldButtonHeight, ScreenSize, SafeArea));
    }
}
