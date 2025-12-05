using System.Collections.Generic;

namespace Maggsoft.Framework.Options;

public class IPFilterOptions
{
    /// <summary>
    /// İzin verilen IP adresleri listesi
    /// Whitelist of allowed IP addresses
    /// </summary>
    public List<string> WhitelistedIPs { get; set; } = new();

    /// <summary>
    /// İzin verilen IP adresleri listesi (eski)
    /// Whitelist of allowed IP addresses (legacy)
    /// </summary>
    public List<string> AllowedIPs { get; set; } = new();

    /// <summary>
    /// Yasaklanan IP adresleri listesi
    /// Blacklist of blocked IP addresses
    /// </summary>
    public List<string> BlockedIPs { get; set; } = new();

    /// <summary>
    /// İzin verilen IP aralıkları (CIDR formatında)
    /// Allowed IP ranges in CIDR format
    /// </summary>
    public List<string> AllowedIPRanges { get; set; } = new();

    /// <summary>
    /// Yasaklanan IP aralıkları (CIDR formatında)
    /// Blocked IP ranges in CIDR format
    /// </summary>
    public List<string> BlockedIPRanges { get; set; } = new();

    /// <summary>
    /// Dakika başına izin verilen maksimum istek sayısı
    /// Maximum number of requests allowed per minute
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 100;

    /// <summary>
    /// Varsayılan izin politikası (true: izin ver, false: engelle)
    /// Default allow policy (true: allow, false: block)
    /// </summary>
    public bool DefaultAllow { get; set; } = true;

    /// <summary>
    /// IP filtreleme için muaf tutulacak path'ler
    /// Paths that are exempt from IP filtering
    /// </summary>
    public List<string> ExemptPaths { get; set; } = new();

    /// <summary>
    /// Strict Mode: Sadece WhitelistedIPs ve AllowedIPs listesindeki IP'ler erişebilir
    /// Strict Mode: Only IPs in WhitelistedIPs and AllowedIPs lists can access
    /// </summary>
    public bool StrictMode { get; set; } = false;

    /// <summary>
    /// İzin verilen domain listesi (örn: example.com, *.example.com)
    /// Whitelist of allowed domains (e.g., example.com, *.example.com)
    /// </summary>
    public List<string> WhitelistedDomains { get; set; } = new();

    /// <summary>
    /// Yasaklanan domain listesi (örn: spam.com, *.spam.com)
    /// Blacklist of blocked domains (e.g., spam.com, *.spam.com)
    /// </summary>
    public List<string> BlockedDomains { get; set; } = new();

    /// <summary>
    /// Domain kontrolünü aktif et
    /// Enable domain filtering
    /// </summary>
    public bool EnableDomainFilter { get; set; } = false;
}