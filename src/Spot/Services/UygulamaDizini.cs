using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Spot.Core;

namespace Spot.Services;

/// <summary>shell:AppsFolder'daki uygulamaları (Başlat menüsünün gördüğü her şey) listeler,
/// ikonlarını üretir ve başlatır. Shell COM çağrıları tek bir STA iş parçacığında yapılır.</summary>
sealed class UygulamaDizini
{
    const string AppsFolder = "shell:AppsFolder";
    static readonly string[] AtlananUzantilar = [".txt", ".chm", ".url", ".pdf", ".htm", ".html", ".ini", ".log", ".rtf"];
    static readonly string[] AtlananKelimeler = ["uninstall", "kaldır", "readme", "release notes"];

    readonly BlockingCollection<Action> _kuyruk = [];
    readonly Dictionary<string, ImageSource> _ikonlar = [];
    DateTime _sonTarama = DateTime.MinValue;
    Task? _tarama;

    public IReadOnlyList<Uygulama> Liste { get; private set; } = [];
    public event Action? Yenilendi;

    public UygulamaDizini()
    {
        var isci = new Thread(() => { foreach (var is_ in _kuyruk.GetConsumingEnumerable()) is_(); })
        { IsBackground = true, Name = "Spot.Kabuk" };
        isci.SetApartmentState(ApartmentState.STA);
        isci.Start();
    }

    Task<T> Kabukta<T>(Func<T> f)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _kuyruk.Add(() =>
        {
            try { tcs.SetResult(f()); }
            catch (Exception e) { tcs.SetException(e); }
        });
        return tcs.Task;
    }

    /// <summary>Liste bir dakikadan eskiyse arka planda yeniden tarar.</summary>
    public Task YenileGerekirse()
    {
        if (_tarama is { IsCompleted: false }) return _tarama;
        if (DateTime.Now - _sonTarama < TimeSpan.FromMinutes(1)) return Task.CompletedTask;
        return _tarama = Yenile();
    }

    async Task Yenile()
    {
        List<Uygulama> liste;
        try { liste = await Kabukta(Tara); }
        catch (Exception e)
        {
            Gunluk.Yaz("Tarama başarısız: " + e);
            return;
        }
        Gunluk.Yaz($"Tarama: {liste.Count} uygulama");
        _sonTarama = DateTime.Now;
        Liste = liste;
        Yenilendi?.Invoke();
    }

    static List<Uygulama> Tara()
    {
        var tip = Type.GetTypeFromProgID("Shell.Application")!;
        dynamic kabuk = Activator.CreateInstance(tip)!;
        var sonuc = new List<Uygulama>();
        var gorulen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            dynamic klasor = kabuk.NameSpace(AppsFolder);
            foreach (dynamic oge in klasor.Items())
            {
                string ad = oge.Name;
                string yol = oge.Path;
                if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(yol)) continue;
                var kucuk = ad.ToLowerInvariant();
                if (AtlananKelimeler.Any(kucuk.Contains)) continue;
                if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
                if (AtlananUzantilar.Any(u => yol.EndsWith(u, StringComparison.OrdinalIgnoreCase))) continue;
                if (gorulen.Add(ad)) sonuc.Add(new Uygulama(ad, yol));
            }
        }
        finally { Marshal.FinalReleaseComObject(kabuk); }
        return sonuc;
    }

    /// <summary>İkonu önbellekten verir, yoksa üretir. UI iş parçacığından çağrılmalı.</summary>
    public async Task<ImageSource?> Ikon(Uygulama u, int piksel)
    {
        var anahtar = u.Kimlik + "|" + piksel;
        if (_ikonlar.TryGetValue(anahtar, out var hazir)) return hazir;

        var veri = await Kabukta(() => IkonPikselleri(u.Kimlik, piksel));
        if (veri is null) return null;

        var (en, boy, baytlar) = veri.Value;
        var bmp = new WriteableBitmap(en, boy);
        using (var akis = bmp.PixelBuffer.AsStream()) akis.Write(baytlar);
        bmp.Invalidate();
        _ikonlar[anahtar] = bmp;
        return bmp;
    }

    static (int, int, byte[])? IkonPikselleri(string kimlik, int piksel)
    {
        var iid = typeof(Yerel.IShellItemImageFactory).GUID;
        Yerel.IShellItemImageFactory fabrika;
        try { Yerel.SHCreateItemFromParsingName($@"{AppsFolder}\{kimlik}", IntPtr.Zero, ref iid, out fabrika); }
        catch (COMException) { return null; }

        try
        {
            var boyut = new Yerel.SIZE { cx = piksel, cy = piksel };
            if (fabrika.GetImage(boyut, Yerel.SIIGBF_ICONONLY | Yerel.SIIGBF_BIGGERSIZEOK, out var hbm) != 0)
                return null;
            try
            {
                Yerel.GetObject(hbm, Marshal.SizeOf<Yerel.BITMAP>(), out var bilgi);
                int en = bilgi.bmWidth, boy = bilgi.bmHeight;
                var bi = new Yerel.BITMAPINFOHEADER
                {
                    biSize = Marshal.SizeOf<Yerel.BITMAPINFOHEADER>(),
                    biWidth = en, biHeight = -boy, // eksi: satırlar yukarıdan aşağı
                    biPlanes = 1, biBitCount = 32,
                };
                var baytlar = new byte[en * boy * 4];
                var hdc = Yerel.GetDC(IntPtr.Zero);
                try { Yerel.GetDIBits(hdc, hbm, 0, (uint)boy, baytlar, ref bi, 0); }
                finally { Yerel.ReleaseDC(IntPtr.Zero, hdc); }

                // Alfa kanalı olmayan eski ikonlar tamamen saydam gelir; opak yap.
                var alfaVar = false;
                for (var i = 3; i < baytlar.Length && !alfaVar; i += 4) alfaVar = baytlar[i] != 0;
                if (!alfaVar) for (var i = 3; i < baytlar.Length; i += 4) baytlar[i] = 255;

                return (en, boy, baytlar);
            }
            finally { Yerel.DeleteObject(hbm); }
        }
        finally { Marshal.ReleaseComObject(fabrika); }
    }

    public static void Baslat(Uygulama u) =>
        Process.Start(new ProcessStartInfo($@"{AppsFolder}\{u.Kimlik}") { UseShellExecute = true });
}
