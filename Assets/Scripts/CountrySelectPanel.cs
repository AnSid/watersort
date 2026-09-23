using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Панель выбора страны.
/// Компонент сам создаёт Canvas на своём GameObject.
/// Close() уничтожает сам контейнер, поэтому повторное открытие работает.
/// </summary>
public class CountrySelectPanel : MonoBehaviour
{
    private Transform contentRoot;
    private System.Action onSelected;

    public void Show(System.Action onSelected = null)
    {
        this.onSelected = onSelected;
        BuildUI();
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Затемнённый фон
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Заголовок
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "Выберите страну";
        title.font = UIHelper.Font;
        title.fontSize = 60;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -120);
        titleRt.sizeDelta = new Vector2(900, 100);

        // Фон списка
        GameObject listBg = new GameObject("ListBG");
        listBg.transform.SetParent(transform, false);
        Image listBgImg = listBg.AddComponent<Image>();
        listBgImg.color = new Color(1f, 1f, 1f, 0.95f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            listBgImg.sprite = UIHelper.RoundedButtonSprite;
            listBgImg.type = Image.Type.Sliced;
        }
        RectTransform listBgRt = listBg.GetComponent<RectTransform>();
        listBgRt.anchorMin = new Vector2(0.5f, 0.5f);
        listBgRt.anchorMax = new Vector2(0.5f, 0.5f);
        listBgRt.pivot = new Vector2(0.5f, 0.5f);
        listBgRt.anchoredPosition = new Vector2(0, 80f);
        listBgRt.sizeDelta = new Vector2(900, 1000);

        // ScrollRect
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(listBg.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(20, 20);
        scrollRt.offsetMax = new Vector2(-20, -20);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 6f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = vpRt;
        scroll.content = contentRt;
        contentRoot = content.transform;

        // Кнопки стран
        foreach (var country in CountryData.Countries)
        {
            CreateCountryButton(country.code, country.name);
        }

        // Кнопка «Отмена» — всегда
        CreateCancelButton();
    }

    void CreateCountryButton(string code, string name)
    {
        GameObject btnObj = new GameObject("Country_" + code);
        btnObj.transform.SetParent(contentRoot, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.95f, 0.95f, 0.97f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            img.sprite = UIHelper.RoundedButtonSprite;
            img.type = Image.Type.Sliced;
        }

        Button btn = btnObj.AddComponent<Button>();
        string capturedCode = code;
        btn.onClick.AddListener(() => OnCountrySelected(capturedCode));

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 110);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.minHeight = 110;
        le.preferredHeight = 110;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = CountryData.GetFlagWithName(code);
        t.font = UIHelper.Font;
        t.fontSize = 44;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = new Color(0.15f, 0.15f, 0.2f);
        t.raycastTarget = false;

        RectTransform textRt = t.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(30, 0);
        textRt.offsetMax = new Vector2(-30, 0);
    }

    void CreateCancelButton()
    {
        GameObject btnObj = new GameObject("CancelButton");
        btnObj.transform.SetParent(transform, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.6f, 0.6f, 0.65f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            img.sprite = UIHelper.RoundedButtonSprite;
            img.type = Image.Type.Sliced;
        }

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(Close);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 40);
        rt.sizeDelta = new Vector2(400, 100);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = "Отмена";
        t.font = UIHelper.Font;
        t.fontSize = 44;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform textRt = t.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    void OnCountrySelected(string code)
    {
        PlayerProfile.CountryCode = code;
        PlayerProfile.IsSetupDone = true;
        Debug.Log($"[Profile] Выбрана страна: {code} ({CountryData.GetName(code)})");
        onSelected?.Invoke();
        Close();
    }

    void Close()
    {
        Destroy(gameObject);
    }
}