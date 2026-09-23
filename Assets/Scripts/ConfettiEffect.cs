using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Разовый салют из прямоугольных конфетти.
/// Создаёт временные GameObject со SpriteRenderer, анимирует их
/// в одной корутине и уничтожает по завершении.
///
/// Работает через SpriteRenderer — не зависит от URP/Built-in шейдеров
/// ParticleSystem. Использует тот же layerSprite, что и колбы.
/// </summary>
public static class ConfettiEffect
{
    // ---- Параметры салюта ----
    public const int COUNT = 140;
    public const float DURATION = 2.2f;

    // Первые BURST_DURATION сек конфетти разлетаются веером,
    // потом включается гравитация и они падают.
    public const float BURST_DURATION = 0.4f;

    public const float BURST_SPEED_MIN = 3.0f;
    public const float BURST_SPEED_MAX = 7.0f;

    public const float GRAVITY = -4.0f;

    public const float SPIN_MIN = -400f;
    public const float SPIN_MAX = 400f;

    public const float WIDTH_MIN = 0.10f;
    public const float WIDTH_MAX = 0.16f;
    public const float HEIGHT_MIN = 0.14f;
    public const float HEIGHT_MAX = 0.22f;

    /// <summary>
    /// Запустить салют.
    /// </summary>
    /// <param name="origin">Мировая точка центра взрыва.</param>
    /// <param name="sprite">Спрайт конфетки (layerSprite — белый квадрат).</param>
    /// <param name="host">MonoBehaviour, на котором висит корутина.</param>
    /// <param name="sortingOrder">Sorting order, по умолчанию 25 (поверх колб, под UI).</param>
    public static void Play(Vector3 origin, Sprite sprite, MonoBehaviour host, int sortingOrder = 25)
    {
        if (sprite == null || host == null) return;
        host.StartCoroutine(Run(origin, sprite, sortingOrder));
    }

    static IEnumerator Run(Vector3 origin, Sprite sprite, int sortingOrder)
    {
        // Палитра: 8 базовых контрастных + 3 праздничных.
        List<Color> palette = new List<Color>();
        palette.AddRange(ColorPalette.GetContrastingColors(8));
        palette.Add(new Color(1.00f, 0.85f, 0.20f)); // золотой
        palette.Add(new Color(0.95f, 0.35f, 0.65f)); // розовый
        palette.Add(new Color(0.30f, 0.85f, 0.85f)); // бирюзовый

        // Объект-контейнер, чтобы всё разом удалить в конце.
        GameObject root = new GameObject("ConfettiRoot");
        root.transform.position = origin;

        List<Transform> transforms = new List<Transform>(COUNT);
        List<SpriteRenderer> renderers = new List<SpriteRenderer>(COUNT);
        List<Vector2> velocities = new List<Vector2>(COUNT);
        List<float> spins = new List<float>(COUNT);

        // Размеры спрайта в мировых единицах (для скейла).
        float spriteW = sprite.bounds.size.x;
        float spriteH = sprite.bounds.size.y;

        for (int i = 0; i < COUNT; i++)
        {
            GameObject obj = new GameObject("C_" + i);
            obj.transform.SetParent(root.transform, false);
            obj.transform.position = origin;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = palette[Random.Range(0, palette.Count)];
            sr.sortingOrder = sortingOrder;

            float w = Random.Range(WIDTH_MIN, WIDTH_MAX);
            float h = Random.Range(HEIGHT_MIN, HEIGHT_MAX);
            obj.transform.localScale = new Vector3(w / spriteW, h / spriteH, 1f);

            // Направление разлёта: веер вверх ±80° от вертикали.
            float angle = Random.Range(-80f, 80f) * Mathf.Deg2Rad;
            float speed = Random.Range(BURST_SPEED_MIN, BURST_SPEED_MAX);
            Vector2 v = new Vector2(Mathf.Sin(angle) * speed, Mathf.Cos(angle) * speed);
            velocities.Add(v);

            spins.Add(Random.Range(SPIN_MIN, SPIN_MAX));

            transforms.Add(obj.transform);
            renderers.Add(sr);
        }

        float elapsed = 0f;

        while (elapsed < DURATION)
        {
            elapsed += Time.deltaTime;
            float dt = Time.deltaTime;

            // Фаза разлёта: скорость умножается на плавно затухающий фактор,
            // гравитация ещё не действует. После BURST_DURATION — обычное падение.
            float burstT = Mathf.Clamp01(elapsed / BURST_DURATION);
            float burstDamp = 1f - burstT; // линейное затухание толчка

            for (int i = 0; i < COUNT; i++)
            {
                Transform tr = transforms[i];
                if (tr == null) continue;

                Vector2 v = velocities[i];
                Vector3 pos = tr.position;

                if (elapsed < BURST_DURATION)
                {
                    // Толчок затухает
                    pos += new Vector3(v.x, v.y, 0f) * burstDamp * dt;
                }
                else
                {
                    // Падение с гравитацией
                    v.y += GRAVITY * dt;
                    pos += new Vector3(v.x, v.y, 0f) * dt;

                    // Горизонтальное затухание (трение воздуха)
                    v.x *= 1f - 0.6f * dt;
                    velocities[i] = v;
                }

                pos.z = 0f;
                tr.position = pos;

                // Вращение
                tr.Rotate(0f, 0f, spins[i] * dt);

                // Fade-out: плавно с 60% длительности
                float life = elapsed / DURATION;
                float alpha = life < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (life - 0.6f) / 0.4f);
                Color c = renderers[i].color;
                c.a = alpha;
                renderers[i].color = c;
            }

            yield return null;
        }

        Object.Destroy(root);
    }
}