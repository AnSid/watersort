using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Панель настроек: анимация, звуки, музыка, профиль, сброс прогресса.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    private GameObject panelObj;
    private Action onProfileRequested;
    private Action onProgressReset;

    public void Show(Action onProfileRequested, Action onProgressReset)
    {
        this.onProfileRequested = onProfileRequested;
        this.onProgressReset = onProgressReset;
        BuildUI();
    }

    void BuildUI()
    {
        panelObj = new GameObject("SettingsPanel");
        panelObj.transform.SetParent(transform, false);

        Canvas canvas = panelObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        CanvasScaler scaler = panelObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        panelObj.AddComponent<GraphicRaycaster>();

        // Затемнение
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(panelObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Белая панель
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(panelObj.transform, false);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(1f, 1f, 1f, 0.98f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            panelImg.sprite = UIHelper.RoundedButtonSprite;
            panelImg.type = Image.Type.Sliced;
        }
        RectTransform pRt = panel.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(860f, 1100f);

        // Заголовок
        MakeText(panel.transform, "Настройки",
            new Vector2(0, 470), 70, FontStyle.Bold, new Color(0.15f, 0.15f, 0.2f),
            new Vector2(800, 100), TextAnchor.MiddleCenter);

        // Переключатели
        float y = 320f;
        CreateToggleRow(panel.transform, "Анимация перелива", GameSettings.AnimationEnabled, y,
            v => GameSettings.AnimationEnabled = v);
        y -= 140f;
        CreateToggleRow(panel.transform, "Звуки", GameSettings.SoundEnabled, y,
            v => GameSettings.SoundEnabled = v);
        y -= 140f;
        CreateToggleRow(panel.transform, "Музыка", GameSettings.MusicEnabled, y,
            v => GameSettings.MusicEnabled = v);

        // Кнопка Профиль
        y -= 180f;
        CreateButton(panel.transform, "Профиль", new Vector2(0, y),
            new Vector2(500, 100), new Color(0.3f, 0.55f, 0.85f), () =>
            {
                onProfileRequested?.Invoke();
                Close();
            });

        // Кнопка Сбросить прогресс
        y -= 130f;
        CreateButton(panel.transform, "Сбросить прогресс", new Vector2(0, y),
            new Vector2(500, 100), new Color(0.85f, 0.35f, 0.3f), () =>
            {
                onProgressReset?.Invoke();
                Close();
            });

        // Кнопка Закрыть
        CreateButton(panel.transform, "Закрыть", new Vector2(0, -470f),
            new Vector2(400, 100), new Color(0.6f, 0.6f, 0.65f), Close);
    }

    void CreateToggleRow(Transform parent, string label, bool initial, float y, Action<bool> onChange)
    {
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(parent, false);
        Text t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = UIHelper.Font;
        t.fontSize = 50;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = new Color(0.15f, 0.15f, 0.2f);
        t.raycastTarget = false;
        RectTransform trt = t.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(-360f, y);
        trt.sizeDelta = new Vector2(500, 80);

        GameObject btnObj = new GameObject("Toggle");
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = initial ? new Color(0.3f, 0.75f, 0.4f) : new Color(0.7f, 0.7f, 0.72f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            img.sprite = UIHelper.RoundedButtonSprite;
            img.type = Image.Type.Sliced;
        }
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        RectTransform brt = btnObj.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f);
        brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(300f, y);
        brt.sizeDelta = new Vector2(140, 70);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text bt = btnTextObj.AddComponent<Text>();
        bt.font = UIHelper.Font;
        bt.fontSize = 36;
        bt.fontStyle = FontStyle.Bold;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.color = Color.white;
        bt.raycastTarget = false;
        bt.text = initial ? "ВКЛ" : "ВЫКЛ";
        RectTransform btrt = bt.rectTransform;
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.offsetMin = Vector2.zero;
        btrt.offsetMax = Vector2.zero;

        bool state = initial;
        btn.onClick.AddListener(() =>
        {
            state = !state;
            img.color = state ? new Color(0.3f, 0.75f, 0.4f) : new Color(0.7f, 0.7f, 0.72f);
            bt.text = state ? "ВКЛ" : "ВЫКЛ";
            onChange?.Invoke(state);
        });
    }

    void CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, Action onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + label);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        if (UIHelper.RoundedButtonSprite != null)
        {
            img.sprite = UIHelper.RoundedButtonSprite;
            img.type = Image.Type.Sliced;
        }
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = label;
        t.font = UIHelper.Font;
        t.fontSize = 44;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform trt = t.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    Text MakeText(Transform parent, string content, Vector2 pos, int size, FontStyle style, Color color, Vector2 sizeDelta, TextAnchor align)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = UIHelper.Font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    void Close()
    {
        if (panelObj != null) Destroy(panelObj);
        panelObj = null;
    }
}