using Spot.Core;

namespace Spot.Tests;

// Kriterler Hüseyin'in tarifinden: "chr yazıp Enter'a bastığımda Chrome açılsın",
// iPhone'daki gibi "spot" yazınca kutuda "spotify — Aç" görünsün.
public class EslestiriciTestleri
{
    static readonly Uygulama[] Liste =
    [
        new("Google Chrome", "Chrome"),
        new("Chrome Remote Desktop", "crd"),
        new("Character Map", "charmap"),
        new("Spotify", "Spotify"),
        new("Visual Studio Code", "vscode"),
        new("Hesap Makinesi", "calc"),
        new("Ayarlar", "settings"),
        new("WhatsApp", "whatsapp"),
        new("Steam", "steam"),
        new("Microsoft Store", "store"),
    ];

    static Eslesme Ilk(string sorgu, Func<string, int>? kullanim = null) =>
        Eslestirici.Ara(sorgu, Liste, kullanim)[0];

    [Fact]
    public void Chr_Chromeu_bulur_ve_kelimeyi_tamamlar()
    {
        var e = Ilk("chr");
        Assert.Equal("Google Chrome", e.Uygulama.Ad);
        Assert.Equal("ome", e.Tamamlama);
    }

    [Fact]
    public void Spot_Spotifyi_bulur_ve_adi_tamamlar()
    {
        var e = Ilk("spot");
        Assert.Equal("Spotify", e.Uygulama.Ad);
        Assert.Equal(EslesmeTuru.AdBasi, e.Tur);
        Assert.Equal("ify", e.Tamamlama);
    }

    [Fact]
    public void Bas_harfler_eslesir() => Assert.Equal("Visual Studio Code", Ilk("vsc").Uygulama.Ad);

    [Fact]
    public void Camel_case_kelime_basi_sayilir() => Assert.Equal("WhatsApp", Ilk("app").Uygulama.Ad);

    [Fact]
    public void Buyuk_harf_ve_turkce_harf_fark_etmez()
    {
        Assert.Equal("Hesap Makinesi", Ilk("MAKİN").Uygulama.Ad);
        Assert.Equal("Ayarlar", Ilk("AYAR").Uygulama.Ad);
    }

    [Fact]
    public void Sik_acilan_ayni_kademede_one_gecer()
    {
        Assert.Equal("Steam", Ilk("st").Uygulama.Ad); // kısa ad kazanır
        var e = Ilk("st", k => k == "store" ? 5 : 0);
        Assert.Equal("Microsoft Store", e.Uygulama.Ad);
    }

    [Fact]
    public void Kelime_basi_icinde_gecmeyi_yener()
    {
        // "re": Chrome [Re]mote kelime başı, Sto[re] ve Chrome içinde geçme.
        var sonuc = Eslestirici.Ara("re", Liste);
        Assert.Equal("Chrome Remote Desktop", sonuc[0].Uygulama.Ad);
        Assert.Contains(sonuc, e => e.Uygulama.Ad == "Microsoft Store" && e.Tur == EslesmeTuru.IcindeGecme);
    }

    [Fact]
    public void Harf_sirasi_daha_iyi_eslesme_varken_gosterilmez()
    {
        var liste = Liste.Append(new Uygulama("Epic Games Launcher", "epic")).ToArray();
        Assert.DoesNotContain(Eslestirici.Ara("chr", liste), e => e.Uygulama.Ad == "Epic Games Launcher");
        // Başka eşleşme yoksa gevşek eşleşme yine bulunur.
        Assert.Equal("Epic Games Launcher", Eslestirici.Ara("eplau", liste)[0].Uygulama.Ad);
    }

    [Fact]
    public void Emojili_ad_ve_sorgu_cokertmez()
    {
        var liste = Liste.Append(new Uygulama("Oyun 🎮 Merkezi", "oyun")).ToArray();
        Assert.Equal("Oyun 🎮 Merkezi", Eslestirici.Ara("oyun", liste)[0].Uygulama.Ad);
        Assert.Empty(Eslestirici.Ara("🎮x", liste));
    }

    // Hüseyin: Ekran Görüntüleri klasörünü sık kullanıyor, uygulamalarla karışmasın.
    // Aynı kademede hangisinin öne çıkacağına kullanım karar verir.
    [Fact]
    public void Klasor_ile_uygulama_kullanima_gore_siralanir()
    {
        var liste = Liste.Concat(new Uygulama[]
        {
            new("Ekran Alıntısı Aracı", "snip"),
            new("Ekran Görüntüleri", @"C:\Users\h\Pictures\Screenshots", Klasor: true),
            new("İndirilenler", @"C:\Users\h\Downloads", Klasor: true),
        }).ToArray();

        var e = Eslestirici.Ara("ekran", liste, k => k.EndsWith("Screenshots") ? 4 : 1)[0];
        Assert.True(e.Uygulama.Klasor);
        Assert.Equal("Ekran Görüntüleri", e.Uygulama.Ad);
        Assert.Equal("Ekran Alıntısı Aracı", Eslestirici.Ara("ekran", liste, k => k == "snip" ? 4 : 1)[0].Uygulama.Ad);
        Assert.Equal("İndirilenler", Eslestirici.Ara("indir", liste)[0].Uygulama.Ad);
    }

    [Fact]
    public void Bos_sorgu_sonuc_vermez() => Assert.Empty(Eslestirici.Ara("  ", Liste));

    [Fact]
    public void Alakasiz_sorgu_sonuc_vermez() => Assert.Empty(Eslestirici.Ara("xyzq", Liste));
}
