using UnityEngine;
using UnityEngine.UI;
using System;

public class Toolbar : MonoBehaviour
{
    public Action OnSettingsClicked;
    public Action OnLeaderboardClicked;
    public Action OnAddTubeClicked;
    public Action OnUndoClicked;
    public Action OnRefreshClicked;

    private Text levelText;
    private Text topText;
    private Text hintText;
    private Font uiFont;

    private Button undoButton;
    private Button addTubeButton;
    private Button flagButton;
    private Image undoImage;
    private Image addTubeImage;
    private Image flagImage;

    private const float BAR_HEIGHT = 120f;
    private const float BTN_SIZE = 90f;
    private const float BTN_GAP = 6f;
    private const float NUM_W = 90f;
    private const float NUM_GAP = 10f;
    private const float EDGE_PAD = 20f;

    private static readonly Color BtnActive = Color.white;
    private static readonly Color BtnDisabled = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color FlagIdle = new Color(1f, 1f, 1f, 0.75f);

    public void Build(Transform canvasTransform)
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvasTransform, false);
        Image topImg = topBar.AddComponent<Image>();
        topImg.color = new Color(0.3f, 0.55f, 0.85f);
        topImg.raycastTarget = false;

        RectTransform topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0f, 1f);
        topRt.anchorMax = new Vector2(1f, 1f);
        topRt.pivot = new Vector2(0.5f, 1f);
        topRt.anchoredPosition = Vector2.zero;
        topRt.sizeDelta = new Vector2(0f, BAR_HEIGHT);

        // ============================================================
        // ЛЕВАЯ ГРУППА: [leaderboard] [N] gap [flag] [M]
        // ============================================================
        float x = EDGE_PAD;

        CreateButtonAt(topBar.transform, "leaderboard",
            new Vector2(x + BTN_SIZE / 2f, 0f),
            () => OnLeaderboardClicked?.Invoke());
        x += BTN_SIZE + BTN_GAP;

        topText = CreateNumberBlock(topBar.transform, x, "—",
            () => OnLeaderboardClicked?.Invoke());
        x += NUM_W + NUM_GAP;

        flagButton = CreateButtonAt(topBar.transform, "flag",
            new Vector2(x + BTN_SIZE / 2f, 0f),
            () => { });
        flagButton.interactable = false;
        if (flagImage != null) flagImage.color = FlagIdle;
        x += BTN_SIZE + BTN_GAP;

        levelText = CreateNumberBlock(topBar.transform, x, "1", null);

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

        hintText = MakeText(canvasTransform, "Hint", "Выбери колбочку",
            TextAnchor.UpperCenter, new Vector2(0, -160), 45,
            new Color(0.3f, 0.3f, 0.35f), new Vector2(900, 200));
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
            icon.color = Color.white;
            icon.preserveAspect = true;
        }
        else
        {
            icon.color = new Color(1f, 1f, 1f, 0.5f);
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
        if (iconName == "flag") flagImage = icon;
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
        t.color = Color.white;
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

    // Цвет цифры уровня по диапазону сложности.
    // Логика совпадает с LevelGenerator.GetLevelParams:
    //   чем выше уровень — тем «горячее» цвет.
    Color GetLevelColor(int level)
    {
        if (level <= 4) return new Color(0.35f, 0.85f, 0.40f); // зелёный
        if (level <= 6) return new Color(0.65f, 0.85f, 0.35f); // жёлто-зелёный
        if (level <= 9) return new Color(0.95f, 0.85f, 0.30f); // жёлтый
        if (level <= 12) return new Color(0.95f, 0.65f, 0.25f); // оранжевый
        if (level <= 15) return new Color(0.95f, 0.45f, 0.25f); // оранжево-красный
        if (level <= 18) return new Color(0.95f, 0.30f, 0.30f); // красный
        if (level <= 20) return new Color(0.90f, 0.30f, 0.55f); // красно-розовый
        if (level <= 25) return new Color(0.75f, 0.40f, 0.85f); // фиолетовый
        return new Color(0.60f, 0.45f, 0.95f);                  // тёмно-фиолетовый
    }

    private int _level = 1;
    private int _top = -1;

    public void SetLevel(int level)
    {
        _level = level;
        UpdateLevelText();
    }

    public void SetTop(int topPosition)
    {
        _top = topPosition;
        UpdateLevelText();
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = _level.ToString();
            levelText.color = GetLevelColor(_level);
        }

        if (topText != null)
            topText.text = _top > 0 ? _top.ToString() : "—";
    }

    public void SetHint(string text)
    {
        if (hintText != null) hintText.text = text;
    }

    public void SetUndoEnabled(bool enabled)
    {
        if (undoButton != null) undoButton.interactable = enabled;
        if (undoImage != null) undoImage.color = enabled ? BtnActive : BtnDisabled;
    }

    public void SetAddTubeEnabled(bool enabled)
    {
        if (addTubeButton != null) addTubeButton.interactable = enabled;
        if (addTubeImage != null) addTubeImage.color = enabled ? BtnActive : BtnDisabled;
    }

    public void SetLoading(bool isLoading)
    {
        if (hintText != null)
            hintText.text = isLoading ? "Загрузка…" : "Выбери колбочку";
    }
}