using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuild
{
    [MenuItem("Tools/Android/Build APK Igual Ao Script")]
    public static void BuildApk()
    {
        string buildDir = AndroidBuildConsistency.ResolveBuildDirectory();
        int versionCode = AndroidBuildConsistency.ResolveScriptVersionCode();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        AndroidBuildConsistency.ApplyVersionCode(versionCode);
        AndroidBuildConsistency.ApplySharedSettings();
        AndroidBrandingAndTypography.Apply();

        string[] scenes = AndroidBuildConsistency.GetEnabledScenes();

        string apkPath = Path.Combine(buildDir, $"AsylumHorror-{versionCode}.apk");
        BuildOptions buildOptions = AndroidBuildConsistency.ResolveBuildOptions();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = buildOptions
        };

        AndroidBuildConsistency.BeginScriptBuild();
        BuildReport report;
        try
        {
            report = BuildPipeline.BuildPlayer(options);
        }
        finally
        {
            AndroidBuildConsistency.EndScriptBuild();
        }

        BuildSummary summary = report.summary;
        Debug.Log($"Android build result: {summary.result}, output: {apkPath}, size: {summary.totalSize}");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Android build failed: {summary.result}");
        }
    }
}
