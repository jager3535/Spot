# Sabitler

| Ad | Değer | Kodda |
|---|---|---|
| Kısayol | Alt + `` ` `` (`MOD_ALT`, `VK_OEM_3` = 0xC0) | `KisayolDinleyici` |
| Panel genişliği | 680 DIP | `MainWindow.Genislik` |
| Gösterilen en fazla sonuç | 1 en iyi + 6 diğer | `MainWindow.EnFazlaSonuc` |
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
