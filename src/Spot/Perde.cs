using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Spot.Services;
using Windows.Graphics;
using Windows.UI;

namespace Spot;

/// <summary>Panelin arkasında tüm ekranı kaplayan bulanık, kararmış perde (iPhone Spotlight gibi).
/// Hiç odak almaz; tıklanırsa panel odağı kaybeder ve ikisi birlikte gizlenir.</summary>
sealed class Perde : Window
{
    readonly DesktopAcrylicController _cam; // alanda tutulmazsa GC camı söker

    public Perde()
    {
        Content = new Grid();
        ExtendsContentIntoTitleBar = true;
        AppWindow.IsShownInSwitchers = false;
        if (AppWindow.Presenter is OverlappedPresenter p)
        {
            p.IsResizable = false;
            p.IsMaximizable = false;
            p.IsMinimizable = false;
            p.IsAlwaysOnTop = true;
            p.SetBorderAndTitleBar(false, false);
        }
        // Windows 11 kenarlığı ve yuvarlak köşesi tam ekran perdede ekran kenarında çizgi bırakıyor.
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        int yok = Yerel.DWMWA_COLOR_NONE, koseYok = Yerel.DWMWCP_DONOTROUND;
        Yerel.DwmSetWindowAttribute(hwnd, Yerel.DWMWA_BORDER_COLOR, ref yok, sizeof(int));
        Yerel.DwmSetWindowAttribute(hwnd, Yerel.DWMWA_WINDOW_CORNER_PREFERENCE, ref koseYok, sizeof(int));
        // SetBorderAndTitleBar(false,false) boyutlandırma çerçevesini bırakıyor (ekran kenarında 3 px
        // açık çizgi). Saf açılır pencere yap.
        var stil = (long)Yerel.GetWindowLongPtr(hwnd, Yerel.GWL_STYLE);
        stil = (stil & ~(Yerel.WS_CAPTION | Yerel.WS_THICKFRAME)) | Yerel.WS_POPUP;
        Yerel.SetWindowLongPtr(hwnd, Yerel.GWL_STYLE, (IntPtr)stil);
        Yerel.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            Yerel.SWP_NOMOVE | Yerel.SWP_NOSIZE | Yerel.SWP_NOZORDER | Yerel.SWP_NOACTIVATE | Yerel.SWP_FRAMECHANGED);

        _cam = Cam.Uygula(this, DesktopAcrylicKind.Base, Color.FromArgb(0xFF, 0x0A, 0x0A, 0x0C), 0.45f, 0.35f);
    }

    public void Goster(RectInt32 ekran)
    {
        AppWindow.MoveAndResize(ekran);
        AppWindow.Show(false);
    }

    public void Gizle() => AppWindow.Hide();
}
