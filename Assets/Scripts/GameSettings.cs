using UnityEngine;

/// <summary>
/// Обёртка над PlayerPrefs для настроек игры.
/// </summary>
public static class GameSettings
{
    private const string KEY_ANIM = "Setting_Anim";
    private const string KEY_SOUND = "Setting_Sound";
    private const string KEY_MUSIC = "Setting_Music";

    public static bool AnimationEnabled
    {
        get => PlayerPrefs.GetInt(KEY_ANIM, 1) == 1;
        set { PlayerPrefs.SetInt(KEY_ANIM, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool SoundEnabled
    {
        get => PlayerPrefs.GetInt(KEY_SOUND, 1) == 1;
        set { PlayerPrefs.SetInt(KEY_SOUND, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool MusicEnabled
    {
        get => PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        set { PlayerPrefs.SetInt(KEY_MUSIC, value ? 1 : 0); PlayerPrefs.Save(); }
    }
}