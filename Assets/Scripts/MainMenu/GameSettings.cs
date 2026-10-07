using UnityEngine;

public static class GameSettings
{
    private const string VolumeKey = "PowerchairSoccer.MasterVolume";

    public static float MasterVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));

        set
        {
            float volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, volume);
            AudioListener.volume = volume;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Apply();
    }

    public static void Apply()
    {
        AudioListener.volume = MasterVolume;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}