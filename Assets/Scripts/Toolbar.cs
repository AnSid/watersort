using System;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    public Action OnSettingsClicked;
    public Action OnLeaderboardClicked;
    public Action OnAddTubeClicked;
    public Action OnUndoClicked;
    public Action OnRefreshClicked;

    private Text topText;
    private Text hintText;
    private Font uiFont;

    private Button undoButton;
    private Button addTubeButton;
    private Image undoImage;
    private Image addTubeImage;

    private const float BAR_HEIGHT = 120f;
    private const float BTN_SIZE = 90f;
    private const float BTN_GAP = 6f;
    private const float NUM_W = 90f;
    private const float NUM_GAP = 10f;
    private const float EDGE_PAD = 20f;

    // Цвета иконок и цифр на белом тулбаре.
    private static readonly Color IconActive = new Color(0.25f, 0.30f, 0.40f, 1f);
    private static readonly Color IconDisabled = new Color(0.25f, 0.30f, 0.40f, 0.30f);
    private static readonly Color TopTextColor = new Color(0.25f, 0.30f, 0.40f, 1f);

    public void Build(Transform canvasTransform)
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ---- Тулбар: белая полоса ----
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvasTransform, false);
        Image topImg = topBar.AddComponent<Image>();
        topImg.color = new Color(1f, 1f, 1f, 0.95f);
        topImg.raycastTarget = false;

        RectTransform topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0f, 1f);
        topRt.anchorMax = new Vector2(1f, 1f);
        topRt.pivot = new Vector2(0.5f, 1f);
        topRt.anchoredPosition = Vector2.zero;
        topRt.sizeDelta = new Vector2(0f, BAR_HEIGHT);

        // ---- Тень под тулбаром: тонкая полоска ----
        GameObject shadow = new GameObject("TopBarShadow");
        shadow.transform.SetParent(topBar.transform, false);
        Image shadowImg = shadow.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.06f);
        shadowImg.raycastTarget = false;
        RectTransform shRt = shadow.GetComponent<RectTransform>();
        shRt.anchorMin = new Vector2(0f, 0f);
        shRt.anchorMax = new Vector2(1f, 0f);
        shRt.pivot = new Vector2(0.5f, 1f);
        shRt.anchoredPosition = Vector2.zero;
        shRt.sizeDelta = new Vector2(0f, 6f);

        // ============================================================
        // ЛЕВАЯ ГРУППА: [leaderboard] [N позиция]
        // ============================================================
        float x = EDGE_PAD;

        CreateButtonAt(topBar.transform, "leaderboard",
            new Vector2(x + BTN_SIZE / 2f, 0f),
            () => OnLeaderboardClicked?.Invoke());
        x += BTN_SIZE + BTN_GAP;

        topText = CreateNumberBlock(topBar.transform, x, "—",
            () => OnLeaderboardClicked?.Invoke());

        // ============================================================
        // ПРАВАЯ ГРУППА: [addtube] [undo] [refresh] [settings]
        // ============================================================
        float rightBase = -EDGE_PAD - BTN_SIZE / 2f;
        float step = BTN_SIZE + BTN_GAP;

        CreateButtonAtRight(topBar.transform, "settings", rightBase,
            () => OnSettingsClicked?.Invoke());
        CreateButtonAtRight(topBar.transform, "refresh", rightBase - step,
            () => OnRefreshClicked?.Invoke());
        undoButton = CreateButtonAtRight(topBar.transform, "undo", rightBase - step * 2f,
            () => OnUndoClicked?.Invoke());
        addTubeButton = CreateButtonAtRight(topBar.transform, "addtube", rightBase - step * 3f,
            () => OnAddTubeClicked?.Invoke());

        // Хинт — ниже, потому что над ним «Уровень N».
        hintText = MakeText(canvasTransform, "Hint", "Выбери колбочку",
            TextAnchor.UpperCenter, new Vector2(0, -300), 36,
            new Color(0.45f, 0.45f, 0.5f), new Vector2(900, 120));
    }

    Button CreateButtonAt(Transform parent, string iconName, Vector2 centerPos, Action onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + iconName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = centerPos;
        rt.sizeDelta = new Vector2(BTN_SIZE, BTN_SIZE);

        Image hit = btnObj.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0.001f);
        hit.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = hit;
        btn.onClick.AddListener(() => onClick?.Invoke());

        BuildIcon(btnObj.transform, iconName);
        return btn;
    }

    Button CreateButtonAtRight(Transform parent, string iconName, float rightOffset, Action onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + iconName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(rightOffset, 0f);
        rt.sizeDelta = new Vector2(BTN_SIZE, BTN_SIZE);

        Image hit = btnObj.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0.001f);
        hit.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = hit;
        btn.onClick.AddListener(() => onClick?.Invoke());

        BuildIcon(btnObj.transform, iconName);
        return btn;
    }

    void BuildIcon(Transform parent, string iconName)
    {
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(parent, false);

        Image icon = iconObj.AddComponent<Image>();
        Sprite sp = Resources.Load<Sprite>("Icons/icon_" + iconName);
        if (sp != null)
        {
            icon.sprite = sp;
            icon.color = IconActive;
            icon.preserveAspect = true;
        }
        else
        {
            icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Debug.LogWarning($"[Toolbar] Не найден спрайт Icons/icon_{iconName}");
        }
        icon.raycastTarget = false;

        RectTransform itRt = icon.rectTransform;
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.offsetMin = new Vector2(10, 10);
        itRt.offsetMax = new Vector2(-10, -10);

        if (iconName == "undo") undoImage = icon;
        if (iconName == "addtube") addTubeImage = icon;
    }

    Text CreateNumberBlock(Transform parent, float leftX, string initial, Action onClick)
    {
        GameObject block = new GameObject("NumBlock");
        block.transform.SetParent(parent, false);

        RectTransform bRt = block.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0f, 0.5f);
        bRt.anchorMax = new Vector2(0f, 0.5f);
        bRt.pivot = new Vector2(0f, 0.5f);
        bRt.anchoredPosition = new Vector2(leftX, 0f);
        bRt.sizeDelta = new Vector2(NUM_W, BTN_SIZE);

        if (onClick != null)
        {
            Image hit = block.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0.001f);
            hit.raycastTarget = true;
            Button btn = block.AddComponent<Button>();
            btn.targetGraphic = hit;
            btn.onClick.AddListener(() => onClick?.Invoke());
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(block.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = initial;
        t.font = uiFont;
        t.fontSize = 46;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = TopTextColor;
        t.raycastTarget = false;

        RectTransform ttRt = t.rectTransform;
        ttRt.anchorMin = Vector2.zero;
        ttRt.anchorMax = Vector2.one;
        ttRt.offsetMin = Vector2.zero;
        ttRt.offsetMax = Vector2.zero;

        return t;
    }

    Text MakeText(Transform parent, string name, string content,
        TextAnchor anchor, Vector2 pos, int size, Color color, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = uiFont;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;

        RectTransform rect = t.rectTransform;
        Vector2 anchorPoint;
        if (anchor == TextAnchor.MiddleLeft) anchorPoint = new Vector2(0f, 0.5f);
        else if (anchor == TextAnchor.UpperLeft) anchorPoint = new Vector2(0f, 1f);
        else if (anchor == TextAnchor.UpperCenter) anchorPoint = new Vector2(0.5f, 1f);
        else if (anchor == TextAnchor.UpperRight) anchorPoint = new Vector2(1f, 1f);
        else anchorPoint = new Vector2(0.5f, 0.5f);

        rect.anchorMin = anchorPoint;
        rect.anchorMax = anchorPoint;
        rect.pivot = anchorPoint;
        rect.anchoredPosition = pos;
        rect.sizeDelta = sizeDelta;
        return t;
    }

    // Цвет цифры уровня — плавный градиент в одной гамме.
    public static Color GetLevelColor(int level)
    {
        float t = Mathf.Clamp01((level - 1) / 29f);
        Color from = new Color(0.45f, 0.70f, 0.55f);
        Color to = new Color(0.40f, 0.55f, 0.75f);
        return Color.Lerp(from, to, t);
    }

    // Цвет цифры позиции в рейтинге. На белом фоне — тёмные цвета.
    Color GetTopColor(int top)
    {
        if (top <= 0) return TopTextColor;
        if (top == 1) return new Color(0.85f, 0.65f, 0.20f); // золотой
        if (top == 2) return new Color(0.60f, 0.60f, 0.65f); // серебряный
        if (top == 3) return new Color(0.70f, 0.45f, 0.25f); // бронзовый
        if (top <= 10) return new Color(0.30f, 0.65f, 0.35f); // зелёный
        if (top <= 50) return new Color(0.75f, 0.65f, 0.30f); // тёмно-жёлтый
        return TopTextColor;
    }

    private int _level = 1;
    private int _top = -1;

    public void SetLevel(int level)
    {
        _level = level;
    }

    public void SetTop(int topPosition)
    {
        _top = topPosition;
        UpdateTopText();
    }

    void UpdateTopText()
    {
        if (topText != null)
        {
            topText.text = _top > 0 ? _top.ToString() : "—";
            topText.color = GetTopColor(_top);
        }
    }

    public void SetHint(string text)
    {
        if (hintText != null) hintText.text = text;
    }

    public void SetUndoEnabled(bool enabled)
    {
        if (undoButton != null) undoButton.interactable = enabled;
        if (undoImage != null) undoImage.color = enabled ? IconActive : IconDisabled;
    }

    public void SetAddTubeEnabled(bool enabled)
    {
        if (addTubeButton != null) addTubeButton.interactable = enabled;
        if (addTubeImage != null) addTubeImage.color = enabled ? IconActive : IconDisabled;
    }

    public void SetLoading(bool isLoading)
    {
        if (hintText != null)
            hintText.text = isLoading ? "Загрузка…" : "Выбери колбочку";
    }
}