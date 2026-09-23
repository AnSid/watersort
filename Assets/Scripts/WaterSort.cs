using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WaterSort : MonoBehaviour
{
    private const int LAYERS_PER_TUBE = 4;
    private const int MAX_PER_ROW = 7;
    private const int MAX_TUBES = 14;

    private const float TUBE_SPACING = 0.9f;
    private const float ROW_SPACING = 3.0f;

    private int currentLevel = 1;

    private int TUBE_COUNT = 5;
    private int COLOR_COUNT = 3;
    private List<List<int>> tubes = new List<List<int>>();
    private List<TubeVisual> tubeVisuals = new List<TubeVisual>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<Vector3> tubePositions = new List<Vector3>();
    private int selectedTube = -1;
    private bool gameWon = false;
    private bool isAnimating = false;

    // Клик во время анимации: скипаем анимацию и обрабатываем клик,
    // когда isAnimating снова станет false.
    private bool pendingClick = false;

    private Color[] colors;

    // Стек ходов для Undo
    private struct MoveRecord { public int from; public int to; public int count; }
    private Stack<MoveRecord> moveHistory = new Stack<MoveRecord>();

    private Text winText;
    private Image winPanel;
    private GameObject nextButtonObj;
    private Camera cam;
    private CountrySelectPanel countryPanel;
    private Toolbar toolbar;
    private Transform uiCanvas;

    private AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip pourSound;
    public AudioClip winSound;

    public Sprite tubeSprite;
    public Sprite capSprite;
    public Sprite layerSprite;
    public Sprite layerBottomSprite;
    public Sprite roundedButtonSprite;

    void Start()
    {
        UIHelper.RoundedButtonSprite = roundedButtonSprite;

        PlayerProfile.TryAutoSetup();

        SetupLighting();
        SetupCamera();
        SetupAudio();
        SetupToolbar();
        SetupWinUI();
        SetupCountryPanel();

        int saved = PlayerPrefs.GetInt("WaterSort_Level", 1);
        LoadLevel(saved);
    }

    void SetupAudio()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;

        if (clickSound == null) clickSound = Resources.Load<AudioClip>("Sounds/click");
        if (pourSound == null) pourSound = Resources.Load<AudioClip>("Sounds/pour");
        if (winSound == null) winSound = Resources.Load<AudioClip>("Sounds/win");
    }

    void PlaySound(AudioClip clip)
    {
        if (!GameSettings.SoundEnabled) return;
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    void SetupLighting()
    {
        GameObject lightObj = new GameObject("MainLight");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.98f, 0.95f);
        light.intensity = 1.1f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.shadows = LightShadows.None;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.9f, 0.9f, 0.95f);
        RenderSettings.ambientEquatorColor = new Color(0.75f, 0.75f, 0.8f);
        RenderSettings.ambientGroundColor = new Color(0.6f, 0.6f, 0.65f);
    }

    void SetupCamera()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        cam.orthographic = true;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.94f, 0.94f, 0.96f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        AdjustCameraToFitTubes();
    }

    void AdjustCameraToFitTubes()
    {
        float worldWidth = (MAX_PER_ROW - 1) * TUBE_SPACING + 1.6f;
        float screenAspect = (float)Screen.width / Screen.height;
        float sizeByWidth = worldWidth / (2f * screenAspect);

        float heightNeeded = ROW_SPACING + TubeVisual.TUBE_HEIGHT + 2.5f;
        float sizeByHeight = heightNeeded / 2f;

        cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
    }

    void SetupToolbar()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        uiCanvas = canvasObj.transform;

        GameObject tbObj = new GameObject("Toolbar");
        tbObj.transform.SetParent(transform, false);
        toolbar = tbObj.AddComponent<Toolbar>();
        toolbar.Build(canvasObj.transform);

        toolbar.OnSettingsClicked += OpenSettings;
        toolbar.OnLeaderboardClicked += OpenLeaderboard;
        toolbar.OnUndoClicked += OnUndoClicked;
        toolbar.OnHintClicked += OnHintClicked;
        toolbar.OnRefreshClicked += () => { PlaySound(clickSound); StartNewGameInternal(); };
    }

    void SetupWinUI()
    {
        GameObject winPanelObj = new GameObject("WinPanel");
        winPanelObj.transform.SetParent(uiCanvas, false);
        winPanel = winPanelObj.AddComponent<Image>();
        winPanel.color = new Color(1f, 1f, 1f, 0.92f);
        if (roundedButtonSprite != null)
        {
            winPanel.sprite = roundedButtonSprite;
            winPanel.type = Image.Type.Sliced;
        }
        RectTransform wpRt = winPanelObj.GetComponent<RectTransform>();
        wpRt.anchorMin = new Vector2(0.5f, 0.5f);
        wpRt.anchorMax = new Vector2(0.5f, 0.5f);
        wpRt.pivot = new Vector2(0.5f, 0.5f);
        wpRt.anchoredPosition = new Vector2(0, 300);
        wpRt.sizeDelta = new Vector2(800, 220);
        winPanelObj.SetActive(false);

        GameObject winTextObj = new GameObject("Win");
        winTextObj.transform.SetParent(uiCanvas, false);
        winText = winTextObj.AddComponent<Text>();
        winText.text = "ПОБЕДА!";
        winText.font = UIHelper.Font;
        winText.fontSize = 100;
        winText.fontStyle = FontStyle.Bold;
        winText.alignment = TextAnchor.MiddleCenter;
        winText.color = new Color(0.95f, 0.65f, 0.1f);
        winText.raycastTarget = false;
        RectTransform wtRt = winText.rectTransform;
        wtRt.anchorMin = new Vector2(0.5f, 0.5f);
        wtRt.anchorMax = new Vector2(0.5f, 0.5f);
        wtRt.pivot = new Vector2(0.5f, 0.5f);
        wtRt.anchoredPosition = new Vector2(0, 300);
        wtRt.sizeDelta = new Vector2(900, 200);
        winTextObj.SetActive(false);

        nextButtonObj = new GameObject("NextButton");
        nextButtonObj.transform.SetParent(uiCanvas, false);
        Image nextImg = nextButtonObj.AddComponent<Image>();
        nextImg.color = new Color(0.3f, 0.75f, 0.4f);
        if (roundedButtonSprite != null)
        {
            nextImg.sprite = roundedButtonSprite;
            nextImg.type = Image.Type.Sliced;
        }
        Button nextButton = nextButtonObj.AddComponent<Button>();
        nextButton.onClick.AddListener(NextLevel);

        RectTransform nextRt = nextButtonObj.GetComponent<RectTransform>();
        nextRt.anchorMin = new Vector2(0.5f, 0f);
        nextRt.anchorMax = new Vector2(0.5f, 0f);
        nextRt.pivot = new Vector2(0.5f, 0f);
        nextRt.anchoredPosition = new Vector2(0, 220);
        nextRt.sizeDelta = new Vector2(500, 120);

        GameObject nextTextObj = new GameObject("NextText");
        nextTextObj.transform.SetParent(nextButtonObj.transform, false);
        Text nt = nextTextObj.AddComponent<Text>();
        nt.text = "Следующий →";
        nt.font = UIHelper.Font;
        nt.fontSize = 50;
        nt.alignment = TextAnchor.MiddleCenter;
        nt.color = Color.white;
        nt.raycastTarget = false;
        RectTransform ntRt = nt.rectTransform;
        ntRt.anchorMin = Vector2.zero;
        ntRt.anchorMax = Vector2.one;
        ntRt.offsetMin = Vector2.zero;
        ntRt.offsetMax = Vector2.zero;

        nextButtonObj.SetActive(false);
    }

    void SetupCountryPanel()
    {
        GameObject panelObj = new GameObject("CountrySelectPanel");
        panelObj.transform.SetParent(transform, false);
        countryPanel = panelObj.AddComponent<CountrySelectPanel>();
        // Панель при старте не показываем — страна ставится автоматически в TryAutoSetup().
        // Пользователь может открыть её вручную через Настройки → Профиль → Страна.
    }

    void OpenSettings()
    {
        if (transform.Find("SettingsPanel") != null) return;
        PlaySound(clickSound);
        GameObject spObj = new GameObject("SettingsPanel");
        spObj.transform.SetParent(transform, false);
        SettingsPanel sp = spObj.AddComponent<SettingsPanel>();
        sp.Show(OpenProfile, ResetProgress);
    }

    void OpenProfile()
    {
        if (transform.Find("ProfilePanel") != null) return;
        GameObject ppObj = new GameObject("ProfilePanel");
        ppObj.transform.SetParent(transform, false);
        ProfilePanel pp = ppObj.AddComponent<ProfilePanel>();
        pp.Show();
    }

    void OpenLeaderboard()
    {
        if (transform.Find("LeaderboardPanel") != null) return;
        PlaySound(clickSound);
        GameObject lpObj = new GameObject("LeaderboardPanel");
        lpObj.transform.SetParent(transform, false);
        LeaderboardPanel lp = lpObj.AddComponent<LeaderboardPanel>();
        lp.Show();
    }

    void ResetProgress()
    {
        PlayerPrefs.SetInt("WaterSort_Level", 1);
        PlayerPrefs.Save();
        LoadLevel(1);
    }

    // ============================================================
    // Уровни
    // ============================================================

    void LoadLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
        PlayerPrefs.SetInt("WaterSort_Level", currentLevel);
        PlayerPrefs.Save();

        int colorCount, tubeCount, shuffleSteps;
        LevelGenerator.GetLevelParams(currentLevel, out colorCount, out tubeCount, out shuffleSteps);

        COLOR_COUNT = colorCount;
        TUBE_COUNT = tubeCount;
        if (TUBE_COUNT > MAX_TUBES) TUBE_COUNT = MAX_TUBES;

        colors = ColorPalette.GetContrastingColors(COLOR_COUNT);

        if (toolbar != null) toolbar.SetLevel(currentLevel);
        StartNewGameInternal();
    }

    void NextLevel()
    {
        PlaySound(clickSound);
        LoadLevel(currentLevel + 1);
    }

    void StartNewGameInternal()
    {
        StopAllCoroutines();
        isAnimating = false;
        pendingClick = false;
        PourAnimator.ResetSkip();

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        tubeVisuals.Clear();
        tubes.Clear();
        selectedTube = -1;
        gameWon = false;
        moveHistory.Clear();

        if (winText != null) winText.gameObject.SetActive(false);
        if (winPanel != null) winPanel.gameObject.SetActive(false);
        if (nextButtonObj != null) nextButtonObj.SetActive(false);

        int cc, tc, ss;
        LevelGenerator.GetLevelParams(currentLevel, out cc, out tc, out ss);

        tubes = LevelGenerator.Generate(cc, tc, 0);

        if (toolbar != null) toolbar.SetHint("Выбери колбочку");
        CreateTubeVisuals();
    }

    void CalculateTubePositions()
    {
        tubePositions.Clear();
        int rows = (TUBE_COUNT <= MAX_PER_ROW) ? 1 : 2;

        if (rows == 1)
        {
            float totalWidth = (TUBE_COUNT - 1) * TUBE_SPACING;
            float startX = -totalWidth / 2f;
            for (int i = 0; i < TUBE_COUNT; i++)
                tubePositions.Add(new Vector3(startX + i * TUBE_SPACING, 0f, 0f));
        }
        else
        {
            int topCount = Mathf.CeilToInt(TUBE_COUNT / 2f);
            int bottomCount = TUBE_COUNT - topCount;

            float topWidth = (topCount - 1) * TUBE_SPACING;
            float topStartX = -topWidth / 2f;
            for (int i = 0; i < topCount; i++)
                tubePositions.Add(new Vector3(topStartX + i * TUBE_SPACING, ROW_SPACING / 2f, 0f));

            float bottomWidth = (bottomCount - 1) * TUBE_SPACING;
            float bottomStartX = -bottomWidth / 2f;
            for (int i = 0; i < bottomCount; i++)
                tubePositions.Add(new Vector3(bottomStartX + i * TUBE_SPACING, -ROW_SPACING / 2f, 0f));
        }
    }

    void CreateTubeVisuals()
    {
        CalculateTubePositions();

        for (int i = 0; i < TUBE_COUNT; i++)
        {
            GameObject tubeObj = new GameObject("Tube_" + i);
            spawnedObjects.Add(tubeObj);

            TubeVisual tv = tubeObj.AddComponent<TubeVisual>();
            tv.Setup(i, tubePositions[i], colors, tubeSprite, capSprite, layerSprite, layerBottomSprite);
            tv.SetLayers(tubes[i]);
            tubeVisuals.Add(tv);
        }
    }

    // ============================================================
    // Ввод
    // ============================================================

    void Update()
    {
        if (gameWon) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isAnimating)
            {
                PourAnimator.RequestSkip();
                pendingClick = true;
            }
            else
            {
                ProcessClick();
            }
            return;
        }

        if (pendingClick && !isAnimating)
        {
            pendingClick = false;
            ProcessClick();
        }
    }

    void ProcessClick()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        int clickedTube = GetTubeAtPosition(mousePos);
        if (clickedTube >= 0) OnTubeClicked(clickedTube);
    }

    int GetTubeAtPosition(Vector3 worldPos)
    {
        for (int i = 0; i < tubePositions.Count; i++)
        {
            Vector3 p = tubePositions[i];
            float halfW = TubeVisual.TUBE_WIDTH / 2f + 0.15f;
            float halfH = TubeVisual.TUBE_HEIGHT / 2f;
            if (worldPos.x > p.x - halfW && worldPos.x < p.x + halfW &&
                worldPos.y > p.y - halfH && worldPos.y < p.y + halfH)
                return i;
        }
        return -1;
    }

    void OnTubeClicked(int tubeIndex)
    {
        if (selectedTube == -1)
        {
            if (tubes[tubeIndex].Count == 0)
            {
                toolbar.SetHint("Пустая колбочка — выбери другую");
                PlaySound(clickSound);
                return;
            }
            selectedTube = tubeIndex;
            toolbar.SetHint("Выбрана колбочка " + (tubeIndex + 1));
            tubeVisuals[tubeIndex].SetHighlight(true);
            HighlightTargets(tubeIndex);
            PlaySound(clickSound);
        }
        else if (selectedTube == tubeIndex)
        {
            ClearHighlights();
            selectedTube = -1;
            toolbar.SetHint("Выбери колбочку");
            PlaySound(clickSound);
        }
        else
        {
            if (CanPour(selectedTube, tubeIndex))
            {
                ClearHighlights();
                PlaySound(pourSound);
                StartCoroutine(PourAnimation(selectedTube, tubeIndex));
            }
            else
            {
                ClearHighlights();
                selectedTube = -1;
                PlaySound(clickSound);

                if (tubes[tubeIndex].Count > 0)
                {
                    selectedTube = tubeIndex;
                    tubeVisuals[tubeIndex].SetHighlight(true);
                    HighlightTargets(tubeIndex);
                    toolbar.SetHint("Выбрана колбочка " + (tubeIndex + 1));
                }
                else
                {
                    toolbar.SetHint("Так нельзя. Выбери другую");
                }
            }
        }
    }

    bool CanPour(int from, int to)
    {
        if (tubes[from].Count == 0) return false;
        if (tubes[to].Count >= LAYERS_PER_TUBE) return false;
        int topColor = tubes[from][tubes[from].Count - 1];
        if (tubes[to].Count > 0 && tubes[to][tubes[to].Count - 1] != topColor)
            return false;
        return true;
    }

    int GetPourCount(int from, int to)
    {
        int topColor = tubes[from][tubes[from].Count - 1];
        int count = 0;
        for (int i = tubes[from].Count - 1; i >= 0; i--)
        {
            if (tubes[from][i] == topColor) count++;
            else break;
        }
        int freeSpace = LAYERS_PER_TUBE - tubes[to].Count;
        return Mathf.Min(count, freeSpace);
    }

    // ============================================================
    // Перелив
    // ============================================================

    IEnumerator PourAnimation(int from, int to)
    {
        isAnimating = true;
        PourAnimator.ResetSkip();

        int topColor = tubes[from][tubes[from].Count - 1];
        int pourCount = GetPourCount(from, to);

        TubeVisual fromTV = tubeVisuals[from];
        TubeVisual toTV = tubeVisuals[to];

        System.Action onTakeFromSource = () =>
        {
            tubes[from].RemoveAt(tubes[from].Count - 1);
            fromTV.SetLayers(tubes[from]);
        };

        System.Func<GameObject> onAddToTarget = () =>
        {
            tubes[to].Add(topColor);
            toTV.SetLayers(tubes[to]);
            if (toTV.layerObjs.Count > 0)
                return toTV.layerObjs[toTV.layerObjs.Count - 1];
            return null;
        };

        if (GameSettings.AnimationEnabled)
        {
            yield return PourAnimator.Play(
                fromTV, toTV, colors[topColor], pourCount,
                onTakeFromSource, onAddToTarget);
        }
        else
        {
            for (int i = 0; i < pourCount; i++)
            {
                onTakeFromSource?.Invoke();
                onAddToTarget?.Invoke();
            }
        }

        moveHistory.Push(new MoveRecord { from = from, to = to, count = pourCount });

        selectedTube = -1;
        toolbar.SetHint("Выбери колбочку");
        isAnimating = false;

        CheckWin();
    }

    void HighlightTargets(int from)
    {
        for (int i = 0; i < TUBE_COUNT; i++)
        {
            if (i == from) continue;
            if (CanPour(from, i))
                tubeVisuals[i].SetHint(true);
        }
    }

    void ClearHighlights()
    {
        for (int i = 0; i < TUBE_COUNT; i++)
            tubeVisuals[i].SetHighlight(false);
    }

    // ============================================================
    // Undo / Hint
    // ============================================================

    void OnUndoClicked()
    {
        if (gameWon) return;
        if (isAnimating)
        {
            PourAnimator.RequestSkip();
            return;
        }
        if (moveHistory.Count == 0)
        {
            toolbar.SetHint("Нет ходов для отмены");
            PlaySound(clickSound);
            return;
        }

        MoveRecord move = moveHistory.Pop();

        for (int i = 0; i < move.count; i++)
        {
            if (tubes[move.to].Count == 0) break;
            int top = tubes[move.to][tubes[move.to].Count - 1];
            tubes[move.to].RemoveAt(tubes[move.to].Count - 1);
            tubes[move.from].Add(top);
        }

        tubeVisuals[move.from].SetLayers(tubes[move.from]);
        tubeVisuals[move.to].SetLayers(tubes[move.to]);

        selectedTube = -1;
        ClearHighlights();
        toolbar.SetHint("Ход отменён");
        PlaySound(clickSound);
    }

    void OnHintClicked()
    {
        if (gameWon) return;
        if (isAnimating) return;

        List<(int from, int to)> valid = new List<(int, int)>();
        for (int from = 0; from < TUBE_COUNT; from++)
        {
            if (tubes[from].Count == 0) continue;
            if (IsTubeSolved(from)) continue;

            for (int to = 0; to < TUBE_COUNT; to++)
            {
                if (to == from) continue;
                if (!CanPour(from, to)) continue;
                if (IsUselessMove(from, to)) continue;

                valid.Add((from, to));
            }
        }

        if (valid.Count == 0)
        {
            toolbar.SetHint("Ходов нет. Нажми ⟳ для нового поля");
            PlaySound(clickSound);
            return;
        }

        var pick = valid[Random.Range(0, valid.Count)];
        StartCoroutine(ShowHint(pick.from, pick.to));
    }

    bool IsTubeSolved(int index)
    {
        var t = tubes[index];
        if (t.Count != LAYERS_PER_TUBE) return false;
        for (int i = 1; i < t.Count; i++)
            if (t[i] != t[0]) return false;
        return true;
    }

    bool IsUselessMove(int from, int to)
    {
        if (tubes[to].Count == 0)
        {
            bool allSame = true;
            int first = tubes[from][0];
            for (int i = 1; i < tubes[from].Count; i++)
                if (tubes[from][i] != first) { allSame = false; break; }
            if (allSame) return true;
        }
        return false;
    }

    IEnumerator ShowHint(int from, int to)
    {
        tubeVisuals[from].SetHint(true);
        tubeVisuals[to].SetHint(true);
        toolbar.SetHint($"Подсказка: {from + 1} → {to + 1}");

        yield return new WaitForSeconds(1.5f);

        tubeVisuals[from].SetHint(false);
        tubeVisuals[to].SetHint(false);
        toolbar.SetHint("Выбери колбочку");
    }

    // ============================================================
    // Победа
    // ============================================================

    void CheckWin()
    {
        foreach (List<int> tube in tubes)
        {
            if (tube.Count == 0) continue;
            if (tube.Count != LAYERS_PER_TUBE) return;
            int first = tube[0];
            foreach (int c in tube)
                if (c != first) return;
        }

        gameWon = true;
        PlaySound(winSound);
        StartCoroutine(WinAnimation());
        StartCoroutine(SubmitScoreAfterWin());
    }

    IEnumerator SubmitScoreAfterWin()
    {
        string deviceId = PlayerProfile.DeviceId;
        string playerName = PlayerProfile.PlayerName;
        string countryCode = PlayerProfile.CountryCode;
        int score = currentLevel * 100;

        yield return LeaderboardAPI.SubmitScore(deviceId, playerName, score, countryCode);
    }

    IEnumerator WinAnimation()
    {
        winText.text = "ПОБЕДА!";

        if (winPanel != null) winPanel.gameObject.SetActive(true);
        winText.gameObject.SetActive(true);

        // ---- Конфетти-салют ----
        if (layerSprite != null && cam != null)
        {
            Vector3 origin = new Vector3(0f, cam.transform.position.y + 1.0f, 0f);
            ConfettiEffect.Play(origin, layerSprite, this);
        }

        RectTransform txtRt = winText.rectTransform;
        RectTransform panelRt = winPanel.rectTransform;

        Vector2 txtStart = new Vector2(0, 700);
        Vector2 txtEnd = new Vector2(0, 300);
        Vector2 panelStart = new Vector2(0, 700);
        Vector2 panelEnd = new Vector2(0, 300);

        txtRt.anchoredPosition = txtStart;
        panelRt.anchoredPosition = panelStart;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.5f;
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            txtRt.anchoredPosition = Vector2.Lerp(txtStart, txtEnd, ease);
            panelRt.anchoredPosition = Vector2.Lerp(panelStart, panelEnd, ease);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        if (nextButtonObj != null) nextButtonObj.SetActive(true);
        toolbar.SetHint("Следующий уровень →");
    }
}