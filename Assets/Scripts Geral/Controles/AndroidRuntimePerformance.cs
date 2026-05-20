using UnityEngine;

public static class AndroidRuntimePerformance
{
    private const int TargetFrameRate = 30;
    private const float RenderScale = 0.75f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyBeforeSceneLoad()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ApplyGlobalSettings();
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Apply()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ApplyGlobalSettings();

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!camera.gameObject.scene.isLoaded) continue;

            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = true;
            camera.renderingPath = RenderingPath.Forward;
            camera.useOcclusionCulling = true;

            if (camera.farClipPlane > 120f)
            {
                camera.farClipPlane = 120f;
            }
        }

        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!light.gameObject.scene.isLoaded) continue;

            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForceVertex;
        }

        foreach (ReflectionProbe probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!probe.gameObject.scene.isLoaded) continue;

            probe.enabled = false;
        }
#endif
    }

    private static void ApplyGlobalSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        QualitySettings.SetQualityLevel(0, true);
        Application.targetFrameRate = TargetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Time.fixedDeltaTime = 1f / TargetFrameRate;
        Time.maximumDeltaTime = 0.1f;

        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.softParticles = false;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.lodBias = 0.55f;
        QualitySettings.maximumLODLevel = 1;
        QualitySettings.skinWeights = SkinWeights.TwoBones;
        QualitySettings.asyncUploadTimeSlice = 2;
        QualitySettings.asyncUploadBufferSize = 8;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 128f;
        QualitySettings.streamingMipmapsRenderersPerFrame = 128;
        QualitySettings.streamingMipmapsMaxLevelReduction = 2;

        ScalableBufferManager.ResizeBuffers(RenderScale, RenderScale);
#endif
    }
}
