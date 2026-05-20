using System.Collections.Generic;
using UnityEngine;

public sealed class MobileCameraTouchPolicy
{
    private readonly HashSet<int> blockedFingerIds = new HashSet<int>();
    private int cameraFingerId = -1;
    private Vector2 lastCameraPosition;

    public bool HasCameraFinger => cameraFingerId != -1;
    public bool IsFingerBlocked(int fingerId) => blockedFingerIds.Contains(fingerId);

    public void Reset()
    {
        blockedFingerIds.Clear();
        cameraFingerId = -1;
        lastCameraPosition = Vector2.zero;
    }

    public Vector2 ProcessTouch(
        MobileTouchSample touch,
        Vector2 screenSize,
        Rect safeArea,
        Rect normalizedCameraArea,
        bool capturedBySceneUi,
        bool pointerOverEventSystem)
    {
        if (touch.Phase == TouchPhase.Began)
        {
            if (ShouldBlockForCamera(touch.Position, screenSize, safeArea, capturedBySceneUi, pointerOverEventSystem))
            {
                blockedFingerIds.Add(touch.FingerId);
                if (cameraFingerId == touch.FingerId)
                {
                    cameraFingerId = -1;
                }

                return Vector2.zero;
            }

            if (cameraFingerId == -1 &&
                MobileTouchZones.IsCameraZone(touch.Position, screenSize, safeArea, normalizedCameraArea))
            {
                cameraFingerId = touch.FingerId;
                lastCameraPosition = touch.Position;
            }

            return Vector2.zero;
        }

        if (blockedFingerIds.Contains(touch.FingerId))
        {
            if (touch.Phase == TouchPhase.Ended || touch.Phase == TouchPhase.Canceled)
            {
                blockedFingerIds.Remove(touch.FingerId);
            }

            return Vector2.zero;
        }

        if (touch.FingerId != cameraFingerId)
        {
            return Vector2.zero;
        }

        if (ShouldBlockForCamera(touch.Position, screenSize, safeArea, capturedBySceneUi, pointerOverEventSystem))
        {
            cameraFingerId = -1;
            blockedFingerIds.Add(touch.FingerId);
            return Vector2.zero;
        }

        if (touch.Phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.Position - lastCameraPosition;
            lastCameraPosition = touch.Position;
            return delta;
        }

        if (touch.Phase == TouchPhase.Stationary)
        {
            lastCameraPosition = touch.Position;
            return Vector2.zero;
        }

        if (touch.Phase == TouchPhase.Ended || touch.Phase == TouchPhase.Canceled)
        {
            cameraFingerId = -1;
        }

        return Vector2.zero;
    }

    private static bool ShouldBlockForCamera(
        Vector2 screenPosition,
        Vector2 screenSize,
        Rect safeArea,
        bool capturedBySceneUi,
        bool pointerOverEventSystem)
    {
        return capturedBySceneUi ||
               pointerOverEventSystem ||
               MobileTouchZones.IsReservedControlZone(screenPosition, screenSize, safeArea);
    }
}
