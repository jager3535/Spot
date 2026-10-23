using Microsoft.UI.Xaml;

namespace Spot;

public partial class App : Application
{
    static Mutex? _tekOrnek;
    MainWindow? _pencere;

    public App()
    {
        InitializeComponent();
        // Gün boyu arka planda çalışıyor: tek bir hata yüzünden sessizce ölmesin.
        UnhandledException += (_, e) =>
        {
            Services.Gunluk.Yaz("Yakalanmamış hata: " + e.Exception);
            e.Handled = true;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Tek örnek: Spot zaten çalışıyorsa ikincisi sessizce kapanır.
        _tekOrnek = new Mutex(true, @"Local\Spot.TekOrnek", out var ilk);
        if (!ilk)
        {
            Exit();
            return;
        }

        // Pencere gizli doğar; Alt+` ile görünür.
        _pencere = new MainWindow();
    }
}
