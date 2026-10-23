using System.Globalization;
using System.Text;

namespace Spot.Core;

public enum EslesmeTuru
{
    Yok = 0,
    HarfSirasi = 1,   // "gchr" → G...Chr... (harfler sırayla geçiyor)
    IcindeGecme = 2,  // "rom" → ch[rom]e
    BasHarfler = 3,   // "vsc" → Visual Studio Code
    KelimeBasi = 4,   // "chr" → Google [Chr]ome
    AdBasi = 5,       // "spot" → [Spot]ify
}

/// <summary>Bir uygulamanın sorguyla eşleşme sonucu. Tamamlama, kutuda soluk görünen kısımdır.</summary>
public sealed record Eslesme(Uygulama Uygulama, EslesmeTuru Tur, int Puan, string Tamamlama);

public static class Eslestirici
{
    /// <summary>Sorguya uyan uygulamaları en iyiden kötüye sıralar. Önce eşleşme türü,
    /// sonra kullanım sayısı, sonra kısa ad kazanır.</summary>
    public static List<Eslesme> Ara(string sorgu, IEnumerable<Uygulama> uygulamalar,
        Func<string, int>? kullanim = null)
    {
        var s = Sadelestir(sorgu.Trim());
        if (s.Length == 0) return [];

        var sonuc = new List<Eslesme>();
        foreach (var u in uygulamalar)
        {
            var (tur, tamamlama) = Olc(s, u.Ad);
            if (tur == EslesmeTuru.Yok) continue;
            var sayi = kullanim?.Invoke(u.Kimlik) ?? 0;
            // Ad başı ile kelime başı aynı kademe: "chr" yazınca "Google Chrome",
            // "Chrome Remote Desktop"a sırf adı Chrome'la başlıyor diye yenilmesin.
            var kademe = tur == EslesmeTuru.AdBasi ? EslesmeTuru.KelimeBasi : tur;
            // Kademe baskın; kademe içinde önce kullanım, sonra kısa ad kazanır.
            var puan = (int)kademe * 1_000_000 + Math.Min(sayi, 999) * 100 - Math.Min(u.Ad.Length, 99);
            sonuc.Add(new Eslesme(u, tur, puan, tamamlama));
        }

        // Harf sırası en gevşek eşleşme ("chr" → Epic Games Launcher): yalnız başka sonuç yoksa göster.
        if (sonuc.Any(e => e.Tur > EslesmeTuru.HarfSirasi))
            sonuc.RemoveAll(e => e.Tur == EslesmeTuru.HarfSirasi);

        sonuc.Sort((a, b) => b.Puan != a.Puan
            ? b.Puan.CompareTo(a.Puan)
            : string.Compare(a.Uygulama.Ad, b.Uygulama.Ad, StringComparison.CurrentCultureIgnoreCase));
        return sonuc;
    }

    /// <summary>Sadeleştirilmiş sorgu (s) ile uygulama adını karşılaştırır.</summary>
    static (EslesmeTuru, string) Olc(string s, string ad)
    {
        // Sadeleştirme harf sayısını korur; indeksler orijinal ad üzerinde de geçerli.
        var a = Sadelestir(ad);

        if (a.StartsWith(s, StringComparison.Ordinal))
            return (EslesmeTuru.AdBasi, ad[s.Length..]);

        var kelimeBaslari = KelimeBaslari(ad);
        foreach (var i in kelimeBaslari)
        {
            if (i == 0) continue;
            if (i + s.Length <= a.Length && string.CompareOrdinal(a, i, s, 0, s.Length) == 0)
            {
                var son = ad.IndexOf(' ', i);
                var kelimeSonu = son < 0 ? ad.Length : son;
                return (EslesmeTuru.KelimeBasi, ad[(i + s.Length)..Math.Max(i + s.Length, kelimeSonu)]);
            }
        }

        if (s.Length >= 2)
        {
            var bas = new string(kelimeBaslari.Select(i => a[i]).ToArray());
            if (bas.StartsWith(s, StringComparison.Ordinal))
                return (EslesmeTuru.BasHarfler, "");
        }

        if (a.Contains(s, StringComparison.Ordinal))
            return (EslesmeTuru.IcindeGecme, "");

        if (s.Length >= 2 && HarfSirasiyla(s, a))
            return (EslesmeTuru.HarfSirasi, "");

        return (EslesmeTuru.Yok, "");
    }

    /// <summary>Kelime başlarının indeksleri: boşluk/tire/nokta sonrası ve CamelCase geçişleri.</summary>
    static List<int> KelimeBaslari(string ad)
    {
        var liste = new List<int>();
        for (var i = 0; i < ad.Length; i++)
        {
            if (!char.IsLetterOrDigit(ad[i])) continue;
            if (i == 0 || !char.IsLetterOrDigit(ad[i - 1])
                || (char.IsUpper(ad[i]) && char.IsLower(ad[i - 1])))
                liste.Add(i);
        }
        return liste;
    }

    static bool HarfSirasiyla(string s, string a)
    {
        var j = 0;
        foreach (var c in a)
            if (j < s.Length && c == s[j]) j++;
        return j == s.Length;
    }

    /// <summary>Küçük harfe çevirir, Türkçe ve aksanlı harfleri ASCII karşılığına indirir.
    /// Harf sayısı korunur.</summary>
    public static string Sadelestir(string metin)
    {
        var sb = new StringBuilder(metin.Length);
        foreach (var c in metin)
        {
            var k = c switch
            {
                'ı' or 'I' or 'İ' => 'i',
                'ğ' or 'Ğ' => 'g',
                'ş' or 'Ş' => 's',
                'ç' or 'Ç' => 'c',
                'ö' or 'Ö' => 'o',
                'ü' or 'Ü' => 'u',
                _ => char.ToLowerInvariant(c),
            };
            // Emoji gibi vekil çiftlerin yarısı Normalize'da patlar; olduğu gibi bırak.
            if (k > 127 && !char.IsSurrogate(k))
            {
                var d = k.ToString().Normalize(NormalizationForm.FormD);
                k = d.Length > 0 && CharUnicodeInfo.GetUnicodeCategory(d[0]) != UnicodeCategory.NonSpacingMark
                    ? d[0] : k;
            }
            sb.Append(k);
        }
        return sb.ToString();
    }
}
