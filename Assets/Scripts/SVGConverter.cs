using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class SVGConverter : MonoBehaviour
{
    // ==================== КОЛБА ====================

    [ContextMenu("Generate Tube PNG")]
    public void GenerateTube()
    {
        int W = 400;
        int H = 1200;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] clear = new Color[W * H];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        float scaleX = W / 200f;
        float scaleY = H / 600f;

        float bodyLeft = 55, bodyRight = 145;
        float bodyTop = 56, bodyBottom = 545;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < bodyTop || svgY > bodyBottom) continue;

            float bottomCurve = 1f;
            if (svgY > bodyBottom - 30)
            {
                float d = (svgY - (bodyBottom - 30)) / 30f;
                bottomCurve = Mathf.Sqrt(Mathf.Max(0, 1f - d * d));
            }

            float leftShift = (1f - bottomCurve) * 45f;
            float l = bodyLeft + leftShift;
            float r = bodyRight - leftShift;

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX >= l && svgX <= r)
                {
                    float t = (svgX - l) / (r - l);
                    tex.SetPixel(x, H - 1 - y, GlassGradient(t));
                }
            }
        }

        float neckTop = 34, neckBottom = 56;
        float neckTopLeft = 76, neckTopRight = 124;
        float neckBotLeft = 72, neckBotRight = 128;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < neckTop || svgY > neckBottom) continue;

            float t = (svgY - neckTop) / (neckBottom - neckTop);
            float l = Mathf.Lerp(neckTopLeft, neckBotLeft, t);
            float r = Mathf.Lerp(neckTopRight, neckBotRight, t);

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX >= l && svgX <= r)
                {
                    float tt = (svgX - l) / (r - l);
                    tex.SetPixel(x, H - 1 - y, GlassGradient(tt));
                }
            }
        }

        float rimTop = 20, rimBottom = 36;
        float rimLeft = 66, rimRight = 134;
        float rimRadius = 7f;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < rimTop || svgY > rimBottom) continue;

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < rimLeft || svgX > rimRight) continue;

                bool inside = true;
                float rl = rimLeft + rimRadius;
                float rr = rimRight - rimRadius;
                float rt = rimTop + rimRadius;
                float rb = rimBottom - rimRadius;

                if (svgX < rl && svgY < rt)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rl, rt)) <= rimRadius;
                else if (svgX > rr && svgY < rt)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rr, rt)) <= rimRadius;
                else if (svgX < rl && svgY > rb)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rl, rb)) <= rimRadius;
                else if (svgX > rr && svgY > rb)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rr, rb)) <= rimRadius;

                if (!inside) continue;

                float tt = (svgX - rimLeft) / (rimRight - rimLeft);
                tex.SetPixel(x, H - 1 - y, RimGradient(tt));
            }
        }

        float cx = 100, cy = 543;
        float rx = 44, ry = 8;
        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                float dx = (svgX - cx) / rx;
                float dy = (svgY - cy) / ry;
                float dist = dx * dx + dy * dy;
                if (dist <= 1f)
                {
                    float alpha = (1f - dist) * 0.45f;
                    Color c = new Color(0.32f, 0.45f, 0.58f, alpha);
                    BlendPixel(tex, x, H - 1 - y, c);
                }
            }
        }

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < 70 || svgY > 520) continue;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < 62 || svgX > 82) continue;

                float distFromCenter = (svgX - 71f) / 8f;
                float alpha = Mathf.Exp(-distFromCenter * distFromCenter * 2f) * 0.25f;

                float edgeFade = 1f;
                if (svgY < 100) edgeFade = (svgY - 70) / 30f;
                if (svgY > 490) edgeFade = (520 - svgY) / 30f;

                Color c = new Color(1f, 1f, 1f, alpha * edgeFade);
                BlendPixel(tex, x, H - 1 - y, c);
            }
        }

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < 120 || svgY > 460) continue;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < 124 || svgX > 138) continue;

                float distFromCenter = (svgX - 131f) / 6f;
                float alpha = Mathf.Exp(-distFromCenter * distFromCenter * 2f) * 0.14f;

                float edgeFade = 1f;
                if (svgY < 140) edgeFade = (svgY - 120) / 20f;
                if (svgY > 440) edgeFade = (460 - svgY) / 20f;

                Color c = new Color(1f, 1f, 1f, alpha * edgeFade);
                BlendPixel(tex, x, H - 1 - y, c);
            }
        }

        tex.Apply();
        SavePNG(tex, "tube.png");
    }

    // ==================== КОЛБА V2 ====================
    // Отличия от V1:
    //  - тело колбы (где жидкость) — alpha 0, цвет не искажается;
    //  - кромки слева/справа — плотнее (0.55), границы колбы читаются;
    //  - горлышко и ободок — 0.5–0.65;
    //  - блики — как в V1, но поверх кромок;
    //  - дно — 0.45.
    // Сохраняется в tube_v2.png, оригинал tube.png не трогается.

    [ContextMenu("Generate Tube V2 PNG")]
    public void GenerateTubeV2()
    {
        int W = 400;
        int H = 1200;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] clear = new Color[W * H];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        float scaleX = W / 200f;
        float scaleY = H / 600f;

        float bodyLeft = 55, bodyRight = 145;
        float bodyTop = 56, bodyBottom = 545;

        // Ширина кромки в SVG-координатах: 8 единиц (≈10% от ширины тела 90).
        float edgeW = 8f;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < bodyTop || svgY > bodyBottom) continue;

            // Скругление дна — как в V1.
            float bottomCurve = 1f;
            if (svgY > bodyBottom - 30)
            {
                float d = (svgY - (bodyBottom - 30)) / 30f;
                bottomCurve = Mathf.Sqrt(Mathf.Max(0, 1f - d * d));
            }

            float leftShift = (1f - bottomCurve) * 45f;
            float l = bodyLeft + leftShift;
            float r = bodyRight - leftShift;

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < l || svgX > r) continue;

                float fromLeft = svgX - l;
                float fromRight = r - svgX;
                float fromEdge = Mathf.Min(fromLeft, fromRight);

                if (fromEdge >= edgeW)
                {
                    // Тело — полностью прозрачное, чтобы не искажать жидкость.
                    continue;
                }

                // Кромка: у самого края alpha 0.55, к внутренней границе — 0.
                float t = fromEdge / edgeW; // 0 = внешний край, 1 = внутренняя граница
                float alpha = Mathf.Lerp(0.55f, 0f, t);

                // Лёгкий голубовато-серый оттенок стекла.
                Color c = new Color(0.78f, 0.82f, 0.86f, alpha);
                tex.SetPixel(x, H - 1 - y, c);
            }
        }

        // Горлышко — плотнее, 0.5.
        float neckTop = 34, neckBottom = 56;
        float neckTopLeft = 76, neckTopRight = 124;
        float neckBotLeft = 72, neckBotRight = 128;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < neckTop || svgY > neckBottom) continue;

            float t = (svgY - neckTop) / (neckBottom - neckTop);
            float l = Mathf.Lerp(neckTopLeft, neckBotLeft, t);
            float r = Mathf.Lerp(neckTopRight, neckBotRight, t);

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < l || svgX > r) continue;

                // У горлышка — сплошная плотность 0.5 (стекло видно).
                Color c = new Color(0.80f, 0.84f, 0.88f, 0.50f);
                tex.SetPixel(x, H - 1 - y, c);
            }
        }

        // Ободок — плотнее, 0.65.
        float rimTop = 20, rimBottom = 36;
        float rimLeft = 66, rimRight = 134;
        float rimRadius = 7f;

        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < rimTop || svgY > rimBottom) continue;

            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < rimLeft || svgX > rimRight) continue;

                bool inside = true;
                float rl = rimLeft + rimRadius;
                float rr = rimRight - rimRadius;
                float rt = rimTop + rimRadius;
                float rb = rimBottom - rimRadius;

                if (svgX < rl && svgY < rt)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rl, rt)) <= rimRadius;
                else if (svgX > rr && svgY < rt)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rr, rt)) <= rimRadius;
                else if (svgX < rl && svgY > rb)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rl, rb)) <= rimRadius;
                else if (svgX > rr && svgY > rb)
                    inside = Vector2.Distance(new Vector2(svgX, svgY), new Vector2(rr, rb)) <= rimRadius;

                if (!inside) continue;

                float tt = (svgX - rimLeft) / (rimRight - rimLeft);
                Color g = RimGradient(tt);
                g.a = 0.65f;
                tex.SetPixel(x, H - 1 - y, g);
            }
        }

        // Дно — эллипс, alpha 0.45 (как V1).
        float cx = 100, cy = 543;
        float rx = 44, ry = 8;
        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                float dx = (svgX - cx) / rx;
                float dy = (svgY - cy) / ry;
                float dist = dx * dx + dy * dy;
                if (dist <= 1f)
                {
                    float alpha = (1f - dist) * 0.45f;
                    Color c = new Color(0.32f, 0.45f, 0.58f, alpha);
                    BlendPixel(tex, x, H - 1 - y, c);
                }
            }
        }

        // Блик 1 (левый, широкий) — 0.30 вместо 0.25, поверх кромки.
        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < 70 || svgY > 520) continue;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < 62 || svgX > 82) continue;

                float distFromCenter = (svgX - 71f) / 8f;
                float alpha = Mathf.Exp(-distFromCenter * distFromCenter * 2f) * 0.30f;

                float edgeFade = 1f;
                if (svgY < 100) edgeFade = (svgY - 70) / 30f;
                if (svgY > 490) edgeFade = (520 - svgY) / 30f;

                Color c = new Color(1f, 1f, 1f, alpha * edgeFade);
                BlendPixel(tex, x, H - 1 - y, c);
            }
        }

        // Блик 2 (правый, узкий) — 0.18 вместо 0.14.
        for (int y = 0; y < H; y++)
        {
            float svgY = y / scaleY;
            if (svgY < 120 || svgY > 460) continue;
            for (int x = 0; x < W; x++)
            {
                float svgX = x / scaleX;
                if (svgX < 124 || svgX > 138) continue;

                float distFromCenter = (svgX - 131f) / 6f;
                float alpha = Mathf.Exp(-distFromCenter * distFromCenter * 2f) * 0.18f;

                float edgeFade = 1f;
                if (svgY < 140) edgeFade = (svgY - 120) / 20f;
                if (svgY > 440) edgeFade = (460 - svgY) / 20f;

                Color c = new Color(1f, 1f, 1f, alpha * edgeFade);
                BlendPixel(tex, x, H - 1 - y, c);
            }
        }

        tex.Apply();
        SavePNG(tex, "tube_v2.png");
    }

    // ==================== КРЫШКА ====================

    [ContextMenu("Generate Cap PNG")]
    public void GenerateCap()
    {
        int W = 256;
        int H = 96;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] clear = new Color[W * H];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        float topTop = 0.00f * H;
        float topBottom = 0.40f * H;
        float topLeft = 0.00f * W;
        float topRight = 1.00f * W;
        float topRadius = 0.20f * W;

        float baseLeft = 0.04f * W;
        float baseRight = 0.96f * W;
        float baseTop = 0.40f * H;
        float baseBottom = 1.00f * H;
        float baseRadius = 0.03f * W;

        for (int y = 0; y < H; y++)
        {
            float py = y + 0.5f;
            float svgY = H - py;

            if (svgY < topTop || svgY > topBottom) continue;

            for (int x = 0; x < W; x++)
            {
                float px = x + 0.5f;
                if (px < topLeft || px > topRight) continue;

                bool inside = true;
                float rl = topLeft + topRadius;
                float rr = topRight - topRadius;

                if (px < rl && svgY < topTop + topRadius)
                    inside = Vector2.Distance(new Vector2(px, svgY), new Vector2(rl, topTop + topRadius)) <= topRadius;
                else if (px > rr && svgY < topTop + topRadius)
                    inside = Vector2.Distance(new Vector2(px, svgY), new Vector2(rr, topTop + topRadius)) <= topRadius;

                if (!inside) continue;

                float t = (px - topLeft) / (topRight - topLeft);
                tex.SetPixel(x, y, CapTopGradient(t));
            }
        }

        for (int y = 0; y < H; y++)
        {
            float py = y + 0.5f;
            float svgY = H - py;

            if (svgY < baseTop || svgY > baseBottom) continue;

            for (int x = 0; x < W; x++)
            {
                float px = x + 0.5f;
                if (px < baseLeft || px > baseRight) continue;

                bool inside = true;
                float rl = baseLeft + baseRadius;
                float rr = baseRight - baseRadius;
                float rb = baseBottom - baseRadius;

                if (px < rl && svgY > rb)
                    inside = Vector2.Distance(new Vector2(px, svgY), new Vector2(rl, rb)) <= baseRadius;
                else if (px > rr && svgY > rb)
                    inside = Vector2.Distance(new Vector2(px, svgY), new Vector2(rr, rb)) <= baseRadius;

                if (!inside) continue;

                float t = (px - baseLeft) / (baseRight - baseLeft);
                tex.SetPixel(x, y, CapGradient(t));
            }
        }

        float capCx = 0.5f * W;
        float capCy = 0.18f * H;
        float capRx = 0.40f * W;
        float capRy = 0.08f * H;

        for (int y = 0; y < H; y++)
        {
            float svgY = H - (y + 0.5f);
            for (int x = 0; x < W; x++)
            {
                float svgX = x + 0.5f;
                float dx = (svgX - capCx) / capRx;
                float dy = (svgY - capCy) / capRy;
                float dist = dx * dx + dy * dy;
                if (dist <= 1f)
                {
                    float alpha = (1f - dist) * 0.35f;
                    Color c = new Color(1f, 1f, 1f, alpha);
                    BlendPixel(tex, x, y, c);
                }
            }
        }

        tex.Apply();
        SavePNG(tex, "cap.png");
    }

    // ==================== СЛОЙ ====================

    [ContextMenu("Generate Layer PNG")]
    public void GenerateLayer()
    {
        int W = 300;
        int H = 120;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] fill = new Color[W * H];
        for (int i = 0; i < fill.Length; i++) fill[i] = Color.white;
        tex.SetPixels(fill);
        tex.Apply();

        SavePNG(tex, "layer.png");
    }

    [ContextMenu("Generate Layer Bottom PNG")]
    public void GenerateLayerBottom()
    {
        int W = 300;
        int H = 120;
        float radius = 30f;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] clear = new Color[W * H];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                bool inside = true;

                if (py < radius)
                {
                    float dx = 0f;
                    float dy = radius - py;

                    if (px < radius)
                        dx = radius - px;
                    else if (px > W - radius)
                        dx = px - (W - radius);

                    if (dx > 0f)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        inside = (dist <= radius);
                    }
                }

                if (!inside) continue;

                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        SavePNG(tex, "layer_bottom.png");
    }

    [ContextMenu("Generate Rounded Button PNG")]
    public void GenerateRoundedButton()
    {
        int W = 256;
        int H = 256;
        float radius = 8f;
        Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);

        Color[] clear = new Color[W * H];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float dx = 0f, dy = 0f;
                if (px < radius) dx = radius - px;
                else if (px > W - radius) dx = px - (W - radius);
                if (py < radius) dy = radius - py;
                else if (py > H - radius) dy = py - (H - radius);

                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha;
                if (dist <= radius - 1f) alpha = 1f;
                else if (dist >= radius) alpha = 0f;
                else alpha = 1f - (dist - (radius - 1f));

                if (alpha > 0f)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        SavePNG(tex, "ui_rounded.png");
    }

    // ==================== ГРАДИЕНТЫ ====================

    Color GlassGradient(float t)
    {
        Color edge = new Color(0.82f, 0.84f, 0.86f, 0.25f);
        Color light = new Color(0.96f, 0.97f, 0.98f, 0.35f);
        Color mid = new Color(0.90f, 0.92f, 0.94f, 0.08f);

        if (t < 0.15f) return Color.Lerp(edge, light, t / 0.15f);
        if (t < 0.45f) return Color.Lerp(light, mid, (t - 0.15f) / 0.30f);
        if (t < 0.75f) return Color.Lerp(mid, light, (t - 0.45f) / 0.30f);
        return Color.Lerp(light, edge, (t - 0.75f) / 0.25f);
    }

    Color RimGradient(float t)
    {
        Color dark = new Color(0.70f, 0.73f, 0.76f, 0.55f);
        Color light = new Color(0.88f, 0.90f, 0.92f, 0.75f);
        if (t < 0.5f) return Color.Lerp(dark, light, t * 2f);
        return Color.Lerp(light, dark, (t - 0.5f) * 2f);
    }

    Color CapGradient(float t)
    {
        Color dark = new Color(0.70f, 0.73f, 0.76f, 1f);
        Color light = new Color(0.85f, 0.87f, 0.90f, 1f);
        if (t < 0.5f) return Color.Lerp(dark, light, t * 2f);
        return Color.Lerp(light, dark, (t - 0.5f) * 2f);
    }

    Color CapTopGradient(float t)
    {
        Color dark = new Color(0.66f, 0.69f, 0.72f, 1f);
        Color light = new Color(0.88f, 0.90f, 0.93f, 1f);
        if (t < 0.5f) return Color.Lerp(dark, light, t * 2f);
        return Color.Lerp(light, dark, (t - 0.5f) * 2f);
    }

    // ==================== ВСПОМОГАТЕЛЬНЫЕ ====================

    void BlendPixel(Texture2D tex, int x, int y, Color top)
    {
        if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) return;
        Color bottom = tex.GetPixel(x, y);
        float a = top.a + bottom.a * (1f - top.a);
        if (a < 0.001f) { tex.SetPixel(x, y, new Color(0, 0, 0, 0)); return; }
        Color result = new Color(
            (top.r * top.a + bottom.r * bottom.a * (1f - top.a)) / a,
            (top.g * top.a + bottom.g * bottom.a * (1f - top.a)) / a,
            (top.b * top.a + bottom.b * bottom.a * (1f - top.a)) / a,
            a);
        tex.SetPixel(x, y, result);
    }

    void SavePNG(Texture2D tex, string fileName)
    {
        byte[] png = tex.EncodeToPNG();
        string dir = Application.dataPath + "/Sprites";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = dir + "/" + fileName;
        File.WriteAllBytes(path, png);
        Debug.Log("Сохранено: " + path);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // ==================== SVG-ПАРСЕР И РАСТЕРИЗАТОР ====================

    class SvgPath
    {
        public List<Vector2[]> Subpaths = new List<Vector2[]>();
    }

    SvgPath ParseSvgPath(string d)
    {
        SvgPath path = new SvgPath();
        List<Vector2> current = new List<Vector2>();
        Vector2 cur = Vector2.zero;
        Vector2 start = Vector2.zero;

        int i = 0;
        char cmd = ' ';

        List<float> nums = new List<float>();

        while (i < d.Length)
        {
            char c = d[i];

            if (char.IsLetter(c))
            {
                FlushCommand(path, ref current, ref cur, ref start, cmd, nums);
                nums.Clear();
                cmd = c;
                i++;
                continue;
            }

            if (c == ',' || c == ' ' || c == '\n' || c == '\r' || c == '\t')
            {
                i++;
                continue;
            }

            if (c == '-' || c == '+' || c == '.' || char.IsDigit(c))
            {
                int startIdx = i;
                i++;
                while (i < d.Length && (char.IsDigit(d[i]) || d[i] == '.' || d[i] == '-' || d[i] == '+' || d[i] == 'e' || d[i] == 'E'))
                {
                    if ((d[i] == '-' || d[i] == '+') && i > startIdx) break;
                    if (d[i] == 'e' || d[i] == 'E')
                    {
                        i++;
                        if (i < d.Length && (d[i] == '-' || d[i] == '+')) i++;
                        continue;
                    }
                    i++;
                }
                string numStr = d.Substring(startIdx, i - startIdx);
                float val;
                float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                nums.Add(val);
                continue;
            }

            i++;
        }

        FlushCommand(path, ref current, ref cur, ref start, cmd, nums);

        if (current.Count > 0)
        {
            path.Subpaths.Add(current.ToArray());
        }

        return path;
    }

    void FlushCommand(SvgPath path, ref List<Vector2> current, ref Vector2 cur, ref Vector2 start, char cmd, List<float> nums)
    {
        if (cmd == ' ' || nums.Count == 0) return;

        char upper = char.ToUpper(cmd);
        bool relative = char.IsLower(cmd);

        switch (upper)
        {
            case 'M':
                {
                    int idx = 0;
                    bool first = true;
                    while (idx + 1 < nums.Count)
                    {
                        float x = nums[idx], y = nums[idx + 1];
                        if (relative && !first && current.Count > 0) { x += cur.x; y += cur.y; }
                        Vector2 p = new Vector2(x, y);

                        if (first)
                        {
                            if (current.Count > 0)
                            {
                                path.Subpaths.Add(current.ToArray());
                                current.Clear();
                            }
                            start = p;
                            cur = p;
                            current.Add(p);
                            first = false;
                        }
                        else
                        {
                            cur = p;
                            current.Add(p);
                        }
                        idx += 2;
                    }
                }
                break;

            case 'L':
                {
                    int idx = 0;
                    while (idx + 1 < nums.Count)
                    {
                        float x = nums[idx], y = nums[idx + 1];
                        if (relative) { x += cur.x; y += cur.y; }
                        cur = new Vector2(x, y);
                        current.Add(cur);
                        idx += 2;
                    }
                }
                break;

            case 'H':
                {
                    for (int k = 0; k < nums.Count; k++)
                    {
                        float x = nums[k];
                        if (relative) x += cur.x;
                        cur = new Vector2(x, cur.y);
                        current.Add(cur);
                    }
                }
                break;

            case 'V':
                {
                    for (int k = 0; k < nums.Count; k++)
                    {
                        float y = nums[k];
                        if (relative) y += cur.y;
                        cur = new Vector2(cur.x, y);
                        current.Add(cur);
                    }
                }
                break;

            case 'C':
                {
                    int idx = 0;
                    while (idx + 5 < nums.Count)
                    {
                        float x1 = nums[idx], y1 = nums[idx + 1];
                        float x2 = nums[idx + 2], y2 = nums[idx + 3];
                        float x = nums[idx + 4], y = nums[idx + 5];
                        if (relative)
                        {
                            x1 += cur.x; y1 += cur.y;
                            x2 += cur.x; y2 += cur.y;
                            x += cur.x; y += cur.y;
                        }

                        Vector2 p0 = cur;
                        Vector2 p1 = new Vector2(x1, y1);
                        Vector2 p2 = new Vector2(x2, y2);
                        Vector2 p3 = new Vector2(x, y);

                        int steps = 16;
                        for (int s = 1; s <= steps; s++)
                        {
                            float t = s / (float)steps;
                            Vector2 pt = CubicBezier(p0, p1, p2, p3, t);
                            current.Add(pt);
                        }

                        cur = p3;
                        idx += 6;
                    }
                }
                break;

            case 'Z':
                {
                    if (current.Count > 0)
                    {
                        current.Add(start);
                        path.Subpaths.Add(current.ToArray());
                        current.Clear();
                    }
                    cur = start;
                }
                break;
        }
    }

    Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0;
        p += 3f * uu * t * p1;
        p += 3f * u * tt * p2;
        p += ttt * p3;
        return p;
    }

    Texture2D RasterizeSvgPath(SvgPath path, int size, float viewBoxMin, float viewBoxSize)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] clear = new Color[size * size];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
        tex.SetPixels(clear);

        float scale = size / viewBoxSize;

        List<Vector2[]> segs = new List<Vector2[]>();
        foreach (var sub in path.Subpaths)
        {
            for (int i = 0; i < sub.Length - 1; i++)
            {
                segs.Add(new Vector2[] { sub[i], sub[i + 1] });
            }
        }

        int SS = 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideCount = 0;
                for (int sy = 0; sy < SS; sy++)
                {
                    for (int sx = 0; sx < SS; sx++)
                    {
                        float px = (x + (sx + 0.5f) / SS) / scale + viewBoxMin;
                        float py = (size - 1 - (y + (sy + 0.5f) / SS)) / scale + viewBoxMin;

                        int crossings = 0;
                        foreach (var s in segs)
                        {
                            Vector2 a = s[0];
                            Vector2 b = s[1];
                            if ((a.y > py) != (b.y > py))
                            {
                                float t = (py - a.y) / (b.y - a.y);
                                float xi = a.x + t * (b.x - a.x);
                                if (xi > px) crossings++;
                            }
                        }
                        if (crossings % 2 == 1) insideCount++;
                    }
                }

                float alpha = insideCount / (float)(SS * SS);
                if (alpha > 0f)
                {
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
        }

        tex.Apply();
        return tex;
    }

    // ==================== ИКОНКИ ТУЛБАРА (Material Symbols) ====================

    const string SVG_SETTINGS =
        "M370-80l-16-128q-13-5-24.5-12T307-235l-119 50L78-375l103-78q-1-7-1-13.5v-27q0-6.5 1-13.5L78-585l110-190 119 50q11-8 23-15t24-12l16-128h220l16 128q13 5 24.5 12t22.5 15l119-50 110 190-103 78q1 7 1 13.5v27q0 6.5-1 13.5l103 78-110 190-119-50q-11 8-23 15t-24 12L590-80H370Zm112-260q58 0 99-41t41-99q0-58-41-99t-99-41q-59 0-99.5 41T342-480q0 58 40.5 99t99.5 41Z";

    const string SVG_LEADERBOARD =
        "M180-120q-24 0-42-18t-18-42v-600q0-24 18-42t42-18h180v-60q0-24 18-42t42-18h80q24 0 42 18t18 42v60h180q24 0 42 18t18 42v600q0 24-18 42t-42 18H180Zm240-680h120v-40H420v40Zm0 340q0-40 27-70t73-30q46 0 73 30t27 70h80v-200H420v200Zm-100 200h560v-160H580q-14 20-34 30t-46 10q-26 0-46-10t-34-30H320v160Z";

    const string SVG_UNDO =
        "M280-200v-80h284q63 0 109.5-40T720-420q0-60-46.5-100T564-560H312l104 104-56 56-200-200 200-200 56 56-104 104h252q97 0 166.5 63T800-420q0 94-69.5 157T564-200H280Z";

    const string SVG_HINT =
        "M480-80q-33 0-56.5-23.5T400-160h160q0 33-23.5 56.5T480-80Zm-80-120v-80h160v80H400Zm-84-120q-71-50-113.5-122T160-600q0-133 93.5-226.5T480-920q133 0 226.5 93.5T800-600q0 86-42.5 158T644-320H316Z";

    const string SVG_REFRESH =
        "M480-160q-134 0-227-93t-93-227q0-134 93-227t227-93q69 0 132 28.5T720-690v-110h80v280H520v-80h168q-32-56-87.5-88T480-720q-100 0-170 70t-70 170q0 100 70 170t170 70q77 0 139-44t87-116h84q-28 106-114 173t-196 67Z";

    [ContextMenu("Generate Toolbar Icons")]
    public void GenerateToolbarIcons()
    {
        GenerateIconFromSvg(SVG_SETTINGS, "icon_settings.png");
        GenerateIconFromSvg(SVG_LEADERBOARD, "icon_leaderboard.png");
        GenerateIconFromSvg(SVG_UNDO, "icon_undo.png");
        GenerateIconFromSvg(SVG_HINT, "icon_hint.png");
        GenerateIconFromSvg(SVG_REFRESH, "icon_refresh.png");
    }

    void GenerateIconFromSvg(string svgPathData, string fileName)
    {
        SvgPath path = ParseSvgPath(svgPathData);

        foreach (var sub in path.Subpaths)
        {
            for (int i = 0; i < sub.Length; i++)
            {
                sub[i] = new Vector2(sub[i].x + 480f, sub[i].y + 960f);
            }
        }

        int size = 128;
        Texture2D tex = RasterizeSvgPath(path, size, 0f, 960f);
        SavePNG(tex, fileName);
    }
}