namespace Spot.Core;

/// <summary>Başlatılabilir bir öğe. Uygulamada kimlik, shell:AppsFolder içindeki ayrıştırma adıdır
/// (Store uygulamasında AUMID, masaüstünde genelde exe yolu ya da AUMID); klasörde tam yoldur.</summary>
public sealed record Uygulama(string Ad, string Kimlik, bool Klasor = false);
