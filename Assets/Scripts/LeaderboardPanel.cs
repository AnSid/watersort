using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Панель рейтинга.
/// Компонент сам создаёт Canvas на своём GameObject.
/// Close() уничтожает сам контейнер, поэтому повторное открытие работает.
/// </summary>
public class LeaderboardPanel : MonoBehaviour
{
    private Transform contentRoot;
    private Text statusText;
    private System.Action onProfileRequested;

    public void Show() { Show(null); }

    public void Show(System.Action onProfileRequested)
    {
        this.onProfileRequested = onProfileRequested;
        BuildUI();
        StartCoroutine(LoadScores());
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
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
        pRt.sizeDelta = new Vector2(900f, 1500f);

        MakeCenteredText(panel.transform, "Рейтинг", new Vector2(0, 660), 70, FontStyle.Bold,
            new Color(0.15f, 0.15f, 0.2f), new Vector2(800, 100));

        GameObject listBg = new GameObject("ListBG");
        listBg.transform.SetParent(panel.transform, false);
        Image listBgImg = listBg.AddComponent<Image>();
        listBgImg.color = new Color(0.96f, 0.96f, 0.98f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            listBgImg.sprite = UIHelper.RoundedButtonSprite;
            listBgImg.type = Image.Type.Sliced;
        }
        RectTransform listBgRt = listBg.GetComponent<RectTransform>();
        listBgRt.anchorMin = new Vector2(0.5f, 0.5f);
        listBgRt.anchorMax = new Vector2(0.5f, 0.5f);
        listBgRt.pivot = new Vector2(0.5f, 0.5f);
        listBgRt.anchoredPosition = new Vector2(0, 30f);
        listBgRt.sizeDelta = new Vector2(820, 1180);

        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(listBg.transform, false);
        RectTransform scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(15, 15);
        scrollRt.offsetMax = new Vector2(-15, -15);

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

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

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 4f;
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

        statusText = MakeCenteredText(panel.transform, "", new Vector2(0, 590), 34, FontStyle.Italic,
            new Color(0.5f, 0.5f, 0.55f), new Vector2(800, 60));

        // Три кнопки внизу: Мой профиль / Обновить / Закрыть
        CreateButton(panel.transform, "Мой профиль", new Vector2(-280, -660),
            new Vector2(280, 100), new Color(0.85f, 0.65f, 0.25f), () =>
            {
                onProfileRequested?.Invoke();
            });

        CreateButton(panel.transform, "Обновить", new Vector2(0, -660),
            new Vector2(280, 100), new Color(0.3f, 0.55f, 0.85f), () =>
            {
                StartCoroutine(LoadScores());
            });

        CreateButton(panel.transform, "Закрыть", new Vector2(280, -660),
            new Vector2(280, 100), new Color(0.6f, 0.6f, 0.65f), Close);
    }

    IEnumerator LoadScores()
    {
        List<LeaderboardRecord> cached = LeaderboardCache.Load();
        if (cached != null && cached.Count > 0)
        {
            ShowRecords(cached);
            if (statusText != null) statusText.text = "Обновление…";
        }
        else
        {
            ClearList();
            if (statusText != null) statusText.text = "Загрузка…";
        }

        List<LeaderboardRecord> fresh = null;
        yield return LeaderboardAPI.GetTopScores(50, result => { fresh = result; });

        if (fresh != null && fresh.Count > 0)
        {
            ShowRecords(fresh);
            LeaderboardCache.Save(fresh);
            if (statusText != null) statusText.text = "";
        }
        else
        {
            if (cached != null && cached.Count > 0)
            {
                if (statusText != null) statusText.text = "Нет соединения. Показан кэш";
            }
            else
            {
                if (statusText != null) statusText.text = "Нет соединения. Нет сохранённых данных";
            }
        }
    }

    void ClearList()
    {
        if (contentRoot == null) return;
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);
    }

    void ShowRecords(List<LeaderboardRecord> records)
    {
        ClearList();
        if (records == null) return;
        for (int i = 0; i < records.Count; i++)
            CreateRow(i + 1, records[i]);
    }

    void CreateRow(int place, LeaderboardRecord rec)
    {
        GameObject row = new GameObject("Row_" + place);
        row.transform.SetParent(contentRoot, false);

        Image rowImg = row.AddComponent<Image>();
        rowImg.color = (place % 2 == 1)
            ? new Color(1f, 1f, 1f, 0.6f)
            : new Color(0.92f, 0.92f, 0.95f, 0.6f);

        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 90);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = 90;
        le.preferredHeight = 90;

        MakeRowText(row.transform, place.ToString(), new Vector2(20, 0), 38, FontStyle.Bold,
            new Color(0.2f, 0.2f, 0.3f), new Vector2(60, 90));

        // Флаг (PNG-спрайт), fallback — текстовый код
        Sprite flag = CountryData.GetFlagSprite(rec.country_code);
        if (flag != null)
        {
            GameObject flagObj = new GameObject("Flag");
            flagObj.transform.SetParent(row.transform, false);
            Image flagImg = flagObj.AddComponent<Image>();
            flagImg.sprite = flag;
            flagImg.preserveAspect = true;
            flagImg.raycastTarget = false;

            RectTransform frt = flagImg.rectTransform;
            frt.anchorMin = new Vector2(0f, 0.5f);
            frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = new Vector2(100f, 0f);
            frt.sizeDelta = new Vector2(56, 40);
        }
        else
        {
            string code = string.IsNullOrEmpty(rec.country_code) ? "—" : rec.country_code.ToUpper();
            MakeRowText(row.transform, code, new Vector2(100, 0), 38, FontStyle.Bold,
                new Color(0.3f, 0.55f, 0.85f), new Vector2(70, 90));
        }

        MakeRowText(row.transform, rec.player_name ?? "—", new Vector2(200, 0), 38, FontStyle.Normal,
            new Color(0.15f, 0.15f, 0.2f), new Vector2(440, 90));

        MakeRowTextRight(row.transform, rec.score.ToString(), new Vector2(-20, 0), 38, FontStyle.Bold,
            new Color(0.3f, 0.55f, 0.85f), new Vector2(160, 90));
    }

    Text MakeCenteredText(Transform parent, string content, Vector2 anchoredPos, int size, FontStyle style,
        Color color, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = UIHelper.Font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.raycastTarget = false;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    Text MakeRowText(Transform parent, string content, Vector2 anchoredPos, int size, FontStyle style,
        Color color, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = UIHelper.Font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = color;
        t.raycastTarget = false;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    Text MakeRowTextRight(Transform parent, string content, Vector2 anchoredPos, int size, FontStyle style,
        Color color, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = content;
        t.font = UIHelper.Font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleRight;
        t.color = color;
        t.raycastTarget = false;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    void CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, System.Action onClick)
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
        t.fontSize = 40;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform trt = t.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    void Close()
    {
        Destroy(gameObject);
    }
}