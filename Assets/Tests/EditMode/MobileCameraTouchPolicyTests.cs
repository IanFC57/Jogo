using NUnit.Framework;
using UnityEngine;

public sealed class MobileCameraTouchPolicyTests
{
    private static readonly Vector2 ScreenSize = new Vector2(2400f, 1080f);
    private static readonly Rect SafeArea = new Rect(88f, 0f, 2312f, 1080f);
    private static readonly Rect CameraArea = new Rect(0f, 0f, 1f, 1f);

    [Test]
    public void JoystickFingerDoesNotMoveCameraEvenWhenDraggedForwardOrSideways()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        Vector2 beginOnJoystick = new Vector2(240f, 240f);
        Vector2 draggedFarBeyondJoystickIntoCameraArea = new Vector2(900f, 980f);

        Vector2 beginDelta = policy.ProcessTouch(
            Touch(0, beginOnJoystick, TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 dragDelta = policy.ProcessTouch(
            Touch(0, draggedFarBeyondJoystickIntoCameraArea, TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, beginDelta);
        Assert.AreEqual(Vector2.zero, dragDelta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void SceneUiCapturedJoystickFingerDoesNotMoveCameraEvenOutsideFallbackZone()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(620f, 670f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: true,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(980f, 1000f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: true,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, delta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void SecondFingerAwayFromControlsMovesCameraWhileJoystickFingerIsHeld()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(240f, 240f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        policy.ProcessTouch(
            Touch(1, new Vector2(1500f, 900f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 cameraDelta = policy.ProcessTouch(
            Touch(1, new Vector2(1620f, 860f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(new Vector2(120f, -40f), cameraDelta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsTrue(policy.HasCameraFinger);
    }

    [Test]
    public void FireButtonFingerDoesNotMoveCamera()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        Vector2 fireButton = new Vector2(2200f, 240f);

        policy.ProcessTouch(
            Touch(0, fireButton, TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(2280f, 300f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, delta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void FireButtonFingerDoesNotBecomeCameraWhenDraggedIntoFreeLookArea()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(2200f, 240f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(1300f, 920f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, delta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void ReloadButtonFingerDoesNotMoveCamera()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        Vector2 reloadButton = new Vector2(1600f, 240f);

        policy.ProcessTouch(
            Touch(0, reloadButton, TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(1660f, 300f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, delta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void ReloadButtonFingerDoesNotBecomeCameraWhenDraggedIntoFreeLookArea()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(1600f, 240f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(1200f, 900f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(Vector2.zero, delta);
        Assert.IsTrue(policy.IsFingerBlocked(0));
        Assert.IsFalse(policy.HasCameraFinger);
    }

    [Test]
    public void SingleFingerInCameraAreaMovesCamera()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(1500f, 900f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(1564f, 868f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(new Vector2(64f, -32f), delta);
        Assert.IsTrue(policy.HasCameraFinger);
    }

    [Test]
    public void FreeTouchOnLeftSideAboveJoystickMovesCamera()
    {
        MobileCameraTouchPolicy policy = new MobileCameraTouchPolicy();

        policy.ProcessTouch(
            Touch(0, new Vector2(300f, 900f), TouchPhase.Began),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Vector2 delta = policy.ProcessTouch(
            Touch(0, new Vector2(420f, 860f), TouchPhase.Moved),
            ScreenSize,
            SafeArea,
            CameraArea,
            capturedBySceneUi: false,
            pointerOverEventSystem: false);

        Assert.AreEqual(new Vector2(120f, -40f), delta);
        Assert.IsTrue(policy.HasCameraFinger);
    }

    private static MobileTouchSample Touch(int fingerId, Vector2 position, TouchPhase phase)
    {
        return new MobileTouchSample(fingerId, position, phase);
    }
}
