using UnityEngine;

public static class HeadshotAudioFeedbackRuntimeProbe
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private static bool logged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void LogBundledHeadshotClipStatus()
    {
        if (logged)
            return;

        logged = true;
        AudioClip clip = Resources.Load<AudioClip>(HeadshotFeedbackRules.FallbackResourcePath);
        if (clip == null)
        {
            Debug.LogError($"HeadshotAudioFeedback: bundled {HeadshotFeedbackRules.FallbackSpokenWord} clip not found at Resources/{HeadshotFeedbackRules.FallbackResourcePath}.");
            return;
        }

        Debug.Log($"HeadshotAudioFeedback: bundled {HeadshotFeedbackRules.FallbackSpokenWord} clip loaded ({clip.length:0.00}s, {clip.frequency} Hz).");
    }
#endif
}
