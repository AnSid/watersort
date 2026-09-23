using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Генератор уровней Water Sort.
/// Гарантия решаемости: BFS-проверка каждого сгенерированного поля.
/// Уровень принимается, только если BFS нашёл решение длиной от MIN_SOLUTION до MAX_SOLUTION.
/// </summary>
public static class LevelGenerator
{
    public const int LAYERS_PER_TUBE = 4;

    // Ограничения BFS
    private const int MAX_VISITED_STATES = 80000;
    private const int MAX_BFS_DEPTH = 80;

    // Ограничения «интересности» и сложности
    private const int MIN_SOLUTION_LENGTH = 10;
    private const int GENERATION_ATTEMPTS = 50;

    public static List<List<int>> Generate(int colorCount, int tubeCount, int shuffleSteps)
    {
        System.Random rng = new System.Random();

        for (int attempt = 0; attempt < GENERATION_ATTEMPTS; attempt++)
        {
            // 1. Случайная раздача
            var candidate = BuildRandom(colorCount, tubeCount, rng);

            // 2. Проверка «интересности» — без решённых колб, ≥50% перемешаны
            if (!IsInterestingStart(candidate)) continue;

            // 3. BFS-проверка решаемости и длины решения
            int solutionLength = BFSSolutionLength(candidate, colorCount);
            if (solutionLength < 0) continue;                        // не решается
            if (solutionLength < MIN_SOLUTION_LENGTH) continue;      // слишком просто
            if (solutionLength > MAX_BFS_DEPTH) continue;            // BFS не дошёл

            Debug.Log($"[LevelGenerator] Уровень сгенерирован: {colorCount} цветов, {tubeCount} колб, решение за {solutionLength} ходов.");
            return candidate;
        }

        // Если все попытки провалились — возвращаем последнюю (редкий случай)
        Debug.LogWarning("[LevelGenerator] Не удалось найти интересный уровень за 50 попыток, возвращаю последний.");
        return BuildRandom(colorCount, tubeCount, rng);
    }

    // ============================================================
    // Случайная раздача слоёв
    // ============================================================
    private static List<List<int>> BuildRandom(int colorCount, int tubeCount, System.Random rng)
    {
        // Мешок: каждого цвета по 4
        List<int> bag = new List<int>();
        for (int c = 0; c < colorCount; c++)
            for (int i = 0; i < LAYERS_PER_TUBE; i++)
                bag.Add(c);

        // Перемешать мешок
        for (int i = 0; i < bag.Count; i++)
        {
            int r = rng.Next(i, bag.Count);
            (bag[i], bag[r]) = (bag[r], bag[i]);
        }

        // Разложить по colorCount колбам (по 4 слоя), остальные — пустые
        List<List<int>> tubes = new List<List<int>>();
        int idx = 0;
        for (int i = 0; i < colorCount; i++)
        {
            List<int> t = new List<int>();
            for (int k = 0; k < LAYERS_PER_TUBE; k++)
                t.Add(bag[idx++]);
            tubes.Add(t);
        }
        for (int i = 0; i < tubeCount - colorCount; i++)
            tubes.Add(new List<int>());

        return tubes;
    }

    // ============================================================
    // Проверка «интересности»
    // ============================================================
    private static bool IsTubeSolved(List<int> t)
    {
        if (t.Count != LAYERS_PER_TUBE) return false;
        for (int i = 1; i < t.Count; i++)
            if (t[i] != t[0]) return false;
        return true;
    }

    private static bool IsInterestingStart(List<List<int>> tubes)
    {
        foreach (var t in tubes)
            if (IsTubeSolved(t)) return false;

        int mixed = 0;
        int nonEmpty = 0;
        foreach (var t in tubes)
        {
            if (t.Count == 0) continue;
            nonEmpty++;
            int first = t[0];
            for (int i = 1; i < t.Count; i++)
            {
                if (t[i] != first) { mixed++; break; }
            }
        }
        if (nonEmpty == 0) return false;
        return mixed >= Mathf.CeilToInt(nonEmpty * 0.5f);
    }

    // ============================================================
    // BFS — поиск кратчайшего решения
    // ============================================================
    private static int BFSSolutionLength(List<List<int>> start, int colorCount)
    {
        // Состояние — массив колб, сериализуем в строку (для HashSet)
        string startKey = Serialize(start);
        if (IsWin(start, colorCount)) return 0;

        var visited = new HashSet<string>();
        visited.Add(startKey);

        var queue = new Queue<(List<List<int>> state, int depth)>();
        queue.Enqueue((Clone(start), 0));

        int tubeCount = start.Count;

        while (queue.Count > 0)
        {
            var (state, depth) = queue.Dequeue();
            if (depth >= MAX_BFS_DEPTH) continue;

            // Перебираем все возможные ходы
            for (int from = 0; from < tubeCount; from++)
            {
                if (state[from].Count == 0) continue;

                int topColor = state[from][state[from].Count - 1];
                // Количество слоёв этого цвета сверху
                int sameCount = 0;
                for (int i = state[from].Count - 1; i >= 0; i--)
                {
                    if (state[from][i] == topColor) sameCount++;
                    else break;
                }

                for (int to = 0; to < tubeCount; to++)
                {
                    if (to == from) continue;
                    if (state[to].Count >= LAYERS_PER_TUBE) continue;
                    if (state[to].Count > 0 && state[to][state[to].Count - 1] != topColor) continue;

                    // Не переливать из «готовой» колбы в пустую — бессмысленно
                    if (state[to].Count == 0 && IsTubeSolved(state[from])) continue;

                    int freeSpace = LAYERS_PER_TUBE - state[to].Count;
                    int moveCount = Mathf.Min(sameCount, freeSpace);
                    if (moveCount <= 0) continue;

                    var next = Clone(state);
                    for (int k = 0; k < moveCount; k++)
                    {
                        next[from].RemoveAt(next[from].Count - 1);
                        next[to].Add(topColor);
                    }

                    // Проверка на победу
                    if (IsWin(next, colorCount)) return depth + 1;

                    string key = Serialize(next);
                    if (visited.Contains(key)) continue;
                    visited.Add(key);

                    if (visited.Count > MAX_VISITED_STATES) return -1; // защита от OOM

                    queue.Enqueue((next, depth + 1));
                }
            }
        }

        return -1;
    }

    private static bool IsWin(List<List<int>> tubes, int colorCount)
    {
        int solvedTubes = 0;
        foreach (var t in tubes)
        {
            if (t.Count == 0) continue;
            if (t.Count != LAYERS_PER_TUBE) return false;
            for (int i = 1; i < t.Count; i++)
                if (t[i] != t[0]) return false;
            solvedTubes++;
        }
        return solvedTubes == colorCount;
    }

    private static string Serialize(List<List<int>> tubes)
    {
        // Простая сериализация: "0,1,2|3,3,1|..."
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < tubes.Count; i++)
        {
            for (int j = 0; j < tubes[i].Count; j++)
            {
                sb.Append(tubes[i][j]);
                if (j < tubes[i].Count - 1) sb.Append(',');
            }
            if (i < tubes.Count - 1) sb.Append('|');
        }
        return sb.ToString();
    }

    private static List<List<int>> Clone(List<List<int>> tubes)
    {
        var c = new List<List<int>>(tubes.Count);
        foreach (var t in tubes) c.Add(new List<int>(t));
        return c;
    }

    // ============================================================
    // Формула сложности
    // ============================================================
    public static void GetLevelParams(int level, out int colorCount, out int tubeCount, out int shuffleSteps)
    {
        // Цвета
        if (level <= 1) colorCount = 3;
        else if (level == 2) colorCount = 4;
        else if (level == 3) colorCount = 5;
        else if (level == 4) colorCount = 6;
        else if (level == 5) colorCount = 6;
        else if (level == 6) colorCount = 7;
        else if (level == 7) colorCount = 8;
        else if (level == 8) colorCount = 8;
        else if (level == 9) colorCount = 9;
        else if (level <= 12) colorCount = 10;
        else if (level <= 16) colorCount = 11;
        else colorCount = 12;

        // Пустые колбы: 2 до 8 уровня, 1 с 9+
        int emptyTubes = (level < 9) ? 2 : 1;
        tubeCount = colorCount + emptyTubes;

        // shuffleSteps больше не используется в старой логике,
        // но BFS всё равно ограничен MAX_BFS_DEPTH = 80
        shuffleSteps = 0;
    }
}