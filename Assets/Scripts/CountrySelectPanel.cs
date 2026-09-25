using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CountrySelectPanel : MonoBehaviour
{
    private Transform contentRoot;
    private InputField searchInput;
    private System.Action onSelected;
    private List<(string code, string name)> allCountries;

    public void Show(System.Action onSelected = null)
    {
        this.onSelected = onSelected;
        allCountries = new List<(string, string)>(CountryData.Countries);
        BuildUI();
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 140;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        // Затемнение — 0.85, чтобы профиль чуть просвечивал, но не мешал.
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.85f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Белое окошко. Растянуто по вертикали: от 60px снизу до 220px сверху.
        // По ширине — фиксированные 900px по центру.
        // Заголовок, поиск, скролл и кнопка — ВСЕ внутри этого окошка.
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(1f, 1f, 1f, 1f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            panelImg.sprite = UIHelper.RoundedButtonSprite;
            panelImg.type = Image.Type.Sliced;
        }
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 1f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0, 0);
        panelRt.offsetMin = new Vector2(-450, 60);
        panelRt.offsetMax = new Vector2(450, -220);

        // Заголовок — внутри панели, у её верхнего края.
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "Выберите страну";
        title.font = UIHelper.Font;
        title.fontSize = 60;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.15f, 0.15f, 0.2f);
        title.raycastTarget = false;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -20);
        titleRt.sizeDelta = new Vector2(900, 100);

        // --- Поле поиска (внутри панели, под заголовком) ---
        GameObject searchObj = new GameObject("SearchInput");
        searchObj.transform.SetParent(panelObj.transform, false);
        Image searchBg = searchObj.AddComponent<Image>();
        searchBg.color = new Color(0.93f, 0.93f, 0.95f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            searchBg.sprite = UIHelper.RoundedButtonSprite;
            searchBg.type = Image.Type.Sliced;
        }
        RectTransform searchRt = searchObj.GetComponent<RectTransform>();
        searchRt.anchorMin = new Vector2(0f, 1f);
        searchRt.anchorMax = new Vector2(1f, 1f);
        searchRt.pivot = new Vector2(0.5f, 1f);
        searchRt.anchoredPosition = new Vector2(0, -130f);
        searchRt.sizeDelta = new Vector2(-40f, 90f);

        searchInput = searchObj.AddComponent<InputField>();

        GameObject searchTextObj = new GameObject("Text");
        searchTextObj.transform.SetParent(searchObj.transform, false);
        Text searchText = searchTextObj.AddComponent<Text>();
        searchText.font = UIHelper.Font;
        searchText.fontSize = 40;
        searchText.color = new Color(0.1f, 0.1f, 0.15f);
        searchText.alignment = TextAnchor.MiddleLeft;
        searchText.supportRichText = false;
        RectTransform stRt = searchText.rectTransform;
        stRt.anchorMin = Vector2.zero;
        stRt.anchorMax = Vector2.one;
        stRt.offsetMin = new Vector2(20, 0);
        stRt.offsetMax = new Vector2(-20, 0);

        GameObject searchPlaceholderObj = new GameObject("Placeholder");
        searchPlaceholderObj.transform.SetParent(searchObj.transform, false);
        Text searchPlaceholder = searchPlaceholderObj.AddComponent<Text>();
        searchPlaceholder.text = "Поиск страны…";
        searchPlaceholder.font = UIHelper.Font;
        searchPlaceholder.fontSize = 40;
        searchPlaceholder.fontStyle = FontStyle.Italic;
        searchPlaceholder.color = new Color(0.55f, 0.55f, 0.6f);
        searchPlaceholder.alignment = TextAnchor.MiddleLeft;
        searchPlaceholder.supportRichText = false;
        RectTransform spRt = searchPlaceholder.rectTransform;
        spRt.anchorMin = Vector2.zero;
        spRt.anchorMax = Vector2.one;
        spRt.offsetMin = new Vector2(20, 0);
        spRt.offsetMax = new Vector2(-20, 0);

        searchInput.textComponent = searchText;
        searchInput.placeholder = searchPlaceholder;
        searchInput.onValueChanged.AddListener(OnSearchChanged);

        // --- ScrollRect (внутри панели, между поиском и кнопкой) ---
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(panelObj.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(20, 160);   // низ: место под кнопку
        scrollRt.offsetMax = new Vector2(-20, -240); // верх: под заголовком и поиском

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = new Vector2(-25, 0);
        viewport.AddComponent<RectMask2D>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 6f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll.viewport = vpRt;
        scroll.content = contentRt;
        contentRoot = content.transform;

        // --- Scrollbar справа ---
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollObj.transform, false);
        Image sbBg = scrollbarObj.AddComponent<Image>();
        sbBg.color = new Color(0.85f, 0.85f, 0.88f);
        RectTransform sbRt = scrollbarObj.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(1f, 0f);
        sbRt.anchorMax = new Vector2(1f, 1f);
        sbRt.pivot = new Vector2(1f, 0.5f);
        sbRt.anchoredPosition = Vector2.zero;
        sbRt.sizeDelta = new Vector2(20, 0);

        Scrollbar sb = scrollbarObj.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;

        GameObject sbArea = new GameObject("SlidingArea");
        sbArea.transform.SetParent(scrollbarObj.transform, false);
        RectTransform sbAreaRt = sbArea.AddComponent<RectTransform>();
        sbAreaRt.anchorMin = Vector2.zero;
        sbAreaRt.anchorMax = Vector2.one;
        sbAreaRt.offsetMin = Vector2.zero;
        sbAreaRt.offsetMax = Vector2.zero;

        GameObject sbHandle = new GameObject("Handle");
        sbHandle.transform.SetParent(sbArea.transform, false);
        Image sbHandleImg = sbHandle.AddComponent<Image>();
        sbHandleImg.color = new Color(0.5f, 0.55f, 0.65f);
        RectTransform sbHandleRt = sbHandle.GetComponent<RectTransform>();
        sbHandleRt.anchorMin = Vector2.zero;
        sbHandleRt.anchorMax = Vector2.one;
        sbHandleRt.offsetMin = Vector2.zero;
        sbHandleRt.offsetMax = Vector2.zero;

        sb.handleRect = sbHandleRt;
        sb.targetGraphic = sbHandleImg;
        scroll.verticalScrollbar = sb;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scroll.verticalScrollbarSpacing = 4f;

        // Строим список
        RebuildList(null);

        // Кнопка Отмена — внутри панели, у её нижнего края.
        CreateCancelButton(panelObj.transform);
    }

    void OnSearchChanged(string query)
    {
        RebuildList(query);
    }

    void RebuildList(string query)
    {
        if (contentRoot == null) return;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        IEnumerable<(string code, string name)> filtered = allCountries;

        if (!string.IsNullOrEmpty(query))
        {
            string q = query.Trim().ToLowerInvariant();
            filtered = allCountries.Where(c =>
                c.name.ToLowerInvariant().Contains(q) ||
                c.code.ToLowerInvariant().Contains(q));
        }

        foreach (var country in filtered)
        {
            CreateCountryButton(country.code, country.name);
        }

        Canvas.ForceUpdateCanvases();
        ScrollRect sr = contentRoot.GetComponentInParent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;
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

        Sprite flag = CountryData.GetFlagSprite(code);
        if (flag != null)
        {
            GameObject flagObj = new GameObject("Flag");
            flagObj.transform.SetParent(btnObj.transform, false);
            Image flagImg = flagObj.AddComponent<Image>();
            flagImg.sprite = flag;
            flagImg.preserveAspect = true;
            flagImg.raycastTarget = false;

            RectTransform frt = flagImg.rectTransform;
            frt.anchorMin = new Vector2(0f, 0.5f);
            frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = new Vector2(30f, 0f);
            frt.sizeDelta = new Vector2(72, 52);
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text t = textObj.AddComponent<Text>();
        t.text = name;
        t.font = UIHelper.Font;
        t.fontSize = 44;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = new Color(0.15f, 0.15f, 0.2f);
        t.raycastTarget = false;

        RectTransform textRt = t.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(120, 0);
        textRt.offsetMax = new Vector2(-30, 0);
    }

    void CreateCancelButton(Transform parent)
    {
        GameObject btnObj = new GameObject("CancelButton");
        btnObj.transform.SetParent(parent, false);

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
        onSelected?.Invoke();
        Close();
    }

    void Close()
    {
        Destroy(gameObject);
    }
}