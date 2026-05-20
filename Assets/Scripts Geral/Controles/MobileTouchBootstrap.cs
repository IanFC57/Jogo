using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MobileTouchBootstrap
{
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized) return;

        initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;

        Input.multiTouchEnabled = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        Input.simulateMouseWithTouches = false;
#else
        Input.simulateMouseWithTouches = true;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureLoadedScene()
    {
        ConfigureTouchUi();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureTouchUi();
    }

    private static void ConfigureTouchUi()
    {
        EnsureEventSystem();
        ConfigureCanvasScalers();
        SanitizeGraphicRaycasts();
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        eventSystem.enabled = true;
        inputModule.enabled = true;
        eventSystem.pixelDragThreshold = Mathf.Max(eventSystem.pixelDragThreshold, 12);
    }

    private static void ConfigureCanvasScalers()
    {
        foreach (CanvasScaler scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!scaler.gameObject.scene.isLoaded) continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static void SanitizeGraphicRaycasts()
    {
        foreach (Graphic graphic in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!graphic.gameObject.scene.isLoaded) continue;

            Selectable selectable = graphic.GetComponentInParent<Selectable>(true);
            bool isSelectableTarget = selectable != null &&
                                      (selectable.targetGraphic == graphic || selectable.targetGraphic == null);

            bool hasPointerHandler =
                graphic.GetComponentInParent<IPointerDownHandler>(true) != null ||
                graphic.GetComponentInParent<IPointerClickHandler>(true) != null ||
                graphic.GetComponentInParent<IDragHandler>(true) != null;

            graphic.raycastTarget = isSelectableTarget || hasPointerHandler;
        }
    }
}
