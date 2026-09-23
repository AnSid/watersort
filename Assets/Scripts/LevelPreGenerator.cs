/// <summary>
/// Заглушка. Пре-генерация уровней через Task отключена —
/// она конфликтовала с корутиной ожидания (гонка за pendingColorCount/pendingTubeCount).
/// Сейчас WaterSort генерирует уровень синхронно в StartNewGameInternal().
/// </summary>
public static class LevelPreGenerator
{
    // Ничего не делает. Методы оставлены пустыми, если где-то остались вызовы.
    public static void Request(int colorCount, int tubeCount) { }
    public static bool IsReady => false;
    public static bool Matches(int colorCount, int tubeCount) => false;
    public static System.Collections.Generic.List<System.Collections.Generic.List<int>> Take() => null;
    public static void Reset() { }
}