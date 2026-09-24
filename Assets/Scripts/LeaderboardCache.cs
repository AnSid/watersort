using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Локальный кэш топ-50 рейтинга (PlayerPrefs).
/// Хранит последний успешный ответ сервера.
/// </summary>
public static class LeaderboardCache
{
    private const string KEY = "LeaderboardCache";

    [System.Serializable]
    private class CacheWrapper
    {
        public List<LeaderboardRecord> items;
        public long timestamp;
    }

    public static void Save(List<LeaderboardRecord> records)
    {
        if (records == null) return;
        CacheWrapper w = new CacheWrapper
        {
            items = records,
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        string json = JsonUtility.ToJson(w);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public static List<LeaderboardRecord> Load()
    {
        string json = PlayerPrefs.GetString(KEY, "");
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            CacheWrapper w = JsonUtility.FromJson<CacheWrapper>(json);
            return w?.items;
        }
        catch
        {
            return null;
        }
    }

    public static bool HasCache()
    {
        return PlayerPrefs.HasKey(KEY);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Позиция игрока в закэшированном топе.
    /// Возвращает 1-based позицию или -1, если игрок не найден.
    /// </summary>
    public static int GetPlayerPosition(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId)) return -1;
        var list = Load();
        if (list == null) return -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].device_id == deviceId)
                return i + 1;
        }
        return -1;
    }
}