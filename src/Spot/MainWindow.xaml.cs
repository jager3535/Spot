using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Spot.Core;
using Spot.Services;
using Windows.Graphics;
using Windows.System;
using WinRT.Interop;

namespace Spot;

public sealed partial class MainWindow : Window
{
    const double Genislik = 680;   // SABITLER.md
    const int EnFazlaSonuc = 6;    // en iyi eşleşenin altındaki uygulama sayısı

    static readonly Brush SeciliArkaPlan = new SolidColorBrush(ColorHelper.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
    static readonly Brush KartArkaPlan = new SolidColorBrush(ColorHelper.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
    static readonly Brush Saydam = new SolidColorBrush(Colors.Transparent);

    readonly IntPtr _hwnd;
    readonly UygulamaDizini _dizin = new();
    readonly KullanimSayaci _kullanim = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spot", "kullanim.json"));
    readonly KisayolDinleyici _kisayol;
    readonly DesktopAcrylicController _cam; // alanda tutulmazsa GC camı söker

    List<Eslesme> _sonuclar = [];
    readonly List<Border> _satirlar = [];
    int _secili;

    // Pencere tüm ekranı kaplayan bulanık perdedir; panel onun içinde çizilen bir katman.
    // Ayrı pencere olsaydı Windows köşeyi en fazla 8 px yuvarlardı.
    public MainWindow()
    {
        InitializeComponent();
        _hwnd = WindowNative.GetWindowHandle(this);

        ExtendsContentIntoTitleBar = true;
        _cam = Cam.Uygula(this, DesktopAcrylicKind.Base, ColorHelper.FromArgb(0xFF, 0x0A, 0x0A, 0x0C), 0.45f, 0.35f);
        AppWindow.IsShownInSwitchers = false; // Alt+Tab'da ve görev çubuğunda görünmesin
        if (AppWindow.Presenter is OverlappedPresenter p)
        {
            p.IsResizable = false;
            p.IsMaximizable = false;
            p.IsMinimizable = false;
            p.IsAlwaysOnTop = true;
            p.SetBorderAndTitleBar(false, false);
        }
        CerceveyiKaldir();

        // Panelin dışına (perdeye) tıklamak Spot'u kapatır.
        Kok.Tapped += (_, e) => { if (e.OriginalSource == Kok) Gizle(); };

        // Alt+F4 pencereyi kapatırsa süreç ve kısayol da gider; kapatmak yerine gizle.
        AppWindow.Closing += (_, a) => { a.Cancel = true; Gizle(); };

        Activated += (_, e) =>
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated) Gizle();
        };

        _dizin.Yenilendi += () => { if (AppWindow.IsVisible) Guncelle(); };
        _ = _dizin.YenileGerekirse();

        _kisayol = new KisayolDinleyici(_hwnd);
        _kisayol.Basildi += () => { if (AppWindow.IsVisible) Gizle(); else Goster(); };
        if (!_kisayol.Kayitli)
        {
            // Alt+` başka bir program tarafından alınmış: sessizce çalışmak yerine söyle.
            Goster();
            Kutu.PlaceholderText = "Alt+` başka bir programda kayıtlı, Spot dinleyemiyor";
        }
    }

    double Olcek => Yerel.GetDpiForWindow(_hwnd) / 96.0;

    void Goster()
    {
        _ = _dizin.YenileGerekirse();
        Kutu.Text = "";
        Guncelle();

        // İmlecin bulunduğu ekranı kapla; kutu çalışma alanının (görev çubuğu hariç) tam ortasında.
        // Kutu yerinde kalır, sonuçlar aşağı uzar.
        Yerel.GetCursorPos(out var imlec);
        var ekran = DisplayArea.GetFromPoint(new PointInt32(imlec.X, imlec.Y), DisplayAreaFallback.Primary);
        var dis = ekran.OuterBounds;
        var alan = ekran.WorkArea;
        AppWindow.MoveAndResize(dis);
        var ortaY = (alan.Y - dis.Y + alan.Height / 2.0) / Olcek;
        Panel.Width = Genislik;
        Panel.Margin = new Thickness(0, Math.Max(0, ortaY - KapaliYukseklik / 2), 0, 0);

        AppWindow.Show(true);
        Activate();
        Yerel.SetForegroundWindow(_hwnd);
        Kutu.Focus(FocusState.Programmatic);
    }

    const double KapaliYukseklik = 80; // sonuç yokken panel: 12 + 56 kutu + 12

    void Gizle() => AppWindow.Hide();

    // SetBorderAndTitleBar(false,false) boyutlandırma çerçevesini bırakıyor (ekran kenarında 3 px açık
    // çizgi); DWM de kenarlık ve köşe yuvarlaması ekliyor. Saf, çerçevesiz açılır pencere yap.
    void CerceveyiKaldir()
    {
        int yok = Yerel.DWMWA_COLOR_NONE, koseYok = Yerel.DWMWCP_DONOTROUND;
        Yerel.DwmSetWindowAttribute(_hwnd, Yerel.DWMWA_BORDER_COLOR, ref yok, sizeof(int));
        Yerel.DwmSetWindowAttribute(_hwnd, Yerel.DWMWA_WINDOW_CORNER_PREFERENCE, ref koseYok, sizeof(int));
        var stil = (long)Yerel.GetWindowLongPtr(_hwnd, Yerel.GWL_STYLE);
        stil = (stil & ~(Yerel.WS_CAPTION | Yerel.WS_THICKFRAME)) | Yerel.WS_POPUP;
        Yerel.SetWindowLongPtr(_hwnd, Yerel.GWL_STYLE, (IntPtr)stil);
        Yerel.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0,
            Yerel.SWP_NOMOVE | Yerel.SWP_NOSIZE | Yerel.SWP_NOZORDER | Yerel.SWP_NOACTIVATE | Yerel.SWP_FRAMECHANGED);
    }

    void Kutu_TextChanged(object sender, TextChangedEventArgs e) => Guncelle();

    void Guncelle()
    {
        _sonuclar = Eslestirici.Ara(Kutu.Text, _dizin.Liste, _kullanim.Sayi);
        _secili = 0;

        EnIyiAlan.Children.Clear();
        DigerAlan.Children.Clear();
        _satirlar.Clear();

        if (_sonuclar.Count == 0)
        {
            Sonuclar.Visibility = Visibility.Collapsed;
        }
        else
        {
            Sonuclar.Visibility = Visibility.Visible;
            EnIyiAlan.Children.Add(Satir(_sonuclar[0], buyuk: true));
            foreach (var e in _sonuclar.Skip(1).Take(EnFazlaSonuc))
                DigerAlan.Children.Add(Satir(e, buyuk: false));
            DigerBaslik.Visibility = _sonuclar.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        SecimiBoya();
    }

    /// <summary>Kutudaki soluk tamamlama, seçili sonuca göre: "chr" + "ome" + " — Google Chrome".</summary>
    void HayaletiGuncelle()
    {
        HayaletSorgu.Text = Kutu.Text;
        if (_sonuclar.Count == 0 || Kutu.Text.Length == 0 || Kutu.Text.EndsWith(' '))
        {
            HayaletTamamlama.Text = HayaletEk.Text = "";
            return;
        }
        var e = _sonuclar[_secili];
        var tamamlama = _secili == 0 ? e.Tamamlama : "";
        HayaletTamamlama.Text = tamamlama;
        // Ad sorguyla başlıyorsa (tam yazılmış olsa bile) iPhone'daki gibi "— Aç".
        HayaletEk.Text = _secili == 0 && e.Tur == EslesmeTuru.AdBasi ? " — Aç" : " — " + e.Uygulama.Ad;
    }

    Border Satir(Eslesme e, bool buyuk)
    {
        var ikonBoyu = buyuk ? 56 : 32;
        var ikon = new Image { Width = ikonBoyu, Height = ikonBoyu, VerticalAlignment = VerticalAlignment.Center };
        _ = IkonuYukle(ikon, e.Uygulama, (int)Math.Ceiling(ikonBoyu * Olcek));

        var metin = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(buyuk ? 16 : 12, 0, 0, 0) };
        metin.Children.Add(new TextBlock
        {
            Text = e.Uygulama.Ad,
            FontSize = buyuk ? 20 : 15,
            FontWeight = buyuk ? FontWeights.SemiBold : FontWeights.Normal,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });
        if (buyuk)
            metin.Children.Add(new TextBlock { Text = "Uygulama", FontSize = 13, Foreground = new SolidColorBrush(ColorHelper.FromArgb(0x99, 0xFF, 0xFF, 0xFF)) });

        var icerik = new Grid();
        icerik.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        icerik.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(metin, 1);
        icerik.Children.Add(ikon);
        icerik.Children.Add(metin);

        var satir = new Border
        {
            Child = icerik,
            CornerRadius = new CornerRadius(buyuk ? 18 : 10),
            Padding = buyuk ? new Thickness(14) : new Thickness(10, 7, 10, 7),
        };
        var sira = _satirlar.Count;
        satir.Tapped += (_, _) => Ac(sira);
        // PointerEntered değil: satırlar her tuşta yeniden kurulur, duran fare seçimi çalmasın.
        satir.PointerMoved += (_, _) =>
        {
            if (_secili == sira) return;
            _secili = sira;
            SecimiBoya();
        };
        _satirlar.Add(satir);
        return satir;
    }

    async Task IkonuYukle(Image hedef, Uygulama u, int piksel) => hedef.Source = await _dizin.Ikon(u, piksel);

    void SecimiBoya()
    {
        for (var i = 0; i < _satirlar.Count; i++)
            _satirlar[i].Background = i == _secili ? SeciliArkaPlan : i == 0 ? KartArkaPlan : Saydam;
        HayaletiGuncelle();
    }

    void Kutu_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Escape:
                Gizle();
                break;
            case VirtualKey.Enter:
                Ac(_secili);
                break;
            case VirtualKey.Down when _satirlar.Count > 0:
                _secili = (_secili + 1) % _satirlar.Count;
                SecimiBoya();
                break;
            case VirtualKey.Up when _satirlar.Count > 0:
                _secili = (_secili - 1 + _satirlar.Count) % _satirlar.Count;
                SecimiBoya();
                break;
            case VirtualKey.Tab when HayaletTamamlama.Text.Length > 0:
                // Tamamlamayı kabul et: kutuya yaz, imleci sona al.
                Kutu.Text += HayaletTamamlama.Text;
                Kutu.SelectionStart = Kutu.Text.Length;
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    void Ac(int sira)
    {
        if (sira < 0 || sira >= _satirlar.Count) return;
        var u = _sonuclar[sira].Uygulama;
        Gizle();
        try
        {
            UygulamaDizini.Baslat(u);
            _kullanim.Artir(u.Kimlik);
        }
        // Kaldırılmış uygulama vb.: bir sonraki taramada listeden düşer.
        catch (Exception e) { Gunluk.Yaz($"Açılamadı ({u.Ad}): {e.Message}"); }
    }
}
