using System.Collections.Generic;
using System;

public static class LevelGenerator
{
    public const int LAYERS_PER_TUBE = 4;

    public static List<List<int>> Generate(int colorCount, int tubeCount, int shuffleSteps)
    {
        int emptyTubes = tubeCount - colorCount;
        if (emptyTubes < 1) emptyTubes = 1;
        if (emptyTubes > 2) emptyTubes = 2;

        int baseSeed = Environment.TickCount ^ (colorCount * 73856093) ^ (tubeCount * 19349663);

        int K = colorCount * 5 + shuffleSteps / 2;
        if (K < 24) K = 24;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Random rng = new Random(baseSeed + attempt * 7919);
            var candidate = BuildReverseShuffle(colorCount, emptyTubes, K, rng);
            if (candidate == null) continue;

            if (CountSolvedTubes(candidate) > 0) continue;
            if (CountMonoTubes(candidate) > 1) continue;
            if (MixedRatio(candidate) < 0.5f) continue;

            return candidate;
        }

        // Fallback с перетасовкой до 20 раз
        for (int attempt = 0; attempt < 20; attempt++)
        {
            var fb = BuildFallbackMixed(colorCount, emptyTubes, baseSeed + attempt * 104729);
            if (CountSolvedTubes(fb) == 0 && CountMonoTubes(fb) <= 1)
                return fb;
        }

        // Совсем крайний случай
        return BuildFallbackMixed(colorCount, emptyTubes, baseSeed + 999999);
    }

    private static List<List<int>> BuildReverseShuffle(int colorCount, int emptyTubes, int K, Random rng)
    {
        var tubes = new List<List<int>>(colorCount + emptyTubes);
        for (int c = 0; c < colorCount; c++)
        {
            var t = new List<int>(LAYERS_PER_TUBE);
            for (int i = 0; i < LAYERS_PER_TUBE; i++) t.Add(c);
            tubes.Add(t);
        }
        for (int i = 0; i < emptyTubes; i++)
            tubes.Add(new List<int>());

        int totalTubes = tubes.Count;

        // Стек последних ходов: храним до totalTubes * 2 пар (from, to).
        // Этого достаточно, чтобы разорвать любой короткий цикл на 3-6 цветах.
        int historySize = totalTubes * 2;
        var histFrom = new int[historySize];
        var histTo = new int[historySize];
        int histCount = 0;

        for (int step = 0; step < K; step++)
        {
            var moves = new List<(int from, int to)>();

            for (int from = 0; from < totalTubes; from++)
            {
                if (tubes[from].Count == 0) continue;

                int topColor = tubes[from][tubes[from].Count - 1];
                int sameCount = 0;
                for (int i = tubes[from].Count - 1; i >= 0; i--)
                {
                    if (tubes[from][i] == topColor) sameCount++;
                    else break;
                }
                if (sameCount <= 0) continue;

                for (int to = 0; to < totalTubes; to++)
                {
                    if (to == from) continue;
                    if (tubes[to].Count >= LAYERS_PER_TUBE) continue;
                    if (tubes[to].Count > 0 && tubes[to][tubes[to].Count - 1] != topColor) continue;

                    // Запрет повтора ЛЮБОГО из последних historySize ходов
                    bool repeats = false;
                    for (int h = 0; h < histCount; h++)
                    {
                        if (histFrom[h] == from && histTo[h] == to) { repeats = true; break; }
                    }
                    if (repeats) continue;

                    bool sourceMono = (sameCount == tubes[from].Count);
                    bool targetEmpty = (tubes[to].Count == 0);
                    if (sourceMono && targetEmpty) continue;

                    moves.Add((from, to));
                }
            }

            if (moves.Count == 0) break;

            var pick = moves[rng.Next(moves.Count)];

            int color = tubes[pick.from][tubes[pick.from].Count - 1];
            tubes[pick.from].RemoveAt(tubes[pick.from].Count - 1);
            tubes[pick.to].Add(color);

            // Пихаем ход в стек; если полон — сдвигаем
            if (histCount < historySize)
            {
                histFrom[histCount] = pick.from;
                histTo[histCount] = pick.to;
                histCount++;
            }
            else
            {
                for (int h = 1; h < historySize; h++)
                {
                    histFrom[h - 1] = histFrom[h];
                    histTo[h - 1] = histTo[h];
                }
                histFrom[historySize - 1] = pick.from;
                histTo[historySize - 1] = pick.to;
            }
        }

        return tubes;
    }

    private static List<List<int>> BuildFallbackMixed(int colorCount, int emptyTubes, int seed)
    {
        var bag = new List<int>(colorCount * LAYERS_PER_TUBE);
        for (int c = 0; c < colorCount; c++)
            for (int i = 0; i < LAYERS_PER_TUBE; i++) bag.Add(c);

        var rng = new Random(seed);
        for (int i = 0; i < bag.Count; i++)
        {
            int j = rng.Next(i, bag.Count);
            int tmp = bag[i]; bag[i] = bag[j]; bag[j] = tmp;
        }

        var tubes = new List<List<int>>(colorCount + emptyTubes);
        int idx = 0;
        for (int i = 0; i < colorCount; i++)
        {
            var t = new List<int>(LAYERS_PER_TUBE);
            for (int k = 0; k < LAYERS_PER_TUBE; k++) t.Add(bag[idx++]);
            tubes.Add(t);
        }
        for (int i = 0; i < emptyTubes; i++)
            tubes.Add(new List<int>());

        return tubes;
    }

    private static int CountSolvedTubes(List<List<int>> tubes)
    {
        int n = 0;
        foreach (var t in tubes)
        {
            if (t.Count != LAYERS_PER_TUBE) continue;
            bool mono = true;
            for (int i = 1; i < t.Count; i++)
                if (t[i] != t[0]) { mono = false; break; }
            if (mono) n++;
        }
        return n;
    }

    private static int CountMonoTubes(List<List<int>> tubes)
    {
        int n = 0;
        foreach (var t in tubes)
        {
            if (t.Count == 0) continue;
            bool mono = true;
            for (int i = 1; i < t.Count; i++)
                if (t[i] != t[0]) { mono = false; break; }
            if (mono) n++;
        }
        return n;
    }

    private static float MixedRatio(List<List<int>> tubes)
    {
        int nonEmpty = 0;
        int mixed = 0;
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
        if (nonEmpty == 0) return 0f;
        return (float)mixed / nonEmpty;
    }

    public static void GetLevelParams(int level, out int colorCount, out int tubeCount, out int shuffleSteps)
    {
        int colors;
        int empty;

        if (level <= 2) { colors = 3; empty = 2; }
        else if (level <= 4) { colors = 4; empty = 2; }
        else if (level <= 6) { colors = 5; empty = 2; }
        else if (level <= 9) { colors = 6; empty = 2; }
        else if (level <= 12) { colors = 7; empty = 2; }
        else if (level <= 15) { colors = 8; empty = 2; }
        else if (level <= 18) { colors = 9; empty = 2; }
        else if (level <= 20) { colors = 10; empty = 2; }
        else if (level <= 25) { colors = 11; empty = 2; }
        else { colors = 12; empty = 2; }

        colorCount = colors;
        tubeCount = colors + empty;
        shuffleSteps = level * 2;
    }
}