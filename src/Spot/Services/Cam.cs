using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Windows.UI;
using WinRT;

namespace Spot.Services;

/// <summary>Ayarlanabilir buzlu cam (acrylic). Hazır DesktopAcrylicBackdrop'un saydamlığı
/// değiştirilemiyor ve pencere odak kaybedince düz renge dönüyor; burada ikisi de bizde.</summary>
static class Cam
{
    public static DesktopAcrylicController Uygula(Window pencere, DesktopAcrylicKind tur,
        Color renk, float renkOrani, float isikOrani)
    {
        var ayar = new SystemBackdropConfiguration
        {
            IsInputActive = true, // odakta olmasa da cam kalsın (perde hiç odak almaz)
            Theme = SystemBackdropTheme.Dark,
        };
        var cam = new DesktopAcrylicController
        {
            Kind = tur,
            TintColor = renk,
            TintOpacity = renkOrani,
            LuminosityOpacity = isikOrani,
            FallbackColor = Color.FromArgb(0xF0, renk.R, renk.G, renk.B),
        };
        cam.AddSystemBackdropTarget(pencere.As<ICompositionSupportsSystemBackdrop>());
        cam.SetSystemBackdropConfiguration(ayar);
        return cam;
    }
}
