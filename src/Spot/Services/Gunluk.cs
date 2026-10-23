namespace Spot.Services;

/// <summary>Sessiz hataları görünür kılmak için %LOCALAPPDATA%\Spot\gunluk.txt.</summary>
static class Gunluk
{
    static readonly string Yol = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spot", "gunluk.txt");

    public static void Yaz(string mesaj)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Yol)!);
            if (File.Exists(Yol) && new FileInfo(Yol).Length > 256 * 1024) File.Delete(Yol);
            File.AppendAllText(Yol, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {mesaj}{Environment.NewLine}");
        }
        catch (IOException) { }
    }
}
