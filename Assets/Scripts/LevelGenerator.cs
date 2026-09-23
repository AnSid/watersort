using System.Collections.Generic;
using System;

/// <summary>
/// Генератор уровней Water Sort.
/// Маленькие уровни (≤4 цветов) — без BFS, случайная раздача.
/// Большие — BFS с малыми лимитами (не фризит).
/// Потокобезопасен: не использует UnityEngine API.
/// </summary>
public static class LevelGenerator
{
    public const int LAYERS_PER_TUBE = 4;

    public static List<List<int>> Generate(int colorCount, int tubeCount, int shuffleSteps)
    {
        // Маленькие уровни — без BFS
        if (colorCount <= 4)
            return BuildRandom(colorCount, tubeCount, new Random());

        // Лимиты BFS в зависимости от размера
        int maxVisited = colorCount <= 6 ? 3000 : 8000;
        int minSolution = colorCount <= 6 ? 5 : 8;
        int attempts = colorCount <= 6 ? 3 : 5;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            var candidate = BuildRandom(colorCount, tubeCount, new Random());
            if (!IsInterestingStart(candidate)) continue;

            int len = BFSSolutionLength(candidate, colorCount, maxVisited);
            if (len < 0) continue;
            if (len < minSolution) continue;

            return candidate;
        }

        // Fallback — случайная раздача без проверки
        return BuildRandom(colorCount, tubeCount, new Random());
    }

    private static List<List<int>> BuildRandom(int colorCount, int tubeCount, Random rng)
    {
        List<int> bag = new List<int>();
        for (int c = 0; c < colorCount; c++)
            for (int i = 0; i < LAYERS_PER_TUBE; i++)
                bag.Add(c);

        for (int i = 0; i < bag.Count; i++)
        {
            int r = rng.Next(i, bag.Count);
            int tmp = bag[i]; bag[i] = bag[r]; bag[r] = tmp;
        }

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
        return mixed >= (int)Math.Ceiling(nonEmpty * 0.5);
    }

    private static int BFSSolutionLength(List<List<int>> start, int colorCount, int maxVisited)
    {
        string startKey = Serialize(start);
        if (IsWin(start, colorCount)) return 0;

        var visited = new HashSet<string>();
        visited.Add(startKey);

        var queue = new Queue<Tuple<List<List<int>>, int>>();
        queue.Enqueue(Tuple.Create(Clone(start), 0));

        int tubeCount = start.Count;

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            var state = item.Item1;
            int depth = item.Item2;
            if (depth >= 80) continue;

            for (int from = 0; from < tubeCount; from++)
            {
                if (state[from].Count == 0) continue;

                int topColor = state[from][state[from].Count - 1];
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
                    if (state[to].Count == 0 && IsTubeSolved(state[from])) continue;

                    int freeSpace = LAYERS_PER_TUBE - state[to].Count;
                    int moveCount = Math.Min(sameCount, freeSpace);
                    if (moveCount <= 0) continue;

                    var next = Clone(state);
                    for (int k = 0; k < moveCount; k++)
                    {
                        next[from].RemoveAt(next[from].Count - 1);
                        next[to].Add(topColor);
                    }

                    if (IsWin(next, colorCount)) return depth + 1;

                    string key = Serialize(next);
                    if (visited.Contains(key)) continue;
                    visited.Add(key);

                    if (visited.Count > maxVisited) return -1;

                    queue.Enqueue(Tuple.Create(next, depth + 1));
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

    public static void GetLevelParams(int level, out int colorCount, out int tubeCount, out int shuffleSteps)
    {
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

        int emptyTubes = (level < 9) ? 2 : 1;
        tubeCount = colorCount + emptyTubes;
        shuffleSteps = 0;
    }
}