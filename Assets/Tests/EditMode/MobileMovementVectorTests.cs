using NUnit.Framework;
using UnityEngine;

public sealed class MobileMovementVectorTests
{
    [Test]
    public void HorizontalJoystickInputMovesRightAndLeft()
    {
        Vector3 right = Vector3.right;
        Vector3 forward = Vector3.forward;

        Assert.AreEqual(Vector3.right, MobileMovementVector.CalculateWorldDirection(right, forward, 1f, 0f));
        Assert.AreEqual(Vector3.left, MobileMovementVector.CalculateWorldDirection(right, forward, -1f, 0f));
    }

    [Test]
    public void VerticalJoystickInputMovesForwardAndBackward()
    {
        Vector3 right = Vector3.right;
        Vector3 forward = Vector3.forward;

        Assert.AreEqual(Vector3.forward, MobileMovementVector.CalculateWorldDirection(right, forward, 0f, 1f));
        Assert.AreEqual(Vector3.back, MobileMovementVector.CalculateWorldDirection(right, forward, 0f, -1f));
    }

    [Test]
    public void DiagonalInputIsClamped()
    {
        Vector3 direction = MobileMovementVector.CalculateWorldDirection(Vector3.right, Vector3.forward, 1f, 1f);

        Assert.That(direction.magnitude, Is.LessThanOrEqualTo(1.0001f));
    }

    [Test]
    public void MovementIgnoresCameraPitchAndStaysOnGroundPlane()
    {
        Vector3 pitchedRight = new Vector3(1f, 0.25f, 0f);
        Vector3 pitchedForward = new Vector3(0f, 0.75f, 1f);

        Vector3 forward = MobileMovementVector.CalculateWorldDirection(pitchedRight, pitchedForward, 0f, 1f);
        Vector3 backward = MobileMovementVector.CalculateWorldDirection(pitchedRight, pitchedForward, 0f, -1f);

        Assert.That(forward.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(backward.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(forward.z, Is.GreaterThan(0f));
        Assert.That(backward.z, Is.LessThan(0f));
    }

    [Test]
    public void StrafeInputDoesNotAddForwardOrVerticalDrift()
    {
        Vector3 direction = MobileMovementVector.CalculateWorldDirection(Vector3.right, Vector3.forward, 1f, 0f);

        Assert.That(direction.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(direction.y, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(direction.z, Is.EqualTo(0f).Within(0.0001f));
    }
}
