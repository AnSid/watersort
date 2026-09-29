using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TubeVisual : MonoBehaviour
{
    public const float TUBE_WIDTH = 0.8f;
    public const float TUBE_HEIGHT = 2.4f;

    public const float SVG_HEIGHT = 600f;
    public const float TUBE_BODY_TOP_SVG = 56f;
    public const float TUBE_BODY_BOTTOM_SVG = 545f;

    public static float WaterHeight =>
        TUBE_HEIGHT * (TUBE_BODY_BOTTOM_SVG - TUBE_BODY_TOP_SVG) / SVG_HEIGHT;

    public static float LAYER_HEIGHT => WaterHeight / 4f;

    public static float WaterBottomLocalY
    {
        get
        {
            float bottomOffsetFromSpriteBottom =
                (SVG_HEIGHT - TUBE_BODY_BOTTOM_SVG) / SVG_HEIGHT * TUBE_HEIGHT;
            return -TUBE_HEIGHT / 2f + bottomOffsetFromSpriteBottom - 0.1f;
        }
    }

    // ---- Базовые sortingOrder ----
    private const int ORDER_LAYER = 5;
    private const int ORDER_CAP = 5;
    private const int ORDER_TUBE = 10;

    // ---- Подъём при анимации ----
    private const int POUR_BOOST = 50;
    private bool _isPouring = false;

    public int tubeIndex;
    public List<GameObject> layerObjs = new List<GameObject>();

    private Sprite tubeSprite;
    private Sprite capSprite;
    private Sprite layerSprite;
    private Sprite layerBottomSprite;
    private Sprite layerTopSprite;

    private Transform layersRoot;
    private Color[] colors;
    private SpriteRenderer tubeRenderer;
    private SpriteRenderer capRenderer;
    private Transform capTransform;
    private float capRestY;

    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private float appearTimer = 0f;
    private const float APPEAR_DURATION = 0.4f;
    private Color baseColor = Color.white;
    private Color highlightColor = new Color(0.7f, 0.9f, 1f, 1f);
    private Color hintColor = new Color(0.8f, 1f, 0.8f, 1f);

    private static Sprite _shadowSprite;

    private const float CAP_CLOSE_DURATION = 0.3f;
    private const float CAP_CLOSE_START_OFFSET = 0.6f;

    public void Setup(int index, Vector3 pos, Color[] colorPalette,
        Sprite tube, Sprite cap, Sprite layer, Sprite layerBottom, Sprite layerTop)
    {
        tubeIndex = index;
        transform.position = pos;

        colors = colorPalette;
        tubeSprite = tube;
        capSprite = cap;
        layerSprite = layer;
        layerBottomSprite = layerBottom;
        layerTopSprite = layerTop;

        BuildVisual();
        PlayAppearAnimation();
    }

    void BuildVisual()
    {
        // ---- Тень под колбой ----
        GameObject shadowObj = new GameObject("ShadowSprite");
        shadowObj.transform.SetParent(transform, false);
        float shadowY = -TUBE_HEIGHT / 2f - 0.04f;
        shadowObj.transform.localPosition = new Vector3(0f, shadowY, 0f);

        SpriteRenderer shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = GetShadowSprite();
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.18f);
        shadowRenderer.sortingOrder = 1;

        if (shadowRenderer.sprite != null)
        {
            float spriteWidth = shadowRenderer.sprite.bounds.size.x;
            float spriteHeight = shadowRenderer.sprite.bounds.size.y;
            float scaleX = (TUBE_WIDTH * 0.72f) / spriteWidth;
            float scaleY = (TUBE_WIDTH * 0.18f) / spriteHeight;
            shadowObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // ---- Колба ----
        GameObject tubeObj = new GameObject("TubeSprite");
        tubeObj.transform.SetParent(transform);
        tubeObj.transform.localPosition = Vector3.zero;
        tubeRenderer = tubeObj.AddComponent<SpriteRenderer>();
        tubeRenderer.sprite = tubeSprite;
        tubeRenderer.sortingOrder = ORDER_TUBE;

        if (tubeSprite != null)
        {
            float spriteWidth = tubeSprite.bounds.size.x;
            float spriteHeight = tubeSprite.bounds.size.y;
            float scaleX = TUBE_WIDTH / spriteWidth;
            float scaleY = TUBE_HEIGHT / spriteHeight;
            tubeObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        // ---- Слои ----
        GameObject rootObj = new GameObject("LayersRoot");
        rootObj.transform.SetParent(transform);
        rootObj.transform.localPosition = Vector3.zero;
        layersRoot = rootObj.transform;

        // ---- Крышка (пробка) ----
        GameObject capObj = new GameObject("CapSprite");
        capObj.transform.SetParent(transform);
        capRestY = TUBE_HEIGHT / 2f - 0.05f;
        capObj.transform.localPosition = new Vector3(0, capRestY, 0);
        capTransform = capObj.transform;
        capRenderer = capObj.AddComponent<SpriteRenderer>();
        capRenderer.sprite = capSprite;
        capRenderer.sortingOrder = ORDER_CAP;
        capObj.SetActive(false);

        if (capSprite != null)
        {
            float spriteWidth = capSprite.bounds.size.x;
            float spriteHeight = capSprite.bounds.size.y;
            float capWorldW = TUBE_WIDTH * 1.3f;
            float capWorldH = TUBE_HEIGHT * 0.1f;
            float scaleX = capWorldW / spriteWidth;
            float scaleY = capWorldH / spriteHeight;
            capObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    static Sprite GetShadowSprite()
    {
        if (_shadowSprite != null) return _shadowSprite;

        int W = 128;
        int H = 32;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] px = new Color[W * H];
        float cx = W * 0.5f;
        float cy = H * 0.5f;
        float rx = W * 0.5f;
        float ry = H * 0.5f;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float dx = (x + 0.5f - cx) / rx;
                float dy = (y + 0.5f - cy) / ry;
                float d = dx * dx + dy * dy;
                if (d >= 1f)
                {
                    px[y * W + x] = new Color(0f, 0f, 0f, 0f);
                }
                else
                {
                    float a = 1f - Mathf.Sqrt(d);
                    a = a * a;
                    px[y * W + x] = new Color(0f, 0f, 0f, a);
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();

        _shadowSprite = Sprite.Create(
            tex,
            new Rect(0, 0, W, H),
            new Vector2(0.5f, 0.5f),
            128f
        );
        return _shadowSprite;
    }

    int CurrentLayerOrder => _isPouring ? ORDER_LAYER + POUR_BOOST : ORDER_LAYER;

    public void SetLayers(List<int> layers)
    {
        foreach (GameObject obj in layerObjs)
            if (obj != null) Destroy(obj);
        layerObjs.Clear();

        int layerOrder = CurrentLayerOrder;

        for (int i = 0; i < layers.Count; i++)
        {
            GameObject layerObj = new GameObject("Layer_" + i);
            layerObj.transform.SetParent(layersRoot, false);

            float y = WaterBottomLocalY + LAYER_HEIGHT / 2f + i * LAYER_HEIGHT;
            layerObj.transform.localPosition = new Vector3(0, y, 0);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();

            bool isTop = (i == layers.Count - 1);
            bool isBottom = (i == 0);

            if (isTop && layerTopSprite != null)
                sr.sprite = layerTopSprite;
            else if (isBottom && layerBottomSprite != null)
                sr.sprite = layerBottomSprite;
            else
                sr.sprite = layerSprite;

            sr.color = colors[layers[i]];
            sr.sortingOrder = layerOrder;

            Sprite usedSprite = sr.sprite;
            if (usedSprite != null)
            {
                float spriteWidth = usedSprite.bounds.size.x;
                float spriteHeight = usedSprite.bounds.size.y;
                float scaleX = (TUBE_WIDTH * 0.85f) / spriteWidth;
                float scaleY = LAYER_HEIGHT / spriteHeight;
                layerObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            layerObjs.Add(layerObj);
        }

        bool isSolved = (layers.Count == 4 &&
                         layers[0] == layers[1] &&
                         layers[1] == layers[2] &&
                         layers[2] == layers[3]);

        if (capRenderer != null && !isSolved)
            capRenderer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Включить/выключить режим анимации перелива.
    /// Поднимает sortingOrder колбы, крышки и слоёв, чтобы колба
    /// была поверх остальных во время перелива.
    /// </summary>
    public void SetPouring(bool pouring)
    {
        if (_isPouring == pouring) return;
        _isPouring = pouring;

        int boost = pouring ? POUR_BOOST : 0;

        if (tubeRenderer != null)
            tubeRenderer.sortingOrder = ORDER_TUBE + boost;
        if (capRenderer != null)
            capRenderer.sortingOrder = ORDER_CAP + boost;

        int layerOrder = ORDER_LAYER + boost;
        for (int i = 0; i < layerObjs.Count; i++)
        {
            if (layerObjs[i] == null) continue;
            SpriteRenderer sr = layerObjs[i].GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = layerOrder;
        }
    }

    public void PlayCapCloseAnimation()
    {
        if (capRenderer == null || capTransform == null) return;
        StartCoroutine(CapCloseRoutine());
    }

    IEnumerator CapCloseRoutine()
    {
        capRenderer.gameObject.SetActive(true);
        capTransform.localPosition = new Vector3(0f, capRestY + CAP_CLOSE_START_OFFSET, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / CAP_CLOSE_DURATION;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            float y = Mathf.Lerp(capRestY + CAP_CLOSE_START_OFFSET, capRestY, ease);
            capTransform.localPosition = new Vector3(0f, y, 0f);
            yield return null;
        }

        capTransform.localPosition = new Vector3(0f, capRestY, 0f);
    }

    public GameObject CreateFlyingLayer(int colorIndex, Vector3 startWorldPos, Vector3 endWorldPos)
    {
        GameObject layerObj = new GameObject("FlyingLayer");
        layerObj.transform.position = startWorldPos;

        SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
        sr.sprite = layerSprite;
        sr.color = colors[colorIndex];
        sr.sortingOrder = 20;

        if (layerSprite != null)
        {
            float spriteWidth = layerSprite.bounds.size.x;
            float spriteHeight = layerSprite.bounds.size.y;
            float scaleX = (TUBE_WIDTH * 0.85f) / spriteWidth;
            float scaleY = LAYER_HEIGHT / spriteHeight;
            layerObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        return layerObj;
    }

    public Vector3 GetLayerWorldPosition(int layerIndex)
    {
        float y = WaterBottomLocalY + LAYER_HEIGHT / 2f + layerIndex * LAYER_HEIGHT;
        return transform.position + new Vector3(0, y, 0f);
    }

    public Vector3 GetMouthWorldPosition()
    {
        Vector3 localMouth = new Vector3(0f, TUBE_HEIGHT / 2f, 0f);
        return transform.TransformPoint(localMouth);
    }

    public Vector3 GetMouthEdgeWorldPosition(float sideSign)
    {
        float halfNeckWidth = TUBE_WIDTH * 0.5f;
        Vector3 localEdge = new Vector3(sideSign * halfNeckWidth, TUBE_HEIGHT / 2f, 0f);
        return transform.TransformPoint(localEdge);
    }

    public float GetLayerBaseLocalY(int layerIndex)
    {
        return WaterBottomLocalY + layerIndex * LAYER_HEIGHT;
    }

    public void SetHighlight(bool on)
    {
        isHighlighted = on;
        if (on) pulseTimer = 0f;
        else transform.localScale = Vector3.one;

        if (tubeRenderer != null)
            tubeRenderer.color = on ? highlightColor : baseColor;
    }

    public void SetHint(bool on)
    {
        if (tubeRenderer != null)
            tubeRenderer.color = on ? hintColor : baseColor;
    }

    void Update()
    {
        if (isHighlighted)
        {
            pulseTimer += Time.deltaTime * 4f;
            float scale = 1f + 0.05f * Mathf.Sin(pulseTimer);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        if (appearTimer > 0f)
        {
            appearTimer -= Time.deltaTime;
            float t = 1f - (appearTimer / APPEAR_DURATION);
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            float scale = Mathf.Lerp(0f, 1f, ease);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    void PlayAppearAnimation()
    {
        transform.localScale = Vector3.zero;
        appearTimer = APPEAR_DURATION;
    }
}