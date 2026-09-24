using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class CountryData
{
    public static readonly List<(string code, string name)> Countries = new List<(string, string)>
    {
        ("RU", "Россия"),
        ("BY", "Беларусь"),
        ("KZ", "Казахстан"),
        ("UA", "Украина"),
        ("US", "США"),
        ("GB", "Великобритания"),
        ("DE", "Германия"),
        ("FR", "Франция"),
        ("IT", "Италия"),
        ("ES", "Испания"),
        ("PL", "Польша"),
        ("TR", "Турция"),
        ("CN", "Китай"),
        ("JP", "Япония"),
        ("KR", "Южная Корея"),
        ("IN", "Индия"),
        ("BR", "Бразилия"),
        ("CA", "Канада"),
        ("AU", "Австралия"),
        ("NL", "Нидерланды"),
        ("SE", "Швеция"),
        ("NO", "Норвегия"),
        ("FI", "Финляндия"),
        ("CZ", "Чехия"),
        ("CH", "Швейцария"),
        ("AT", "Австрия"),
        ("PT", "Португалия"),
        ("GR", "Греция"),
        ("IL", "Израиль"),
        ("AE", "ОАЭ"),
        ("EG", "Египет"),
        ("ZA", "ЮАР"),
        ("MX", "Мексика"),
        ("AR", "Аргентина"),
        ("CL", "Чили"),
        ("CO", "Колумбия"),
        ("PE", "Перу"),
        ("VN", "Вьетнам"),
        ("TH", "Таиланд"),
        ("ID", "Индонезия"),
        ("MY", "Малайзия"),
        ("PH", "Филиппины"),
        ("SG", "Сингапур"),
        ("NZ", "Новая Зеландия"),
        ("IE", "Ирландия"),
        ("DK", "Дания"),
        ("BE", "Бельгия"),
        ("HU", "Венгрия"),
        ("RO", "Румыния"),
        ("BG", "Болгария"),
        ("RS", "Сербия"),
        ("HR", "Хорватия"),
        ("SK", "Словакия"),
        ("SI", "Словения"),
        ("LT", "Литва"),
        ("LV", "Латвия"),
        ("EE", "Эстония"),
        ("GE", "Грузия"),
        ("AM", "Армения"),
        ("AZ", "Азербайджан"),
        ("UZ", "Узбекистан"),
        ("KG", "Кыргызстан"),
        ("TJ", "Таджикистан"),
        ("MD", "Молдова"),
        ("MN", "Монголия"),
    };

    /// <summary>
    /// PNG-спрайт флага из Assets/Resources/Flags/.
    /// Имена файлов — lowercase ISO-код: ru.png, us.png, de.png.
    /// Если файла нет — возвращает null (вызывающий код показывает текстовый код).
    /// </summary>
    public static Sprite GetFlagSprite(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 2) return null;
        return Resources.Load<Sprite>("Flags/" + code.ToLower());
    }

    /// <summary>
    /// Эмодзи-флаг — оставлен для fallback-случая, но на Android может не отображаться.
    /// </summary>
    public static string GetFlag(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 2) return "🏳";
        code = code.ToUpper();
        return string.Concat(code.Select(c => char.ConvertFromUtf32(c + 0x1F1A5)));
    }

    /// <summary>
    /// Название страны. Если кода нет в списке — "Неизвестная страна (CH)".
    /// </summary>
    public static string GetName(string code)
    {
        if (string.IsNullOrEmpty(code)) return "Неизвестно";
        var found = Countries.FirstOrDefault(c => c.code == code.ToUpper());
        if (!string.IsNullOrEmpty(found.name)) return found.name;
        return $"Неизвестная страна ({code.ToUpper()})";
    }

    /// <summary>
    /// Страна по умолчанию из региона устройства.
    /// </summary>
    public static string GetDefaultCountryCode()
    {
        try
        {
            string region = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            if (!string.IsNullOrEmpty(region) && region.Length == 2)
                return region.ToUpper();
        }
        catch { }
        return "RU";
    }

    /// <summary>
    /// Флаг + название. Используется в текстовых местах, где нельзя вставить Image.
    /// </summary>
    public static string GetFlagWithName(string code)
    {
        return $"{GetFlag(code)}  {GetName(code)}";
    }

    /// <summary>
    /// Проверяет, что код есть в списке стран.
    /// </summary>
    public static bool IsKnownCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 2) return false;
        code = code.ToUpper();
        return Countries.Any(c => c.code == code);
    }
}