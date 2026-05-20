using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class MobileTouchInputBridge : MonoBehaviour
{
    private const float MaxTapMovePixels = 80f;
    private const float HitPaddingPixels = 36f;
    private const float MaxTapMoveScreenFraction = 0.08f;
    private const float HitPaddingScreenFraction = 0.04f;

    private static MobileTouchInputBridge instance;
    private static readonly HashSet<int> CapturedFingerIds = new HashSet<int>();

    private readonly Dictionary<int, TouchCapture> captures = new Dictionary<int, TouchCapture>();
    private readonly List<Button> buttons = new List<Button>();
    private readonly List<Joystick> joysticks = new List<Joystick>();
    private readonly List<int> endedThisFrame = new List<int>();

    private float nextRescanTime;
    private EventSystem eventSystem;
    private bool loggedScreenMetrics;

    private static readonly FieldInfo JoystickBackgroundField =
        typeof(Joystick).GetField("background", BindingFlags.Instance | BindingFlags.NonPublic);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (instance != null) return;

        GameObject bridgeObject = new GameObject(nameof(MobileTouchInputBridge));
        DontDestroyOnLoad(bridgeObject);
        instance = bridgeObject.AddComponent<MobileTouchInputBridge>();
#endif
    }

    public static bool IsTouchCapturedByMobileUi(int fingerId)
    {
        return CapturedFingerIds.Contains(fingerId);
    }

    public static bool IsScreenPositionReservedForMobileUi(Vector2 screenPosition)
    {
        if (!Application.isMobilePlatform) return false;

        if (instance != null)
        {
            if (instance.FindJoystickAt(screenPosition) != null) return true;
            if (instance.FindButtonAt(screenPosition) != null) return true;
        }

        return MobileTouchZones.IsReservedControlZone(screenPosition);
    }

    public static bool IsScreenPositionReservedForMovement(Vector2 screenPosition)
    {
        if (!Application.isMobilePlatform) return false;

        if (instance != null && instance.FindJoystickAt(screenPosition) != null)
        {
            return true;
        }

        return MobileTouchZones.IsMovementZone(screenPosition);
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Rescan();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Rescan();
    }

    private void Update()
    {
        ReleasePointerCaptureForTouch();

        if (Time.unscaledTime >= nextRescanTime)
        {
            Rescan();
        }

        endedThisFrame.Clear();

        for (int i = 0; i < Input.touchCount; i++)
        {
            ProcessTouch(Input.GetTouch(i));
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < endedThisFrame.Count; i++)
        {
            CapturedFingerIds.Remove(endedThisFrame[i]);
            captures.Remove(endedThisFrame[i]);
        }
    }

    private void Rescan()
    {
        nextRescanTime = Time.unscaledTime + 1f;
        eventSystem = EventSystem.current;

        buttons.Clear();
        buttons.AddRange(FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

        joysticks.Clear();
        joysticks.AddRange(FindObjectsByType<Joystick>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

        DisableUnityUiInputModules();
        LogScreenMetricsOnce();
    }

    private void DisableUnityUiInputModules()
    {
        if (eventSystem == null) return;

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        for (int i = 0; i < modules.Length; i++)
        {
            modules[i].enabled = false;
        }
    }

    private void ProcessTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginTouch(touch);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                MoveTouch(touch);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTouch(touch);
                break;
        }
    }

    private void BeginTouch(Touch touch)
    {
        TouchCapture capture = new TouchCapture
        {
            FingerId = touch.fingerId,
            StartPosition = touch.position,
            LastPosition = touch.position
        };

        Joystick joystick = FindJoystickAt(touch.position);
        if (joystick != null)
        {
            capture.Joystick = joystick;
            captures[touch.fingerId] = capture;
            CapturedFingerIds.Add(touch.fingerId);
            DispatchJoystickDown(joystick, touch);
            return;
        }

        Button button = FindButtonAt(touch.position);
        if (button != null)
        {
            capture.Button = button;
            captures[touch.fingerId] = capture;
            CapturedFingerIds.Add(touch.fingerId);
        }
    }

    private void MoveTouch(Touch touch)
    {
        if (!captures.TryGetValue(touch.fingerId, out TouchCapture capture)) return;

        capture.LastPosition = touch.position;
        captures[touch.fingerId] = capture;

        if (capture.Joystick != null)
        {
            DispatchJoystickDrag(capture.Joystick, touch, capture);
        }
    }

    private void EndTouch(Touch touch)
    {
        if (!captures.TryGetValue(touch.fingerId, out TouchCapture capture)) return;

        if (capture.Joystick != null)
        {
            DispatchJoystickUp(capture.Joystick, touch, capture);
        }
        else if (capture.Button != null &&
                 capture.Button.IsActive() &&
                 capture.Button.IsInteractable() &&
                 Vector2.Distance(capture.StartPosition, touch.position) <= GetMaxTapMovePixels() &&
                 IsInsideButtonReleaseZone(capture.Button, touch.position))
        {
            capture.Button.onClick.Invoke();
            Debug.Log($"Mobile touch invoked button: {capture.Button.name}");
        }

        endedThisFrame.Add(touch.fingerId);
    }

    private Button FindButtonAt(Vector2 screenPosition)
    {
        Button best = null;
        int bestDepth = int.MinValue;

        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.IsActive() || !button.IsInteractable()) continue;

            RectTransform rect = button.transform as RectTransform;
            if (!IsInsideRect(rect, screenPosition)) continue;

            int depth = GetHierarchyDepth(button.transform);
            if (depth >= bestDepth)
            {
                bestDepth = depth;
                best = button;
            }
        }

        return best != null ? best : FindNamedMobileButtonAt(screenPosition);
    }

    private Joystick FindJoystickAt(Vector2 screenPosition)
    {
        for (int i = joysticks.Count - 1; i >= 0; i--)
        {
            Joystick joystick = joysticks[i];
            if (joystick == null || !joystick.isActiveAndEnabled) continue;

            RectTransform rect = GetJoystickHitRect(joystick);
            if (IsInsideRect(rect, screenPosition))
            {
                return joystick;
            }
        }

        if (MobileTouchZones.IsMovementZone(screenPosition) && joysticks.Count > 0)
        {
            return joysticks[0];
        }

        return null;
    }

    private Button FindNamedMobileButtonAt(Vector2 screenPosition)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.IsActive() || !button.IsInteractable()) continue;

            if (IsNamedMobileButton(button, screenPosition))
            {
                return button;
            }
        }

        return null;
    }

    private static bool IsInsideButtonReleaseZone(Button button, Vector2 screenPosition)
    {
        return IsInsideRect(button.transform as RectTransform, screenPosition) ||
               IsNamedMobileButton(button, screenPosition);
    }

    private static bool IsNamedMobileButton(Button button, Vector2 screenPosition)
    {
        if (button == null) return false;

        string name = button.name.ToLowerInvariant();

        if ((name.Contains("atirar") || name.Contains("fire")) &&
            MobileTouchZones.IsFireZone(screenPosition))
        {
            return true;
        }

        if ((name.Contains("recarregar") || name.Contains("reload")) &&
            MobileTouchZones.IsReloadZone(screenPosition))
        {
            return true;
        }

        return false;
    }

    private static RectTransform GetJoystickHitRect(Joystick joystick)
    {
        RectTransform background = JoystickBackgroundField?.GetValue(joystick) as RectTransform;
        return background != null ? background : joystick.transform as RectTransform;
    }

    private static bool IsInsideRect(RectTransform rect, Vector2 screenPosition)
    {
        if (rect == null) return false;

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            minX = Mathf.Min(minX, screenCorner.x);
            minY = Mathf.Min(minY, screenCorner.y);
            maxX = Mathf.Max(maxX, screenCorner.x);
            maxY = Mathf.Max(maxY, screenCorner.y);
        }

        float padding = GetHitPaddingPixels();
        return screenPosition.x >= minX - padding &&
               screenPosition.x <= maxX + padding &&
               screenPosition.y >= minY - padding &&
               screenPosition.y <= maxY + padding;
    }

    private static float GetMaxTapMovePixels()
    {
        return Mathf.Max(MaxTapMovePixels, Mathf.Min(Screen.width, Screen.height) * MaxTapMoveScreenFraction);
    }

    private static float GetHitPaddingPixels()
    {
        return Mathf.Max(HitPaddingPixels, Mathf.Min(Screen.width, Screen.height) * HitPaddingScreenFraction);
    }

    private void LogScreenMetricsOnce()
    {
        if (loggedScreenMetrics) return;

        loggedScreenMetrics = true;
        Rect safeArea = Screen.safeArea;
        Debug.Log(
            $"MobileTouchInputBridge screen={Screen.width}x{Screen.height}, " +
            $"safeArea=({safeArea.x},{safeArea.y},{safeArea.width},{safeArea.height}), " +
            $"buttons={buttons.Count}, joysticks={joysticks.Count}");
    }

    private static void ReleasePointerCaptureForTouch()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private PointerEventData CreatePointerEvent(Touch touch, TouchCapture capture)
    {
        return new PointerEventData(eventSystem)
        {
            pointerId = touch.fingerId,
            position = touch.position,
            pressPosition = capture.StartPosition,
            delta = touch.position - capture.LastPosition,
            button = PointerEventData.InputButton.Left
        };
    }

    private void DispatchJoystickDown(Joystick joystick, Touch touch)
    {
        TouchCapture capture = captures[touch.fingerId];
        joystick.OnPointerDown(CreatePointerEvent(touch, capture));
    }

    private void DispatchJoystickDrag(Joystick joystick, Touch touch, TouchCapture capture)
    {
        joystick.OnDrag(CreatePointerEvent(touch, capture));
    }

    private void DispatchJoystickUp(Joystick joystick, Touch touch, TouchCapture capture)
    {
        joystick.OnPointerUp(CreatePointerEvent(touch, capture));
    }

    private static int GetHierarchyDepth(Transform transform)
    {
        int depth = 0;
        while (transform != null)
        {
            depth++;
            transform = transform.parent;
        }

        return depth;
    }

    private struct TouchCapture
    {
        public int FingerId;
        public Vector2 StartPosition;
        public Vector2 LastPosition;
        public Button Button;
        public Joystick Joystick;
    }
}
