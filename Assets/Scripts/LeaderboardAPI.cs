using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LeaderboardRecord
{
    public string id;
    public string device_id;
    public string player_name;
    public int score;
    public string country_code;
    public string created;
}

[System.Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardRecord> items;
    public int page;
    public int perPage;
    public int totalItems;
}

public static class LeaderboardAPI
{
    private const string BASE_URL = "https://plaintively-objective-vervet.cloudpub.ru";
    private const string COLLECTION = "leaderboard";
    private const int TIMEOUT_SECONDS = 10;

    /// <summary>
    /// Upsert по device_id:
    /// - если запись есть и score >= нового — пропускаем;
    /// - если запись есть и score < нового — PATCH (score, name, country);
    /// - если записи нет — POST.
    /// </summary>
    public static IEnumerator SubmitScore(
        string deviceId,
        string playerName,
        int score,
        string countryCode,
        Action<bool> onComplete = null)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            Debug.LogWarning("[Leaderboard] deviceId пуст, отправка отменена");
            onComplete?.Invoke(false);
            yield break;
        }

        // 1. Ищем запись по device_id
        string filter = UnityWebRequest.EscapeURL($"device_id=\"{deviceId}\"");
        string findUrl = $"{BASE_URL}/api/collections/{COLLECTION}/records?filter=({filter})&perPage=1";

        LeaderboardRecord existing = null;
        using (UnityWebRequest req = UnityWebRequest.Get(findUrl))
        {
            req.timeout = TIMEOUT_SECONDS;
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var list = ParseRecords(req.downloadHandler.text);
                if (list != null && list.Count > 0) existing = list[0];
            }
        }

        // 2. Если запись есть и score >= нового — ничего не делаем
        if (existing != null && existing.score >= score)
        {
            Debug.Log($"[Leaderboard] {playerName}: score {existing.score} >= {score}, пропускаем.");
            onComplete?.Invoke(true);
            yield break;
        }

        // 3. PATCH — обновляем score, name, country
        if (existing != null)
        {
            string patchUrl = $"{BASE_URL}/api/collections/{COLLECTION}/records/{existing.id}";
            string patchJson = $"{{\"score\":{score},\"player_name\":\"{EscapeJson(playerName)}\",\"country_code\":\"{EscapeJson(countryCode)}\"}}";

            using (UnityWebRequest req = new UnityWebRequest(patchUrl, "PATCH"))
            {
                byte[] body = System.Text.Encoding.UTF8.GetBytes(patchJson);
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = TIMEOUT_SECONDS;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[Leaderboard] Обновлено: {playerName} {existing.score} -> {score}");
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogWarning($"[Leaderboard] Ошибка PATCH: {req.error}");
                    onComplete?.Invoke(false);
                }
            }
            yield break;
        }

        // 4. POST — создаём новую запись
        string postUrl = $"{BASE_URL}/api/collections/{COLLECTION}/records";
        string postJson = $"{{\"device_id\":\"{EscapeJson(deviceId)}\",\"player_name\":\"{EscapeJson(playerName)}\",\"score\":{score},\"country_code\":\"{EscapeJson(countryCode)}\"}}";

        using (UnityWebRequest req = new UnityWebRequest(postUrl, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(postJson);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = TIMEOUT_SECONDS;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Leaderboard] Создано: {playerName} = {score}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"[Leaderboard] Ошибка POST: {req.error}");
                onComplete?.Invoke(false);
            }
        }
    }

    public static IEnumerator GetTopScores(int count, Action<List<LeaderboardRecord>> onComplete)
    {
        string url = $"{BASE_URL}/api/collections/{COLLECTION}/records?sort=-score&perPage={count}";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = TIMEOUT_SECONDS;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                List<LeaderboardRecord> list = ParseRecords(req.downloadHandler.text);
                onComplete?.Invoke(list);
            }
            else
            {
                Debug.LogWarning($"[Leaderboard] Ошибка загрузки: {req.error}");
                onComplete?.Invoke(new List<LeaderboardRecord>());
            }
        }
    }

    private static List<LeaderboardRecord> ParseRecords(string rawJson)
    {
        if (string.IsNullOrEmpty(rawJson)) return new List<LeaderboardRecord>();

        int itemsKey = rawJson.IndexOf("\"items\"");
        if (itemsKey < 0) return new List<LeaderboardRecord>();

        int arrStart = rawJson.IndexOf('[', itemsKey);
        if (arrStart < 0) return new List<LeaderboardRecord>();

        int depth = 0;
        int arrEnd = -1;
        for (int i = arrStart; i < rawJson.Length; i++)
        {
            if (rawJson[i] == '[') depth++;
            else if (rawJson[i] == ']')
            {
                depth--;
                if (depth == 0) { arrEnd = i; break; }
            }
        }
        if (arrEnd < 0) return new List<LeaderboardRecord>();

        string itemsJson = rawJson.Substring(arrStart, arrEnd - arrStart + 1);
        string wrapped = "{\"items\":" + itemsJson + "}";

        try
        {
            LeaderboardResponse resp = JsonUtility.FromJson<LeaderboardResponse>(wrapped);
            return resp?.items ?? new List<LeaderboardRecord>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Leaderboard] Ошибка парсинга: {e.Message}");
            return new List<LeaderboardRecord>();
        }
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}