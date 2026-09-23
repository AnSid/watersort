using UnityEngine;
using UnityEngine.UI;
using System;

public class Toolbar : MonoBehaviour
{
    public Action OnSettingsClicked;
    public Action OnLeaderboardClicked;
    public Action OnUndoClicked;
    public Action OnHintClicked;
    public Action OnRefreshClicked;

    private Text levelText;
    private Text hintText;
    private Font uiFont;

    private const float BAR_HEIGHT = 120f;
    private const float BTN_SIZE = 90f;
    private const float BTN_GAP = 6f;

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

        // Уровень — по центру
        levelText = MakeText(topBar.transform, "LevelText", "Уровень 1",
            TextAnchor.MiddleCenter, Vector2.zero, 55, Color.white,
            new Vector2(400, BAR_HEIGHT));

        // Кнопки — справа
        GameObject btnRoot = new GameObject("Buttons");
        btnRoot.transform.SetParent(topBar.transform, false);
        RectTransform btnRt = btnRoot.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0.5f);
        btnRt.anchorMax = new Vector2(1f, 0.5f);
        btnRt.pivot = new Vector2(1f, 0.5f);
        btnRt.anchoredPosition = new Vector2(-15f, 0f);
        btnRt.sizeDelta = new Vector2(5 * (BTN_SIZE + BTN_GAP), BTN_SIZE);

        HorizontalLayoutGroup layout = btnRoot.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = BTN_GAP;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateButton(btnRoot.transform, "settings", () => OnSettingsClicked?.Invoke());
        CreateButton(btnRoot.transform, "leaderboard", () => OnLeaderboardClicked?.Invoke());
        CreateButton(btnRoot.transform, "undo", () => OnUndoClicked?.Invoke());
        CreateButton(btnRoot.transform, "hint", () => OnHintClicked?.Invoke());
        CreateButton(btnRoot.transform, "refresh", () => OnRefreshClicked?.Invoke());

        hintText = MakeText(canvasTransform, "Hint", "Выбери колбочку",
            TextAnchor.UpperCenter, new Vector2(0, -160), 45,
            new Color(0.3f, 0.3f, 0.35f), new Vector2(900, 200));
    }

    void CreateButton(Transform parent, string iconName, Action onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + iconName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(BTN_SIZE, BTN_SIZE);

        Image hit = btnObj.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0.001f);
        hit.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = hit;
        btn.onClick.AddListener(() => onClick?.Invoke());

        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform, false);

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

    public void SetLevel(int level)
    {
        if (levelText != null) levelText.text = "Уровень " + level;
    }

    public void SetHint(string text)
    {
        if (hintText != null) hintText.text = text;
    }

    public void SetLoading(bool isLoading)
    {
        if (hintText != null)
            hintText.text = isLoading ? "Загрузка…" : "Выбери колбочку";
    }
}