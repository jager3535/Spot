namespace Spot.Core;

/// <summary>Başlatılabilir bir uygulama. Kimlik, shell:AppsFolder içindeki ayrıştırma adıdır
/// (Store uygulamasında AUMID, masaüstünde genelde exe yolu ya da AUMID).</summary>
public sealed record Uygulama(string Ad, string Kimlik);
