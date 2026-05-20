using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class AndroidBuildConsistency
{
    public const string PackageName = "com.IanHeitor.AsylumHorror";
    public const string DefaultBuildDirectory = "Builds/Android";

    private static bool scriptBuildInProgress;

    public static bool ScriptBuildInProgress => scriptBuildInProgress;

    public static void BeginScriptBuild()
    {
        scriptBuildInProgress = true;
    }

    public static void EndScriptBuild()
    {
        scriptBuildInProgress = false;
    }

    public static void ApplySharedSettings()
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.MTRendering = true;

        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
    }

    [MenuItem("Tools/Android/Apply Shared Build Settings")]
    public static void ApplySharedSettingsFromMenu()
    {
        ApplySharedSettings();
        AndroidBrandingAndTypography.Apply();
        AssetDatabase.SaveAssets();
        Debug.Log("Android shared build settings applied. Editor builds and AndroidBuild.BuildApk now share the same Android configuration.");
    }

    public static string[] GetEnabledScenes()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("Nenhuma cena habilitada em EditorBuildSettings.");
        }

        return scenes;
    }

    public static int ResolveScriptVersionCode()
    {
        string versionCodeText = Environment.GetEnvironmentVariable("ANDROID_VERSION_CODE");
        if (!string.IsNullOrWhiteSpace(versionCodeText) && int.TryParse(versionCodeText, out int parsedVersionCode))
        {
            return parsedVersionCode;
        }

        return PlayerSettings.Android.bundleVersionCode + 1;
    }

    public static int PrepareEditorVersionCode()
    {
        int versionCode = PlayerSettings.Android.bundleVersionCode + 1;
        ApplyVersionCode(versionCode);
        return versionCode;
    }

    public static void ApplyVersionCode(int versionCode)
    {
        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.bundleVersion = $"1.0.{versionCode}";
    }

    public static BuildOptions ResolveBuildOptions()
    {
        return ResolveDevelopmentBuildFlag() ? BuildOptions.Development : BuildOptions.None;
    }

    public static bool ResolveDevelopmentBuildFlag()
    {
        string value = Environment.GetEnvironmentVariable("ANDROID_DEVELOPMENT_BUILD");
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value == "1" ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        return EditorUserBuildSettings.development;
    }

    public static string ResolveBuildDirectory()
    {
        string buildDir = Environment.GetEnvironmentVariable("ANDROID_BUILD_DIR");
        if (string.IsNullOrWhiteSpace(buildDir))
        {
            buildDir = Path.Combine(Directory.GetCurrentDirectory(), DefaultBuildDirectory);
        }

        Directory.CreateDirectory(buildDir);
        return buildDir;
    }
}

public sealed class AndroidEditorBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        AndroidBuildConsistency.ApplySharedSettings();

        if (!AndroidBuildConsistency.ScriptBuildInProgress)
        {
            int versionCode = AndroidBuildConsistency.PrepareEditorVersionCode();
            Debug.Log($"Android editor build version prepared: versionCode={versionCode}, versionName={PlayerSettings.bundleVersion}.");
        }

        AndroidBrandingAndTypography.Apply();
        AssetDatabase.SaveAssets();
        Debug.Log("Android build preflight applied: editor APK builds now use the same Android settings, branding and typography pipeline as AndroidBuild.BuildApk.");
    }
}
