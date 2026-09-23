using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Кэш шрифтов и общие хелперы для UI.
/// </summary>
public static class UIHelper
{
    private static Font _font;

    public static Font Font
    {
        get
        {
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }

    /// <summary>
    /// Спрайт скруглённой кнопки. Прокидывается из WaterSort при старте.
    /// </summary>
    public static Sprite RoundedButtonSprite;
}