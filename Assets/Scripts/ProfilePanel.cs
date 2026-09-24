using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    private InputField nameInput;
    private Image countryFlagImage;
    private Text countryText;

    public void Show()
    {
        BuildUI();
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 130;
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

        MakeText(panel.transform, "Профиль", new Vector2(0, 660), 70, FontStyle.Bold,
            new Color(0.15f, 0.15f, 0.2f), new Vector2(800, 100), TextAnchor.MiddleCenter);

        // Подпись — pivot (0, 0.5), x = 30 (отступ от левого края панели).
        MakeText(panel.transform, "Имя игрока:", new Vector2(30, 520), 40, FontStyle.Normal,
            new Color(0.3f, 0.3f, 0.35f), new Vector2(500, 60), TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f));

        GameObject inputObj = new GameObject("NameInput");
        inputObj.transform.SetParent(panel.transform, false);
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.95f, 0.95f, 0.97f);
        RectTransform inRt = inputObj.GetComponent<RectTransform>();
        inRt.anchorMin = new Vector2(0.5f, 0.5f);
        inRt.anchorMax = new Vector2(0.5f, 0.5f);
        inRt.pivot = new Vector2(0.5f, 0.5f);
        inRt.anchoredPosition = new Vector2(0, 420f);
        inRt.sizeDelta = new Vector2(700, 90);

        nameInput = inputObj.AddComponent<InputField>();

        GameObject textComp = new GameObject("Text");
        textComp.transform.SetParent(inputObj.transform, false);
        Text inputText = textComp.AddComponent<Text>();
        inputText.font = UIHelper.Font;
        inputText.fontSize = 44;
        inputText.color = new Color(0.1f, 0.1f, 0.15f);
        inputText.alignment = TextAnchor.MiddleLeft;
        inputText.supportRichText = false;
        RectTransform itRt = inputText.rectTransform;
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.offsetMin = new Vector2(20, 0);
        itRt.offsetMax = new Vector2(-20, 0);

        nameInput.textComponent = inputText;
        nameInput.text = PlayerProfile.PlayerName;

        GameObject countryBtn = new GameObject("CountryButton");
        countryBtn.transform.SetParent(panel.transform, false);
        Image cImg = countryBtn.AddComponent<Image>();
        cImg.color = new Color(0.3f, 0.55f, 0.85f);
        if (UIHelper.RoundedButtonSprite != null)
        {
            cImg.sprite = UIHelper.RoundedButtonSprite;
            cImg.type = Image.Type.Sliced;
        }
        Button cBtn = countryBtn.AddComponent<Button>();
        cBtn.targetGraphic = cImg;

        RectTransform cRt = countryBtn.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.pivot = new Vector2(0.5f, 0.5f);
        cRt.anchoredPosition = new Vector2(0, 290f);
        cRt.sizeDelta = new Vector2(700, 100);

        GameObject flagObj = new GameObject("Flag");
        flagObj.transform.SetParent(countryBtn.transform, false);
        countryFlagImage = flagObj.AddComponent<Image>();
        countryFlagImage.preserveAspect = true;
        countryFlagImage.raycastTarget = false;
        RectTransform frt = countryFlagImage.rectTransform;
        frt.anchorMin = new Vector2(0f, 0.5f);
        frt.anchorMax = new Vector2(0f, 0.5f);
        frt.pivot = new Vector2(0f, 0.5f);
        frt.anchoredPosition = new Vector2(30f, 0f);
        frt.sizeDelta = new Vector2(70, 50);

        GameObject ctObj = new GameObject("Text");
        ctObj.transform.SetParent(countryBtn.transform, false);
        countryText = ctObj.AddComponent<Text>();
        countryText.font = UIHelper.Font;
        countryText.fontSize = 44;
        countryText.alignment = TextAnchor.MiddleLeft;
        countryText.color = Color.white;
        countryText.raycastTarget = false;
        RectTransform ctRt = countryText.rectTransform;
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.offsetMin = new Vector2(120, 0);
        ctRt.offsetMax = new Vector2(-20, 0);

        RefreshCountryDisplay();

        cBtn.onClick.AddListener(() =>
        {
            Transform parent = transform.parent != null ? transform.parent : transform;
            Transform old = parent.Find("CountrySelectPanel");
            if (old != null) Destroy(old.gameObject);

            GameObject cspObj = new GameObject("CountrySelectPanel");
            cspObj.transform.SetParent(parent, false);
            CountrySelectPanel csp = cspObj.AddComponent<CountrySelectPanel>();
            csp.Show(() =>
            {
                RefreshCountryDisplay();
            });

            Canvas c = cspObj.GetComponentInChildren<Canvas>(true);
            if (c != null) c.sortingOrder = 150;
        });

        MakeText(panel.transform, "Награды", new Vector2(0, 160), 50, FontStyle.Bold,
            new Color(0.15f, 0.15f, 0.2f), new Vector2(700, 70), TextAnchor.MiddleCenter);

        MakeText(panel.transform, "Пока нет наград", new Vector2(0, 80), 40, FontStyle.Normal,
            new Color(0.5f, 0.5f, 0.55f), new Vector2(700, 70), TextAnchor.MiddleCenter);

        CreateButton(panel.transform, "Сохранить", new Vector2(-180, -500),
            new Vector2(320, 100), new Color(0.3f, 0.75f, 0.4f), () =>
            {
                if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
                    PlayerProfile.PlayerName = nameInput.text.Trim();

                StartCoroutine(PushProfileToLeaderboard());
                Close();
            });

        CreateButton(panel.transform, "Закрыть", new Vector2(180, -500),
            new Vector2(320, 100), new Color(0.6f, 0.6f, 0.65f), Close);
    }

    void RefreshCountryDisplay()
    {
        string code = PlayerProfile.CountryCode;

        Sprite flag = CountryData.GetFlagSprite(code);
        if (flag != null)
        {
            countryFlagImage.sprite = flag;
            countryFlagImage.enabled = true;
        }
        else
        {
            countryFlagImage.enabled = false;
        }

        if (countryText != null)
            countryText.text = CountryData.GetName(code);
    }

    IEnumerator PushProfileToLeaderboard()
    {
        string deviceId = PlayerProfile.DeviceId;
        string playerName = PlayerProfile.PlayerName;
        string countryCode = PlayerProfile.CountryCode;

        int level = PlayerPrefs.GetInt("WaterSort_Level", 1);
        int score = level * 100;

        yield return LeaderboardAPI.SubmitScore(deviceId, playerName, score, countryCode);

        List<LeaderboardRecord> fresh = null;
        yield return LeaderboardAPI.GetTopScores(50, result => { fresh = result; });
        if (fresh != null && fresh.Count > 0)
            LeaderboardCache.Save(fresh);
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

    Text MakeText(Transform parent, string content, Vector2 pos, int size, FontStyle style, Color color, Vector2 sizeDelta, TextAnchor align, Vector2? pivot = null)
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
        Vector2 pv = pivot ?? new Vector2(0.5f, 0.5f);
        rt.anchorMin = pv;
        rt.anchorMax = pv;
        rt.pivot = pv;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return t;
    }

    void Close()
    {
        Destroy(gameObject);
    }
}