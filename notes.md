# Spot — notlar

## Ürün (23.10.2026, Hüseyin'le planlandı)

Referans: iPhone Spotlight ekran görüntüsü (`spot` yazınca kutuda soluk `spotify — Aç`).

- **Alt+`** (Esc'nin altındaki tuş, ENG klavye) → ekranın **tam ortasında** buzlu cam panel.
- Yazdıkça **satır içi soluk tamamlama**: `chr` → `chr`ome — Google Chrome.
- **En İyi Eşleşen**: büyük ikonlu tek kazanan; altında diğer eşleşen uygulamalar.
- `Enter` seçili uygulamayı açar, `↑/↓` seçimi değiştirir, `Esc` veya odak kaybı paneli gizler.
- Sıralama: tam ad başı > kelime başı > baş harfler (`vsc`) > içinde geçme > harf sırası;
  sık açılan uygulama aynı kademede üste çıkar.
- Uygulama listesi `shell:AppsFolder`'dan gelir: Başlat menüsündeki masaüstü + Store uygulamaları.
- Bilgisayar açılınca arka planda sessizce başlar; tek örnek çalışır.
- **Buzlu cam ikisi birden** (23.10, Hüseyin seçti): tüm ekran bulanık + kararmış perde, panel
  kendisi de ayrı, daha açık bir cam. Perdeye tıklamak Spot'u kapatır.

## İlk sürüme girmeyecek (bilinçli)

- Hızlı eylemler (gizli pencere, yeni sekme vb.) — Hüseyin istemedi.
- Web araması, genel dosya araması, hesap makinesi, ayar araması.
  - İstisna (23.10, Hüseyin'in kararı): yalnız **Ekran Görüntüleri** ve **İndirilenler** klasörleri
    aramada çıkar, ayrı "Klasörler" başlığıyla; eşleşirlerse yerleri ayrılır. Takma ad ve tam dosya
    araması (ör. `/` önekiyle) önerildi, istenmedi.
- Ayar ekranı. Kısayol `SABITLER.md`'de, değiştirmek kod değişikliği.
- Tepsi (tray) ikonu.

## Teknoloji kararı

WinUI 3 seçildi (Forms'tan sıkıldı; Acrylic ve Win11 görünümü yerleşik). Windows App SDK
self-contained, unpackaged: sistemde ayrı runtime kurulmaz.
