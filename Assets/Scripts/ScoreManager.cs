using UnityEngine;

/// <summary>
/// Управление накопленным счётом игрока.
/// 
/// Счёт — это сумма баллов за все ВПЕРВЫЕ пройденные уровни.
/// Повторное прохождение баллов не даёт (MaxLevelReached).
/// Сброс прогресса счёт НЕ трогает.
/// 
/// Хранится в PlayerPrefs:
///   WaterSort_TotalScore      — накопленный счёт
///   WaterSort_MaxLevelReached — максимальный уровень, за который начислены баллы
///   WaterSort_ScoreDirty      — 1, если есть несинхронизированные баллы
/// </summary>
public static class ScoreManager
{
    private const string KEY_TOTAL = "WaterSort_TotalScore";
    private const string KEY_MAX_LEVEL = "WaterSort_MaxLevelReached";
    private const string KEY_DIRTY = "WaterSort_ScoreDirty";

    public static int TotalScore
    {
        get => PlayerPrefs.GetInt(KEY_TOTAL, 0);
        private set { PlayerPrefs.SetInt(KEY_TOTAL, value); PlayerPrefs.Save(); }
    }

    public static int MaxLevelReached
    {
        get => PlayerPrefs.GetInt(KEY_MAX_LEVEL, 0);
        private set { PlayerPrefs.SetInt(KEY_MAX_LEVEL, value); PlayerPrefs.Save(); }
    }

    public static bool IsDirty
    {
        get => PlayerPrefs.GetInt(KEY_DIRTY, 0) == 1;
        private set { PlayerPrefs.SetInt(KEY_DIRTY, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// <summary>
    /// Баллы за уровень (ступенями). Легко правится в одном месте.
    /// </summary>
    public static int GetPointsForLevel(int level)
    {
        if (level <= 2) return 10;
        if (level <= 4) return 20;
        if (level <= 6) return 30;
        if (level <= 9) return 45;
        if (level <= 12) return 60;
        if (level <= 15) return 80;
        if (level <= 18) return 100;
        if (level <= 20) return 120;
        if (level <= 25) return 150;
        if (level <= 30) return 200;
        return 250;
    }

    /// <summary>
    /// Начисляет баллы за пройденный уровень, если он новый
    /// (level > MaxLevelReached). Повторное прохождение не даёт баллов.
    /// Возвращает true, если баллы были начислены.
    /// </summary>
    public static bool AddPointsForLevel(int level)
    {
        if (level <= MaxLevelReached) return false;

        int points = GetPointsForLevel(level);
        TotalScore += points;
        MaxLevelReached = level;
        IsDirty = true;

        Debug.Log($"[Score] Уровень {level} впервые пройден: +{points}, всего {TotalScore}");
        return true;
    }

    /// <summary>
    /// Сбросить флаг «несинхронизировано» после успешной отправки.
    /// </summary>
    public static void MarkSynced()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Полный сброс счёта. Использовать только если правда надо
    /// обнулить всё (например, при смене аккаунта). ResetProgress этот
    /// метод НЕ вызывает.
    /// </summary>
    public static void ResetAll()
    {
        TotalScore = 0;
        MaxLevelReached = 0;
        IsDirty = false;
    }
}