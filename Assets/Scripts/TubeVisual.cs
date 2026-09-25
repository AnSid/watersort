using UnityEngine;
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
            return -TUBE_HEIGHT / 2f + bottomOffsetFromSpriteBottom;
        }
    }

    public int tubeIndex;
    public List<GameObject> layerObjs = new List<GameObject>();

    private Sprite tubeSprite;
    private Sprite capSprite;
    private Sprite layerSprite;
    private Sprite layerBottomSprite;

    private Transform layersRoot;
    private Color[] colors;
    private SpriteRenderer tubeRenderer;
    private SpriteRenderer capRenderer;

    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    private float appearTimer = 0f;
    private const float APPEAR_DURATION = 0.4f;
    private Color baseColor = Color.white;
    private Color highlightColor = new Color(0.7f, 0.9f, 1f, 1f);
    private Color hintColor = new Color(0.8f, 1f, 0.8f, 1f);

    public void Setup(int index, Vector3 pos, Color[] colorPalette,
        Sprite tube, Sprite cap, Sprite layer, Sprite layerBottom)
    {
        tubeIndex = index;
        transform.position = pos;

        colors = colorPalette;
        tubeSprite = tube;
        capSprite = cap;
        layerSprite = layer;
        layerBottomSprite = layerBottom;

        BuildVisual();
        PlayAppearAnimation();
    }

    void BuildVisual()
    {
        GameObject tubeObj = new GameObject("TubeSprite");
        tubeObj.transform.SetParent(transform);
        tubeObj.transform.localPosition = Vector3.zero;
        tubeRenderer = tubeObj.AddComponent<SpriteRenderer>();
        tubeRenderer.sprite = tubeSprite;
        tubeRenderer.sortingOrder = 10;

        if (tubeSprite != null)
        {
            float spriteWidth = tubeSprite.bounds.size.x;
            float spriteHeight = tubeSprite.bounds.size.y;
            float scaleX = TUBE_WIDTH / spriteWidth;
            float scaleY = TUBE_HEIGHT / spriteHeight;
            tubeObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        GameObject rootObj = new GameObject("LayersRoot");
        rootObj.transform.SetParent(transform);
        rootObj.transform.localPosition = Vector3.zero;
        layersRoot = rootObj.transform;

        GameObject capObj = new GameObject("CapSprite");
        capObj.transform.SetParent(transform);
        float capY = TUBE_HEIGHT / 2f + 0.03f;
        capObj.transform.localPosition = new Vector3(0, capY, 0);
        capRenderer = capObj.AddComponent<SpriteRenderer>();
        capRenderer.sprite = capSprite;
        capRenderer.sortingOrder = 15;
        capObj.SetActive(false);

        if (capSprite != null)
        {
            float spriteWidth = capSprite.bounds.size.x;
            float spriteHeight = capSprite.bounds.size.y;
            float scaleX = (TUBE_WIDTH * 0.95f) / spriteWidth;
            float scaleY = (TUBE_HEIGHT * 0.05f) / spriteHeight;
            capObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    public void SetLayers(List<int> layers)
    {
        foreach (GameObject obj in layerObjs)
            if (obj != null) Destroy(obj);
        layerObjs.Clear();

        for (int i = 0; i < layers.Count; i++)
        {
            GameObject layerObj = new GameObject("Layer_" + i);
            layerObj.transform.SetParent(layersRoot, false);

            float y = WaterBottomLocalY + LAYER_HEIGHT / 2f + i * LAYER_HEIGHT;
            layerObj.transform.localPosition = new Vector3(0, y, 0);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = (i == 0 && layerBottomSprite != null) ? layerBottomSprite : layerSprite;
            sr.color = colors[layers[i]];
            sr.sortingOrder = 5;

            Sprite usedSprite = sr.sprite;
            if (usedSprite != null)
            {
                float spriteWidth = usedSprite.bounds.size.x;
                float spriteHeight = usedSprite.bounds.size.y;
                float scaleX = (TUBE_WIDTH * 0.7f) / spriteWidth;
                float scaleY = LAYER_HEIGHT / spriteHeight;
                layerObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            layerObjs.Add(layerObj);
        }

        bool isSolved = (layers.Count == 4 &&
                         layers[0] == layers[1] &&
                         layers[1] == layers[2] &&
                         layers[2] == layers[3]);
        if (capRenderer != null)
            capRenderer.gameObject.SetActive(isSolved);
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
            float scaleX = (TUBE_WIDTH * 0.7f) / spriteWidth;
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