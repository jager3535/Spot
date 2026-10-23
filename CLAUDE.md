# Spot

iPhone Spotlight benzeri uygulama başlatıcı. Alt+` → ekranın ortasında arama çubuğu → `chr`
yaz, Enter → Chrome açılır. C# / WinUI 3 (Windows App SDK) / .NET 10. Tek PC, tek kullanıcı.
Sahibi: Hüseyin Albayrak.

## Yükleme sırası

1. `notes.md` — ürün ne, **ilk sürüme ne girmeyecek**.
2. `SABITLER.md` — kısayol, pencere ölçüleri, dosya yolları. Tahmin etme, buradan oku.
3. `backlog.md` — yarım kalan işler.

## Kurallar

- **Mimari önce, kod sonra.**
- **Kapsam dışı listesine dokunulmaz.** Hüseyin yalnız uygulama açılmasını istedi: hızlı
  eylem, web araması, dosya araması yok. Yeni özellik önerilmeden `notes.md`'ye bakılır.
- **`Eslestirici` test edilir.** "chr → Chrome" gibi kriterler Hüseyin'in sözlerinden gelir;
  `tests/Spot.Tests` bunları tutar. "Geçsin diye" test yazılmaz.
- **Ek paket kurulmaz.** Windows App SDK bileşenleri + test projesinde xunit; başka yok.
- **MVVM framework'ü yok.** Code-behind; mantık `Services/` ve `Spot.Core` altında.
- Yarım kalan iş **o anda** `backlog.md`'ye yazılır. Ham araştırma `reports/` altına.
- `CLAUDE.md` ile `AGENTS.md` birlikte yaşar; biri değişince diğeri güncellenir.

## Yapı

| Yer | İçerik |
|---|---|
| `src/Spot.Core/` | Saf mantık, UI yok: `Uygulama`, `Eslestirici`, `KullanimSayaci` |
| `src/Spot/` | WinUI 3 uygulaması: `MainWindow`, `Services/` |
| `src/Spot/Services/UygulamaDizini` | `shell:AppsFolder`'ı tarar (Başlat menüsündeki her şey, Store dahil), ikon üretir |
| `src/Spot/Services/KisayolDinleyici` | `RegisterHotKey` + pencere subclass ile Alt+` |
| `src/Spot/Services/Yerel` | P/Invoke ve COM tanımları |
| `tests/Spot.Tests/` | `Eslestirici` testleri |

Çalıştır: `dotnet run --project src\Spot`. Kur: `SABITLER.md` → "Kurulum".

## Vault bağlantısı

Karar ve takip notları `HuseyinOS/🏰 300-Projects/spot/` altında. Vault'a kod girmez.
