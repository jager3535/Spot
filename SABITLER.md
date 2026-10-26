# Sabitler

| Ad | Değer | Kodda |
|---|---|---|
| Kısayol | Alt + `` ` `` (`MOD_ALT`, `VK_OEM_3` = 0xC0) | `KisayolDinleyici` |
| Panel genişliği | 680 DIP, köşe 20 (satır 10 + 10 boşluk), arama satırı 60 | `MainWindow.Genislik`, `MainWindow.xaml` |
| Gösterilen en fazla sonuç | 1 en iyi + 6 diğer, ekrana sığdığı kadar (alt boşluk 32 DIP) | `MainWindow.EnFazlaSonuc`, `AltBosluk` |
| Aranan klasörler | Ekran Görüntüleri (FOLDERID_Screenshots), İndirilenler (FOLDERID_Downloads) | `UygulamaDizini.SabitKlasorler` |
| Kullanım verisi | `%LOCALAPPDATA%\Spot\kullanim.json` | `App` |
| Kurulum klasörü | `%LOCALAPPDATA%\Spot\app\` | publish hedefi |
| Otomatik başlatma | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` → `Spot` | elle, aşağıda |
| Windows App SDK | WinUI 2.3.9, Foundation 2.3.12, InteractiveExperiences 2.1.9 | `Spot.csproj` |

## Kurulum

```
dotnet publish src\Spot -c Release -o "%LOCALAPPDATA%\Spot\app"
```

Run anahtarına `"%LOCALAPPDATA%\Spot\app\Spot.exe"` yazılır. Güncellemeden önce çalışan
Spot kapatılır (yalnız kendi PID'i ile).


