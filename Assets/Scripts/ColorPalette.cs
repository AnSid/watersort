using UnityEngine;

/// <summary>
/// Палитра контрастных цветов.
/// 28 базовых цветов, равномерно по HSL-кругу + чередование светлоты.
/// При выборке N цветов берём максимально далёкие друг от друга по hue И по L.
/// </summary>
public static class ColorPalette
{
    // Пары (hue, sat) — 28 точек по кругу (~12.8° шаг).
    // Светлота задаётся отдельно: чередуется LIGHT и DARK, чтобы
    // даже соседние по hue цвета различались яркостью.
    private static readonly (float h, float s, float l)[] All = new (float, float, float)[]
    {
        // 0–7: базовые насыщенные
        (  0f, 0.78f, 0.52f), // красный
        ( 15f, 0.80f, 0.60f), // красно-оранжевый (светлый)
        ( 28f, 0.82f, 0.55f), // оранжевый
        ( 42f, 0.85f, 0.62f), // жёлто-оранжевый (светлый)
        ( 56f, 0.85f, 0.52f), // жёлтый
        ( 74f, 0.70f, 0.58f), // салатовый (светлый)
        ( 92f, 0.65f, 0.45f), // зелёно-жёлтый (тёмный)
        (110f, 0.62f, 0.55f), // зелёный

        // 8–15: зелёно-синяя часть
        (128f, 0.65f, 0.45f), // тёмно-зелёный
        (146f, 0.68f, 0.55f), // изумрудный
        (163f, 0.68f, 0.45f), // морской (тёмный)
        (178f, 0.68f, 0.55f), // бирюзовый
        (193f, 0.72f, 0.48f), // циан (тёмный)
        (207f, 0.72f, 0.60f), // голубой (светлый)
        (221f, 0.68f, 0.50f), // небесный
        (236f, 0.65f, 0.58f), // синий (светлый)

        // 16–23: сине-фиолетово-розовая
        (250f, 0.62f, 0.48f), // индиго (тёмный)
        (263f, 0.58f, 0.58f), // сине-фиолетовый
        (277f, 0.58f, 0.50f), // фиолетовый (тёмный)
        (291f, 0.60f, 0.60f), // пурпурный (светлый)
        (305f, 0.62f, 0.52f), // малиновый
        (318f, 0.68f, 0.60f), // розово-красный (светлый)
        (332f, 0.70f, 0.50f), // тёмно-розовый
        (346f, 0.75f, 0.58f), // алый (светлый)

        // 24–27: коричневая группа
        ( 20f, 0.45f, 0.35f), // тёмно-коричневый
        ( 30f, 0.50f, 0.42f), // коричневый
        ( 40f, 0.40f, 0.32f), // оливково-коричневый
        ( 10f, 0.55f, 0.42f), // кирпичный
    };

    public static int Count => All.Length;

    /// <summary>
    /// N максимально контрастных цветов.
    /// Равномерно по палитре (floor) + сортировка по светлоте-зигзагу,
    /// чтобы чередовать светлые и тёмные и не сливаться.
    /// </summary>
    public static Color[] GetContrastingColors(int n)
    {
        if (n <= 0) return new Color[0];
        if (n >= All.Length)
        {
            Color[] all = new Color[All.Length];
            for (int i = 0; i < All.Length; i++)
                all[i] = FromHsl(All[i].h, All[i].s, All[i].l);
            return all;
        }

        // 1. Равномерно выбираем индексы по кругу.
        int[] indices = new int[n];
        for (int i = 0; i < n; i++)
            indices[i] = Mathf.FloorToInt((float)i * All.Length / n);

        // 2. Чередуем светлоту: сортируем выбранные по L, потом берём
        //    через один с начала и с конца — светлый, тёмный, светлый, ...
        System.Array.Sort(indices, (a, b) => All[a].l.CompareTo(All[b].l));

        Color[] result = new Color[n];
        int lo = 0, hi = n - 1;
        for (int i = 0; i < n; i++)
        {
            int idx;
            if ((i & 1) == 0) idx = indices[hi--];   // светлый
            else idx = indices[lo++];   // тёмный
            result[i] = FromHsl(All[idx].h, All[idx].s, All[idx].l);
        }

        return result;
    }

    private static Color FromHsl(float h, float s, float l)
    {
        float c = (1f - Mathf.Abs(2f * l - 1f)) * s;
        float x = c * (1f - Mathf.Abs((h / 60f) % 2f - 1f));
        float m = l - c / 2f;

        float r = 0f, g = 0f, b = 0f;
        if (h < 60f) { r = c; g = x; b = 0f; }
        else if (h < 120f) { r = x; g = c; b = 0f; }
        else if (h < 180f) { r = 0f; g = c; b = x; }
        else if (h < 240f) { r = 0f; g = x; b = c; }
        else if (h < 300f) { r = x; g = 0f; b = c; }
        else { r = c; g = 0f; b = x; }

        return new Color(r + m, g + m, b + m, 1f);
    }
}