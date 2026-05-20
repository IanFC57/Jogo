using UnityEngine;

public static class MobileMovementVector
{
    public static Vector2 ClampInput(float horizontal, float vertical)
    {
        return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
    }

    public static Vector3 CalculateWorldDirection(Vector3 right, Vector3 forward, float horizontal, float vertical)
    {
        Vector2 input = ClampInput(horizontal, vertical);

        Vector3 planarRight = Vector3.ProjectOnPlane(right, Vector3.up).normalized;
        Vector3 planarForward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

        Vector3 direction = planarRight * input.x + planarForward * input.y;
        return Vector3.ClampMagnitude(direction, 1f);
    }
}
