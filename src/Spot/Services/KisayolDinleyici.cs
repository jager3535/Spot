namespace Spot.Services;

/// <summary>Alt+` genel kısayolunu kaydeder; basılınca <see cref="Basildi"/> tetiklenir.</summary>
sealed class KisayolDinleyici : IDisposable
{
    const int Kimlik = 1;
    readonly IntPtr _hwnd;
    readonly Yerel.SubclassProc _proc; // GC toplamasın diye alanda tutuluyor

    public event Action? Basildi;
    public bool Kayitli { get; }

    public KisayolDinleyici(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _proc = PencereMesaji;
        Yerel.SetWindowSubclass(hwnd, _proc, 1, IntPtr.Zero);
        Kayitli = Yerel.RegisterHotKey(hwnd, Kimlik, Yerel.MOD_ALT | Yerel.MOD_NOREPEAT, Yerel.VK_OEM_3);
    }

    IntPtr PencereMesaji(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data)
    {
        if (msg == Yerel.WM_HOTKEY && wParam == Kimlik)
        {
            // Yerel geri çağrıdan kaçan istisna süreci öldürür.
            try { Basildi?.Invoke(); }
            catch (Exception e) { Gunluk.Yaz("Kısayol işlenemedi: " + e); }
            return IntPtr.Zero;
        }
        return Yerel.DefSubclassProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose() => Yerel.UnregisterHotKey(_hwnd, Kimlik);
}
