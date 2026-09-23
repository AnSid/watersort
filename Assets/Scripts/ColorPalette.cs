using UnityEngine;

/// <summary>
/// Палитра контрастных цветов.
/// 20 базовых цветов равномерно по HSL-кругу.
/// При выборке N цветов берём максимально далёкие друг от друга.
/// </summary>
public static class ColorPalette
{
    private static readonly Color[] All = new Color[]
    {
        FromHsl(  0f, 0.75f, 0.55f), // красный
        FromHsl( 20f, 0.80f, 0.55f), // оранжевый
        FromHsl( 40f, 0.85f, 0.55f), // жёлто-оранжевый
        FromHsl( 58f, 0.85f, 0.55f), // жёлтый
        FromHsl( 80f, 0.65f, 0.50f), // салатовый
        FromHsl(100f, 0.60f, 0.45f), // зелёно-жёлтый
        FromHsl(120f, 0.60f, 0.45f), // зелёный
        FromHsl(145f, 0.65f, 0.45f), // изумрудный
        FromHsl(165f, 0.65f, 0.45f), // морской
        FromHsl(180f, 0.65f, 0.50f), // бирюзовый
        FromHsl(195f, 0.70f, 0.55f), // голубой
        FromHsl(210f, 0.70f, 0.55f), // небесный
        FromHsl(225f, 0.65f, 0.55f), // синий
        FromHsl(245f, 0.60f, 0.55f), // индиго
        FromHsl(265f, 0.55f, 0.55f), // сине-фиолетовый
        FromHsl(285f, 0.55f, 0.55f), // фиолетовый
        FromHsl(305f, 0.60f, 0.55f), // пурпурный
        FromHsl(325f, 0.65f, 0.55f), // малиновый
        FromHsl(340f, 0.65f, 0.50f), // розово-красный
        FromHsl( 15f, 0.30f, 0.45f), // коричневый
    };

    public static int Count => All.Length;

    /// <summary>
    /// N максимально контрастных цветов. Равномерно по палитре.
    /// </summary>
    public static Color[] GetContrastingColors(int n)
    {
        if (n <= 0) return new Color[0];
        if (n >= All.Length) return (Color[])All.Clone();

        Color[] result = new Color[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            int idx = Mathf.RoundToInt(t * All.Length) % All.Length;
            result[i] = All[idx];
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