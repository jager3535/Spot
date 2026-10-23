using System.Text.Json;

namespace Spot.Core;

/// <summary>Hangi uygulamanın kaç kez açıldığını JSON dosyasında tutar.</summary>
public sealed class KullanimSayaci(string dosyaYolu)
{
    readonly Dictionary<string, int> _sayilar = Yukle(dosyaYolu);

    public int Sayi(string kimlik) => _sayilar.GetValueOrDefault(kimlik);

    public void Artir(string kimlik)
    {
        _sayilar[kimlik] = Sayi(kimlik) + 1;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dosyaYolu)!);
            File.WriteAllText(dosyaYolu, JsonSerializer.Serialize(_sayilar));
        }
        catch (IOException) { } // Sayaç yazılamazsa başlatma yine çalışır.
    }

    static Dictionary<string, int> Yukle(string yol)
    {
        try
        {
            return File.Exists(yol)
                ? JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(yol)) ?? []
                : [];
        }
        catch (Exception e) when (e is IOException or JsonException) { return []; }
    }
}
