using UnityEngine;

public static class PlayerProfile
{
    private const string KEY_NAME = "PlayerName";
    private const string KEY_COUNTRY = "PlayerCountry";
    private const string KEY_SETUP_DONE = "PlayerSetupDone";
    private const string KEY_DEVICE_ID = "PlayerDeviceId";

    public static string PlayerName
    {
        get
        {
            string saved = PlayerPrefs.GetString(KEY_NAME, "");
            if (string.IsNullOrEmpty(saved))
            {
                saved = "Player_" + Random.Range(1000, 9999);
                PlayerPrefs.SetString(KEY_NAME, saved);
                PlayerPrefs.Save();
            }
            return saved;
        }
        set
        {
            PlayerPrefs.SetString(KEY_NAME, value);
            PlayerPrefs.Save();
        }
    }

    public static string CountryCode
    {
        get
        {
            string saved = PlayerPrefs.GetString(KEY_COUNTRY, "");
            if (string.IsNullOrEmpty(saved))
            {
                saved = CountryData.GetDefaultCountryCode();
                PlayerPrefs.SetString(KEY_COUNTRY, saved);
                PlayerPrefs.Save();
            }
            return saved;
        }
        set
        {
            PlayerPrefs.SetString(KEY_COUNTRY, value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsSetupDone
    {
        get => PlayerPrefs.GetInt(KEY_SETUP_DONE, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(KEY_SETUP_DONE, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Уникальный ID устройства. Генерируется один раз и никогда не меняется.
    /// Используется для идентификации игрока в рейтинге.
    /// </summary>
    public static string DeviceId
    {
        get
        {
            string saved = PlayerPrefs.GetString(KEY_DEVICE_ID, "");
            if (string.IsNullOrEmpty(saved))
            {
                saved = SystemInfo.deviceUniqueIdentifier;
                if (string.IsNullOrEmpty(saved) || saved == SystemInfo.unsupportedIdentifier)
                    saved = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString(KEY_DEVICE_ID, saved);
                PlayerPrefs.Save();
            }
            return saved;
        }
    }
}