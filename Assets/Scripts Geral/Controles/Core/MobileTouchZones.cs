using UnityEngine;

public static class MobileTouchZones
{
    public const float MovementMinX = 0f;
    public const float MovementMaxX = 0.42f;
    public const float MovementMinY = 0f;
    public const float MovementMaxY = 0.58f;
    public const float FireMinX = 0.76f;
    public const float FireMaxX = 1f;
    public const float FireMinY = 0.06f;
    public const float FireMaxY = 0.34f;
    public const float ReloadMinX = 0.60f;
    public const float ReloadMaxX = 0.84f;
    public const float ReloadMinY = 0.06f;
    public const float ReloadMaxY = 0.34f;

    public static bool IsMovementZone(Vector2 screenPosition)
    {
        return IsMovementZone(screenPosition, GetScreenSize(), Screen.safeArea);
    }

    public static bool IsMovementZone(Vector2 screenPosition, Vector2 screenSize, Rect safeArea)
    {
        return IsInNormalizedZone(screenPosition, screenSize, safeArea, MovementMinX, MovementMaxX, MovementMinY, MovementMaxY);
    }

    public static bool IsFireZone(Vector2 screenPosition)
    {
        return IsFireZone(screenPosition, GetScreenSize(), Screen.safeArea);
    }

    public static bool IsFireZone(Vector2 screenPosition, Vector2 screenSize, Rect safeArea)
    {
        return IsInNormalizedZone(screenPosition, screenSize, safeArea, FireMinX, FireMaxX, FireMinY, FireMaxY);
    }

    public static bool IsReloadZone(Vector2 screenPosition)
    {
        return IsReloadZone(screenPosition, GetScreenSize(), Screen.safeArea);
    }

    public static bool IsReloadZone(Vector2 screenPosition, Vector2 screenSize, Rect safeArea)
    {
        return IsInNormalizedZone(screenPosition, screenSize, safeArea, ReloadMinX, ReloadMaxX, ReloadMinY, ReloadMaxY);
    }

    public static bool IsReservedControlZone(Vector2 screenPosition)
    {
        return IsReservedControlZone(screenPosition, GetScreenSize(), Screen.safeArea);
    }

    public static bool IsReservedControlZone(Vector2 screenPosition, Vector2 screenSize, Rect safeArea)
    {
        return IsMovementZone(screenPosition, screenSize, safeArea) ||
               IsFireZone(screenPosition, screenSize, safeArea) ||
               IsReloadZone(screenPosition, screenSize, safeArea);
    }

    public static bool IsCameraZone(Vector2 screenPosition, Rect normalizedCameraArea)
    {
        return IsCameraZone(screenPosition, GetScreenSize(), Screen.safeArea, normalizedCameraArea);
    }

    public static bool IsCameraZone(Vector2 screenPosition, Vector2 screenSize, Rect safeArea, Rect normalizedCameraArea)
    {
        return IsInNormalizedZone(
            screenPosition,
            screenSize,
            safeArea,
            normalizedCameraArea.xMin,
            normalizedCameraArea.xMax,
            normalizedCameraArea.yMin,
            normalizedCameraArea.yMax);
    }

    public static bool IsInNormalizedZone(
        Vector2 screenPosition,
        Vector2 screenSize,
        Rect safeArea,
        float minX,
        float maxX,
        float minY,
        float maxY)
    {
        return IsNormalizedInside(NormalizedPosition(screenPosition, screenSize), minX, maxX, minY, maxY) ||
               IsNormalizedInside(NormalizedSafeAreaPosition(screenPosition, screenSize, safeArea), minX, maxX, minY, maxY);
    }

    private static bool IsNormalizedInside(Vector2 normalized, float minX, float maxX, float minY, float maxY)
    {
        return normalized.x >= minX &&
               normalized.x <= maxX &&
               normalized.y >= minY &&
               normalized.y <= maxY;
    }

    private static Vector2 NormalizedPosition(Vector2 screenPosition, Vector2 screenSize)
    {
        float width = Mathf.Max(1f, screenSize.x);
        float height = Mathf.Max(1f, screenSize.y);
        return new Vector2(screenPosition.x / width, screenPosition.y / height);
    }

    private static Vector2 NormalizedSafeAreaPosition(Vector2 screenPosition, Vector2 screenSize, Rect safeArea)
    {
        if (safeArea.width <= 1f || safeArea.height <= 1f)
        {
            return NormalizedPosition(screenPosition, screenSize);
        }

        return new Vector2(
            (screenPosition.x - safeArea.xMin) / safeArea.width,
            (screenPosition.y - safeArea.yMin) / safeArea.height);
    }

    private static Vector2 GetScreenSize()
    {
        return new Vector2(Mathf.Max(1f, Screen.width), Mathf.Max(1f, Screen.height));
    }
}
