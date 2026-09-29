using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Кэш шрифтов и общие хелперы для UI.
/// Фон-градиент живёт в мире (SpriteRenderer), а не в Canvas.
/// Кнопки создаются единым методом CreateButton — один стиль на весь проект.
/// </summary>
public static class UIHelper
{
    private static Font _font;
    private static Font _fontBold;

    private const string FONT_REGULAR_PATH = "Fonts/Rubik-Regular";
    private const string FONT_BOLD_PATH = "Fonts/Rubik-Bold";

    public static Font Font
    {
        get
        {
            if (_font == null)
            {
                _font = Resources.Load<Font>(FONT_REGULAR_PATH);
                if (_font == null)
                {
                    Debug.LogWarning($"[UIHelper] Не найден шрифт {FONT_REGULAR_PATH}, fallback на LegacyRuntime");
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }
            return _font;
        }
    }

    public static Font FontBold
    {
        get
        {
            if (_fontBold == null)
            {
                _fontBold = Resources.Load<Font>(FONT_BOLD_PATH);
                if (_fontBold == null)
                {
                    _fontBold = Font;
                }
            }
            return _fontBold;
        }
    }

    /// <summary>
    /// Спрайт скруглённой кнопки. Прокидывается из WaterSort при старте.
    /// </summary>
    public static Sprite RoundedButtonSprite;

    // ============================================================
    // ЕДИНЫЙ СТИЛЬ КНОПОК
    // ============================================================

    // Цвет тени под кнопками — общий для всего проекта.
    private static readonly Color ButtonShadowColor = new Color(0f, 0f, 0f, 0.15f);

    /// <summary>
    /// Создаёт кнопку единого стиля.
    /// parent      — куда положить
    /// label       — текст ("" — без текста)
    /// anchoredPos — позиция относительно anchorMin/Max
    /// size        — размер
    /// color       — цвет заливки
    /// onClick     — действие
    /// fontSize    — размер шрифта (по умолчанию 44)
    /// </summary>
    public static Button CreateButton(
        Transform parent,
        string label,
        Vector2 anchoredPos,
        Vector2 size,
        Color color,
        System.Action onClick,
        int fontSize = 44)
    {
        // ---- Тень: чуть больше кнопки, чуть ниже ----
        GameObject shadowObj = new GameObject("BtnShadow_" + label);
        shadowObj.transform.SetParent(parent, false);
        Image shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.color = ButtonShadowColor;
        if (RoundedButtonSprite != null)
        {
            shadowImg.sprite = RoundedButtonSprite;
            shadowImg.type = Image.Type.Sliced;
        }
        shadowImg.raycastTarget = false;

        RectTransform shRt = shadowObj.GetComponent<RectTransform>();
        shRt.anchorMin = new Vector2(0.5f, 0.5f);
        shRt.anchorMax = new Vector2(0.5f, 0.5f);
        shRt.pivot = new Vector2(0.5f, 0.5f);
        shRt.anchoredPosition = anchoredPos + new Vector2(0f, -4f); // тень на 4px ниже
        shRt.sizeDelta = size + new Vector2(4f, 4f);                // и на 4px больше

        // ---- Сама кнопка ----
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        if (RoundedButtonSprite != null)
        {
            img.sprite = RoundedButtonSprite;
            img.type = Image.Type.Sliced;
        }

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // ---- Текст ----
        if (!string.IsNullOrEmpty(label))
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text t = textObj.AddComponent<Text>();
            t.text = label;
            t.font = Font;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;

            RectTransform trt = t.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
        }

        return btn;
    }

    // ---- Фон-градиент ----
    private static Sprite _gradientSprite;
    private static SpriteRenderer _gradientRenderer;

    private static readonly Color BgTop = new Color(0.91f, 0.94f, 0.97f);
    private static readonly Color BgBottom = new Color(0.78f, 0.83f, 0.90f);

    public static SpriteRenderer CreateGradientBackground(Camera cam)
    {
        if (cam == null) return null;

        GameObject bgObj = new GameObject("GradientBG");
        _gradientRenderer = bgObj.AddComponent<SpriteRenderer>();
        _gradientRenderer.sprite = GetGradientSprite();
        _gradientRenderer.color = Color.white;
        _gradientRenderer.sortingOrder = -100;

        FitGradientToCamera(cam);
        return _gradientRenderer;
    }

    public static void FitGradientToCamera(Camera cam)
    {
        if (cam == null || _gradientRenderer == null || _gradientRenderer.sprite == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        float spriteW = _gradientRenderer.sprite.bounds.size.x;
        float spriteH = _gradientRenderer.sprite.bounds.size.y;

        float scaleX = worldWidth / spriteW;
        float scaleY = worldHeight / spriteH;

        _gradientRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        _gradientRenderer.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }

    static Sprite GetGradientSprite()
    {
        if (_gradientSprite != null) return _gradientSprite;

        int W = 1;
        int H = 256;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] px = new Color[W * H];
        for (int y = 0; y < H; y++)
        {
            float t = (float)y / (H - 1);
            Color c = Color.Lerp(BgBottom, BgTop, t);
            px[y * W] = c;
        }

        tex.SetPixels(px);
        tex.Apply();

        _gradientSprite = Sprite.Create(
            tex,
            new Rect(0, 0, W, H),
            new Vector2(0.5f, 0.5f),
            100f
        );
        return _gradientSprite;
    }
}