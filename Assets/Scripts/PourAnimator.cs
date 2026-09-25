using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Отвечает ТОЛЬКО за визуал перелива.
/// Струя растёт сверху вниз, оканчивается на уровне жидкости в приёмнике.
/// </summary>
public static class PourAnimator
{
    public const bool DEBUG = false;

    private static bool skipRequested = false;

    public static void RequestSkip() { skipRequested = true; }
    public static void ResetSkip() { skipRequested = false; }

    // ---- Параметры анимации ----
    public const float MOVE_DURATION = 0.25f;
    public const float TILT_DURATION = 0.20f;
    public const float TILT_ANGLE = 50f;
    public const float FLOW_DURATION = 0.30f;
    public const float RETURN_DURATION = 0.25f;
    public const float HOVER_HEIGHT = 0.5f;
    public const float DELAY_BETWEEN_LAYERS = 0.10f;

    // ---- Параметры струи (обновлены) ----
    public const float STREAM_START_WIDTH = 0.10f;
    public const float STREAM_END_WIDTH = 0.07f;
    public const float STREAM_ALPHA = 0.9f;
    public const int STREAM_POINTS = 10;
    public const float STREAM_CURVE = 0.05f;
    public const float STREAM_WOBBLE_AMP = 0.025f;
    public const float STREAM_WOBBLE_SPEED = 22f;
    public const float STREAM_GROW_DURATION = 0.15f; // время «прорастания» струи сверху вниз

    // ---- Кэш материала ----
    private static Material _streamMaterial;
    private static Material StreamMaterial
    {
        get
        {
            if (_streamMaterial == null)
                _streamMaterial = new Material(Shader.Find("Sprites/Default"));
            return _streamMaterial;
        }
    }

    public static IEnumerator Play(
        TubeVisual fromTV,
        TubeVisual toTV,
        Color topColor,
        int pourCount,
        Action onTakeFromSource,
        Func<GameObject> onAddToTarget)
    {
        skipRequested = false;

        Vector3 fromOriginalPos = fromTV.transform.position;
        Quaternion fromOriginalRot = fromTV.transform.rotation;
        Transform fromOriginalParent = fromTV.transform.parent;
        int fromOriginalSiblingIndex = fromTV.transform.GetSiblingIndex();

        float dir = Mathf.Sign(toTV.transform.position.x - fromOriginalPos.x);
        if (Mathf.Approximately(dir, 0f)) dir = 1f;

        Vector3 toMouth = toTV.GetMouthWorldPosition();
        Vector3 hoverPos = new Vector3(toMouth.x, toMouth.y + HOVER_HEIGHT, 0f);
        Vector3 fromMouth = fromTV.GetMouthWorldPosition();

        // 1. PourPivot
        GameObject pivotObj = new GameObject("PourPivot");
        pivotObj.transform.position = fromMouth;
        pivotObj.transform.rotation = Quaternion.identity;

        Vector3 savedFromWorldPos = fromTV.transform.position;
        Quaternion savedFromWorldRot = fromTV.transform.rotation;

        fromTV.transform.SetParent(pivotObj.transform, false);
        fromTV.transform.position = savedFromWorldPos;
        fromTV.transform.rotation = savedFromWorldRot;

        // 2. Подлёт
        if (!skipRequested)
        {
            float t = 0f;
            while (t < 1f)
            {
                if (skipRequested) { t = 1f; break; }
                t += Time.deltaTime / MOVE_DURATION;
                float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                pivotObj.transform.position = Vector3.Lerp(fromMouth, hoverPos, ease);
                yield return null;
            }
        }
        pivotObj.transform.position = hoverPos;

        // 3. Наклон
        float targetAngle = -dir * TILT_ANGLE;
        Quaternion endRot = Quaternion.Euler(0f, 0f, targetAngle);

        if (!skipRequested)
        {
            float t = 0f;
            Quaternion startRot = pivotObj.transform.rotation;
            while (t < 1f)
            {
                if (skipRequested) { t = 1f; break; }
                t += Time.deltaTime / TILT_DURATION;
                float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                pivotObj.transform.rotation = Quaternion.Slerp(startRot, endRot, ease);
                yield return null;
            }
        }
        pivotObj.transform.rotation = endRot;

        // 4. Струя (создаём LineRenderer сразу, длину будем «растить»)
        GameObject streamObj = null;
        LineRenderer lr = null;
        float curveDir = dir;

        if (!skipRequested)
        {
            streamObj = new GameObject("PourStream");
            lr = streamObj.AddComponent<LineRenderer>();
            lr.positionCount = STREAM_POINTS;
            lr.useWorldSpace = true;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 4;
            lr.material = StreamMaterial;
            Color streamColor = topColor;
            streamColor.a = STREAM_ALPHA;
            lr.startColor = streamColor;
            lr.endColor = streamColor;
            lr.sortingOrder = 12;

            AnimationCurve widthCurve = new AnimationCurve();
            widthCurve.AddKey(0f, STREAM_START_WIDTH);
            widthCurve.AddKey(1f, STREAM_END_WIDTH);
            lr.widthCurve = widthCurve;

            // Стартуем с нулевой длины — все точки в горлышке
            Vector3 startPos = fromTV.GetMouthEdgeWorldPosition(dir);
            for (int p = 0; p < STREAM_POINTS; p++)
                lr.SetPosition(p, startPos);
        }

        // 5. Перелив слоёв
        for (int i = 0; i < pourCount; i++)
        {
            onTakeFromSource?.Invoke();
            GameObject targetLayer = onAddToTarget?.Invoke();

            int targetIndex = toTV.layerObjs.Count - 1;
            Vector3 finalScale = targetLayer != null ? targetLayer.transform.localScale : Vector3.one;
            float layerBaseY = toTV.GetLayerBaseLocalY(targetIndex);
            float halfLayerH = TubeVisual.LAYER_HEIGHT * 0.5f;

            if (targetLayer != null)
            {
                targetLayer.transform.localScale = new Vector3(finalScale.x, 0f, finalScale.z);
                targetLayer.transform.localPosition = new Vector3(0f, layerBaseY, 0f);
            }

            if (!skipRequested)
            {
                // Точка старта — горлышко источника
                Vector3 streamStart = fromTV.GetMouthEdgeWorldPosition(dir);

                // Точка конца — уровень жидкости в приёмнике (а не горлышко)
                float surfaceLocalY = layerBaseY + halfLayerH; // уровень поверхности слоя
                Vector3 surfaceWorld = toTV.transform.TransformPoint(new Vector3(0f, surfaceLocalY, 0f));
                Vector3 streamEnd = new Vector3(surfaceWorld.x, surfaceWorld.y, 0f);

                float t = 0f;
                float growProgress = 0f;

                while (t < 1f)
                {
                    if (skipRequested) { t = 1f; break; }
                    t += Time.deltaTime / FLOW_DURATION;
                    float ease = Mathf.Clamp01(t);

                    // Растим длину струи за STREAM_GROW_DURATION
                    if (growProgress < 1f)
                    {
                        growProgress += Time.deltaTime / STREAM_GROW_DURATION;
                        growProgress = Mathf.Clamp01(growProgress);
                    }

                    if (lr != null)
                    {
                        float wobble = Mathf.Sin(Time.time * STREAM_WOBBLE_SPEED) * STREAM_WOBBLE_AMP;
                        float amp = STREAM_CURVE + wobble;

                        for (int p = 0; p < STREAM_POINTS; p++)
                        {
                            float pt = (float)p / (STREAM_POINTS - 1);
                            // Каждая точка «проявляется» со сдвигом по growProgress
                            // точка 0 — сразу, последняя — при growProgress = 1
                            float visibleT = Mathf.Clamp01((growProgress - pt * 0.15f) / 0.85f);

                            Vector3 pos = Vector3.Lerp(streamStart, streamEnd, pt * visibleT);
                            float curve = Mathf.Sin(pt * visibleT * Mathf.PI) * amp;
                            pos.x += curveDir * curve;
                            lr.SetPosition(p, pos);
                        }
                    }

                    if (targetLayer != null)
                    {
                        float sy = finalScale.y * ease;
                        targetLayer.transform.localScale = new Vector3(finalScale.x, sy, finalScale.z);

                        float py = layerBaseY + halfLayerH * ease;
                        targetLayer.transform.localPosition = new Vector3(0f, py, 0f);
                    }

                    yield return null;
                }
            }

            if (targetLayer != null)
            {
                targetLayer.transform.localScale = finalScale;
                targetLayer.transform.localPosition = new Vector3(0f, layerBaseY + halfLayerH, 0f);
            }

            if (i < pourCount - 1 && !skipRequested)
                yield return new WaitForSeconds(DELAY_BETWEEN_LAYERS);
        }

        if (streamObj != null)
            UnityEngine.Object.Destroy(streamObj);

        // 6. Возврат
        if (!skipRequested)
        {
            float t = 0f;
            Quaternion returnStart = pivotObj.transform.rotation;
            while (t < 1f)
            {
                if (skipRequested) { t = 1f; break; }
                t += Time.deltaTime / RETURN_DURATION;
                float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                pivotObj.transform.rotation = Quaternion.Slerp(returnStart, Quaternion.identity, ease);
                yield return null;
            }
        }
        pivotObj.transform.rotation = Quaternion.identity;

        if (!skipRequested)
        {
            float t = 0f;
            while (t < 1f)
            {
                if (skipRequested) { t = 1f; break; }
                t += Time.deltaTime / RETURN_DURATION;
                float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                pivotObj.transform.position = Vector3.Lerp(hoverPos, fromMouth, ease);
                yield return null;
            }
        }
        pivotObj.transform.position = fromMouth;

        fromTV.transform.SetParent(fromOriginalParent, false);
        fromTV.transform.SetSiblingIndex(fromOriginalSiblingIndex);
        fromTV.transform.position = fromOriginalPos;
        fromTV.transform.rotation = fromOriginalRot;

        UnityEngine.Object.Destroy(pivotObj);

        skipRequested = false;
    }
}