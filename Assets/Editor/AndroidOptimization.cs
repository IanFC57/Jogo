using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

public static class AndroidOptimization
{
    [MenuItem("Tools/Android/Apply Performance Settings")]
    public static void ApplyPerformanceSettings()
    {
        ApplyPlayerSettings();
        ApplyQualitySettings();
        ApplyTextureSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Android performance settings applied.");
    }

    private static void ApplyPlayerSettings()
    {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = true;

        NamedBuildTarget android = NamedBuildTarget.Android;
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Medium);
        PlayerSettings.MTRendering = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidBuildConsistency.MinimumSupportedSdkVersion;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
    }

    private static void ApplyQualitySettings()
    {
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.softParticles = false;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.lodBias = 0.55f;
        QualitySettings.maximumLODLevel = 1;
        QualitySettings.asyncUploadTimeSlice = 2;
        QualitySettings.asyncUploadBufferSize = 8;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 128f;
        QualitySettings.streamingMipmapsRenderersPerFrame = 128;
        QualitySettings.streamingMipmapsMaxLevelReduction = 2;
        QualitySettings.streamingMipmapsAddAllCameras = true;
    }

    private static void ApplyTextureSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
        int changed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                TextureProfile profile = GetTextureProfile(path, importer);
                bool dirty = ApplyTextureProfile(importer, profile);

                if (dirty)
                {
                    changed++;
                    importer.SaveAndReimport();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        Debug.Log($"Android texture optimization updated {changed} texture importers.");
    }

    private static bool ApplyTextureProfile(TextureImporter importer, TextureProfile profile)
    {
        bool dirty = false;

        if (importer.isReadable)
        {
            importer.isReadable = false;
            dirty = true;
        }

        if (importer.mipmapEnabled != profile.UseMipmaps)
        {
            importer.mipmapEnabled = profile.UseMipmaps;
            dirty = true;
        }

        if (importer.streamingMipmaps != profile.UseStreamingMipmaps)
        {
            importer.streamingMipmaps = profile.UseStreamingMipmaps;
            dirty = true;
        }

        if (profile.UseMipmaps && importer.mipMapBias != profile.MipMapBias)
        {
            importer.mipMapBias = profile.MipMapBias;
            dirty = true;
        }

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        if (!android.overridden ||
            android.maxTextureSize != profile.MaxSize ||
            android.format != profile.Format ||
            android.textureCompression != TextureImporterCompression.Compressed ||
            android.compressionQuality != profile.CompressionQuality ||
            android.crunchedCompression)
        {
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = profile.MaxSize;
            android.format = profile.Format;
            android.textureCompression = TextureImporterCompression.Compressed;
            android.compressionQuality = profile.CompressionQuality;
            android.crunchedCompression = false;
            android.allowsAlphaSplitting = false;
            importer.SetPlatformTextureSettings(android);
            dirty = true;
        }

        return dirty;
    }

    private static TextureProfile GetTextureProfile(string path, TextureImporter importer)
    {
        string lowerPath = path.Replace('\\', '/').ToLowerInvariant();
        string fileName = System.IO.Path.GetFileNameWithoutExtension(lowerPath);

        bool isUi =
            importer.textureType == TextureImporterType.Sprite ||
            lowerPath.Contains("/bot") ||
            lowerPath.Contains("/joystick pack/") ||
            lowerPath.Contains("/textmesh pro/");

        bool isNormal =
            importer.textureType == TextureImporterType.NormalMap ||
            fileName.Contains("normal") ||
            fileName.EndsWith("_n", StringComparison.Ordinal);

        bool isMask =
            fileName.Contains("metallic") ||
            fileName.Contains("smoothness") ||
            fileName.Contains("roughness") ||
            fileName.Contains("specular") ||
            fileName.Contains("occlusion") ||
            fileName.Contains("_ao") ||
            fileName.Contains("mask") ||
            fileName.Contains("height");

        bool isMonster = lowerPath.Contains("/monster_scavenger/");
        bool isEnvironment = lowerPath.Contains("/cenario/") || lowerPath.Contains("/corredor/");

        if (isUi)
        {
            return new TextureProfile(1024, TextureImporterFormat.ASTC_6x6, false, false, 50, 0f);
        }

        if (isMask)
        {
            return new TextureProfile(isMonster ? 1024 : 512, TextureImporterFormat.ASTC_8x8, true, true, 40, 0.25f);
        }

        if (isNormal)
        {
            return new TextureProfile(isMonster ? 1024 : 1024, TextureImporterFormat.ASTC_6x6, true, true, 50, 0.15f);
        }

        if (isEnvironment)
        {
            return new TextureProfile(1024, TextureImporterFormat.ASTC_6x6, true, true, 50, 0.15f);
        }

        return new TextureProfile(1024, TextureImporterFormat.ASTC_6x6, true, true, 50, 0.15f);
    }

    private readonly struct TextureProfile
    {
        public TextureProfile(int maxSize, TextureImporterFormat format, bool useMipmaps, bool useStreamingMipmaps, int compressionQuality, float mipMapBias)
        {
            MaxSize = maxSize;
            Format = format;
            UseMipmaps = useMipmaps;
            UseStreamingMipmaps = useStreamingMipmaps;
            CompressionQuality = compressionQuality;
            MipMapBias = mipMapBias;
        }

        public int MaxSize { get; }
        public TextureImporterFormat Format { get; }
        public bool UseMipmaps { get; }
        public bool UseStreamingMipmaps { get; }
        public int CompressionQuality { get; }
        public float MipMapBias { get; }
    }
}
