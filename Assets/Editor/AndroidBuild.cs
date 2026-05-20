using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuild
{
    public static void BuildApk()
    {
        string buildDir = Environment.GetEnvironmentVariable("ANDROID_BUILD_DIR");
        if (string.IsNullOrWhiteSpace(buildDir))
        {
            buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android");
        }

        Directory.CreateDirectory(buildDir);

        string versionCodeText = Environment.GetEnvironmentVariable("ANDROID_VERSION_CODE");
        int versionCode = PlayerSettings.Android.bundleVersionCode + 1;
        if (!string.IsNullOrWhiteSpace(versionCodeText) && int.TryParse(versionCodeText, out int parsedVersionCode))
        {
            versionCode = parsedVersionCode;
        }

        PlayerSettings.Android.bundleVersionCode = versionCode;
        PlayerSettings.bundleVersion = $"1.0.{versionCode}";
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
        }

        string apkPath = Path.Combine(buildDir, $"AsylumHorror-{versionCode}.apk");
        BuildOptions buildOptions = ShouldBuildDevelopment()
            ? BuildOptions.Development
            : BuildOptions.None;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"Android build result: {summary.result}, output: {apkPath}, size: {summary.totalSize}");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Android build failed: {summary.result}");
        }
    }

    private static bool ShouldBuildDevelopment()
    {
        string value = Environment.GetEnvironmentVariable("ANDROID_DEVELOPMENT_BUILD");
        return value == "1" ||
            value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true ||
            value?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true;
    }
}
