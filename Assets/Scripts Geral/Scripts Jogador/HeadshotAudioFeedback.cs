using UnityEngine;

public sealed class HeadshotAudioFeedback : MonoBehaviour
{
    public AudioClip headshotClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool usarFallbackOriginal = true;

    private static AudioClip fallbackClip;
    private static AudioClip resourceClip;
    private static AudioSource globalSource;

    public static void PlayAt(Vector3 fallbackWorldPosition)
    {
        HeadshotAudioFeedback feedback = UnityEngine.Object.FindFirstObjectByType<HeadshotAudioFeedback>(FindObjectsInactive.Exclude);
        if (feedback != null)
        {
            feedback.Play(fallbackWorldPosition);
            return;
        }

        AudioClip clip = GetBundledClip();
        if (clip == null)
        {
            clip = GetFallbackClip();
        }

        PlayClip(clip, 1f, HeadshotFeedbackRules.DefaultAnnouncerPitch);
    }

    public void Play(Vector3 fallbackWorldPosition)
    {
        AudioClip clip = headshotClip;
        float clipPitch = 1f;
        if (clip == null)
        {
            clip = GetBundledClip();
            clipPitch = HeadshotFeedbackRules.DefaultAnnouncerPitch;
        }

        if (clip == null && usarFallbackOriginal)
        {
            clip = GetFallbackClip();
            clipPitch = HeadshotFeedbackRules.DefaultAnnouncerPitch;
        }

        if (clip == null)
            return;

        PlayClip(clip, volume, clipPitch);
    }

    private static void PlayClip(AudioClip clip, float volume, float pitch)
    {
        if (clip == null)
            return;

        AudioSource source = GetOrCreateGlobalSource();
        source.pitch = Mathf.Clamp(pitch, 0.5f, 1.5f);
        source.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private static AudioClip GetBundledClip()
    {
        if (resourceClip != null)
            return resourceClip;

        resourceClip = Resources.Load<AudioClip>(HeadshotFeedbackRules.FallbackResourcePath);
        return resourceClip;
    }

    private static AudioClip GetFallbackClip()
    {
        if (fallbackClip != null)
            return fallbackClip;

        const int sampleRate = 22050;
        const float duration = 0.9f;
        int totalSamples = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[totalSamples];

        AddNoiseSegment(samples, sampleRate, 0.00f, 0.09f, 0.24f, 1.8f); // H
        AddVowelSegment(samples, sampleRate, 0.07f, 0.24f, 145f, 530f, 1840f, 2480f, 0.55f); // HE
        AddStopClick(samples, sampleRate, 0.25f, 0.35f);
        AddNoiseSegment(samples, sampleRate, 0.31f, 0.48f, 0.58f, 6.5f); // SH
        AddVowelSegment(samples, sampleRate, 0.43f, 0.71f, 125f, 650f, 1180f, 2600f, 0.62f); // O
        AddStopClick(samples, sampleRate, 0.72f, 0.55f); // T
        AddLowAnnouncerBody(samples, sampleRate, 0.05f, 0.78f);
        Normalize(samples, 0.92f);

        fallbackClip = AudioClip.Create("OriginalAnnouncer_HEADSHOT", totalSamples, 1, sampleRate, false);
        fallbackClip.SetData(samples, 0);
        return fallbackClip;
    }

    private static AudioSource GetOrCreateGlobalSource()
    {
        if (globalSource != null)
            return globalSource;

        GameObject audioObject = new GameObject("HeadshotAudioFeedback_2D");
        DontDestroyOnLoad(audioObject);

        globalSource = audioObject.AddComponent<AudioSource>();
        globalSource.playOnAwake = false;
        globalSource.loop = false;
        globalSource.spatialBlend = 0f;
        globalSource.priority = 16;
        globalSource.volume = 1f;
        return globalSource;
    }

    private static void AddVowelSegment(
        float[] samples,
        int sampleRate,
        float start,
        float end,
        float pitch,
        float formantA,
        float formantB,
        float formantC,
        float gain)
    {
        int startSample = Mathf.Clamp(Mathf.FloorToInt(start * sampleRate), 0, samples.Length);
        int endSample = Mathf.Clamp(Mathf.FloorToInt(end * sampleRate), startSample, samples.Length);
        int length = Mathf.Max(1, endSample - startSample);

        for (int i = startSample; i < endSample; i++)
        {
            float local = (i - startSample) / (float)length;
            float t = i / (float)sampleRate;
            float envelope = Mathf.Sin(local * Mathf.PI);
            float voice = Mathf.Sin(2f * Mathf.PI * pitch * t) * 0.22f;
            voice += Mathf.Sin(2f * Mathf.PI * pitch * 2f * t) * 0.09f;
            voice += Mathf.Sin(2f * Mathf.PI * pitch * 3f * t) * 0.05f;

            float formants = Mathf.Sin(2f * Mathf.PI * formantA * t) * 0.14f;
            formants += Mathf.Sin(2f * Mathf.PI * formantB * t) * 0.08f;
            formants += Mathf.Sin(2f * Mathf.PI * formantC * t) * 0.04f;
            samples[i] += (voice + formants) * envelope * gain;
        }
    }

    private static void AddNoiseSegment(float[] samples, int sampleRate, float start, float end, float gain, float roughness)
    {
        int startSample = Mathf.Clamp(Mathf.FloorToInt(start * sampleRate), 0, samples.Length);
        int endSample = Mathf.Clamp(Mathf.FloorToInt(end * sampleRate), startSample, samples.Length);
        int length = Mathf.Max(1, endSample - startSample);
        uint state = 0x6D2B79F5u;

        for (int i = startSample; i < endSample; i++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            float noise = ((state & 0xffff) / 32768f) - 1f;
            float local = (i - startSample) / (float)length;
            float t = i / (float)sampleRate;
            float envelope = Mathf.Sin(local * Mathf.PI);
            float hissShape = Mathf.Sin(2f * Mathf.PI * roughness * 1000f * t) * 0.35f + 0.65f;
            samples[i] += noise * hissShape * envelope * gain;
        }
    }

    private static void AddStopClick(float[] samples, int sampleRate, float time, float gain)
    {
        int startSample = Mathf.Clamp(Mathf.FloorToInt(time * sampleRate), 0, samples.Length);
        int length = Mathf.Min(Mathf.CeilToInt(sampleRate * 0.035f), samples.Length - startSample);

        for (int i = 0; i < length; i++)
        {
            float local = i / (float)Mathf.Max(1, length);
            float envelope = 1f - local;
            float t = (startSample + i) / (float)sampleRate;
            samples[startSample + i] += Mathf.Sin(2f * Mathf.PI * 180f * t) * envelope * gain;
        }
    }

    private static void AddLowAnnouncerBody(float[] samples, int sampleRate, float start, float end)
    {
        int startSample = Mathf.Clamp(Mathf.FloorToInt(start * sampleRate), 0, samples.Length);
        int endSample = Mathf.Clamp(Mathf.FloorToInt(end * sampleRate), startSample, samples.Length);
        int length = Mathf.Max(1, endSample - startSample);

        for (int i = startSample; i < endSample; i++)
        {
            float local = (i - startSample) / (float)length;
            float t = i / (float)sampleRate;
            float envelope = Mathf.Sin(local * Mathf.PI);
            samples[i] += Mathf.Sin(2f * Mathf.PI * 74f * t) * envelope * 0.12f;
        }
    }

    private static void Normalize(float[] samples, float peak)
    {
        float max = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            max = Mathf.Max(max, Mathf.Abs(samples[i]));
        }

        if (max <= 0.0001f)
            return;

        float scale = Mathf.Clamp01(peak) / max;
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= scale;
        }
    }
}
