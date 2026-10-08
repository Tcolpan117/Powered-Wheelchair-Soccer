using UnityEngine;

public static class GameSettings
{
    private const string VolumeKey = "PowerchairSoccer.MasterVolume";

    /// <summary>How much one press of the menu's volume buttons changes the volume.</summary>
    public const float VolumeStep = 0.1f;

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

    /// <summary>Master volume as a whole number from 0 to 100, for display.</summary>
    public static int MasterVolumePercent => Mathf.RoundToInt(MasterVolume * 100f);

    /// <summary>Changes the volume, rounds away float drift (0.1 + 0.1 + 0.1), and saves.</summary>
    public static void AdjustMasterVolume(float amount)
    {
        MasterVolume = Mathf.Round((MasterVolume + amount) * 100f) / 100f;
        Save();
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