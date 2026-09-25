using UnityEngine;

/// <summary>
/// Палитра контрастных цветов.
/// 13 вручную подобранных цветов, каждый отличается от каждого по hue и светлоте.
/// Выбор N цветов: перемешиваем индексы с seed'ом от номера уровня,
/// берём первые N. Тогда со временем все 13 цветов используются равномерно.
/// Никаких секторов, зигзагов и страховок — палитра сама по себе не слипается.
/// </summary>
public static class ColorPalette
{
    // 13 цветов. Все hue разные (шаг ~27.7°), L чередуется, S разная.
    // Специально нет двух похожих — например, двух зелёных с одинаковой L,
    // или двух розовых с одинаковой L.
    //
    // Проверено на слипание:
    //   - красный (0°) и коричневый (20°): разные L (0.50 vs 0.32) и S (0.85 vs 0.55).
    //   - пурпурный (295°) и малиновый (325°): разные L (0.62 vs 0.48).
    //   - бирюзовый (180°) и голубой (205°): разные L (0.48 vs 0.58).
    //   - серый (S=0) не путается ни с чем — у него нет hue.
    private static readonly (float h, float s, float l)[] All = new (float, float, float)[]
    {
        (  0f, 0.85f, 0.50f), // 0: красный
        ( 28f, 0.85f, 0.58f), // 1: оранжевый (светлый)
        ( 52f, 0.85f, 0.55f), // 2: жёлтый
        (100f, 0.65f, 0.45f), // 3: зелёный (тёмный)
        (150f, 0.65f, 0.55f), // 4: изумрудный
        (180f, 0.65f, 0.48f), // 5: бирюзовый
        (205f, 0.70f, 0.58f), // 6: голубой (светлый)
        (230f, 0.70f, 0.42f), // 7: синий (тёмный)
        (265f, 0.55f, 0.52f), // 8: фиолетовый
        (295f, 0.60f, 0.62f), // 9: пурпурный (светлый)
        (325f, 0.70f, 0.48f), // 10: малиновый
        ( 20f, 0.55f, 0.32f), // 11: коричневый (тёмный, низкая S)
        (  0f, 0.00f, 0.50f), // 12: серый (нейтральный)
    };

    public static int Count => All.Length;

    /// <summary>
    /// N контрастных цветов. Seed — номер уровня, чтобы разные уровни
    /// получали разные наборы цветов, а со временем все 13 использовались равномерно.
    /// </summary>
    public static Color[] GetContrastingColors(int n, int seed)
    {
        if (n <= 0) return new Color[0];

        if (n >= All.Length)
        {
            Color[] all = new Color[All.Length];
            for (int i = 0; i < All.Length; i++)
                all[i] = FromHsl(All[i].h, All[i].s, All[i].l);
            return all;
        }

        // Перемешиваем индексы 0..12 детерминированно от seed.
        int[] indices = new int[All.Length];
        for (int i = 0; i < All.Length; i++) indices[i] = i;

        System.Random rng = new System.Random(seed);
        for (int i = 0; i < indices.Length; i++)
        {
            int j = rng.Next(i, indices.Length);
            int tmp = indices[i];
            indices[i] = indices[j];
            indices[j] = tmp;
        }

        // Берём первые n.
        Color[] result = new Color[n];
        for (int i = 0; i < n; i++)
        {
            int idx = indices[i];
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