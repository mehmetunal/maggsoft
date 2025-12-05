using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Maggsoft.Framework.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maggsoft.Framework.Middleware;

public class IPFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IPFilterMiddleware> _logger;
    private readonly IPFilterOptions _options;

    public IPFilterMiddleware(
        RequestDelegate next,
        ILogger<IPFilterMiddleware> logger,
        IOptions<IPFilterOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = GetIPAddress(context);
        var path = context.Request.Path.Value?.ToLower();

        // Muaf path kontrolü
        if (_options.ExemptPaths.Any(p => path?.StartsWith(p.ToLower()) ?? false))
        {
            await _next(context);
            return;
        }

        // Domain kontrolü (IP kontrolünden önce)
        if (_options.EnableDomainFilter)
        {
            var domain = GetDomain(context);
            if (!IsDomainAllowed(domain))
            {
                _logger.LogWarning("Domain engellendi: {Domain} (IP: {IpAddress})", domain, ipAddress);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Bu domain'den erişim engellendi",
                    domain = domain,
                    ipAddress = ipAddress
                });
                return;
            }
        }

        // IP kontrolü
        if (IsIPAllowed(ipAddress))
        {
            await _next(context);
        }
        else
        {
            _logger.LogWarning("IP adresi engellendi: {IpAddress}", ipAddress);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Bu IP adresinden erişim engellendi",
                ipAddress = ipAddress
            });
        }
    }

    private bool IsIPAllowed(string ipAddress)
    {
        // IP adresi direkt olarak yasaklı listede mi?
        if (_options.BlockedIPs.Contains(ipAddress))
            return false;

        // IP adresi direkt olarak izin verilen listede mi?
        if (_options.WhitelistedIPs.Contains(ipAddress))
            return true;
        
        // IP adresi direkt olarak izin verilen listede mi?
        if (_options.AllowedIPs.Contains(ipAddress))
            return true;

        // Strict Mode aktifse, sadece whitelist'teki IP'lere izin ver
        if (_options.StrictMode)
        {
            _logger.LogInformation("Strict Mode aktif: IP {IpAddress} sadece whitelist kontrolü yapılıyor", ipAddress);
            return false; // Sadece yukarıdaki whitelist kontrollerinde true dönen IP'ler erişebilir
        }

        // IP adresi yasaklı bir aralıkta mı?
        if (IsIPInRanges(ipAddress, _options.BlockedIPRanges))
            return false;

        // IP adresi izin verilen bir aralıkta mı?
        if (IsIPInRanges(ipAddress, _options.AllowedIPRanges))
            return true;

        // Hiçbir kural eşleşmezse varsayılan politikayı uygula
        return _options.DefaultAllow;
    }

    private static bool IsIPInRanges(string ipAddress, List<string> ranges)
    {
        if (!IPAddress.TryParse(ipAddress, out var address))
            return false;

        var addressBytes = address.GetAddressBytes();

        foreach (var range in ranges)
        {
            var parts = range.Split('/');
            if (parts.Length != 2) continue;

            if (!IPAddress.TryParse(parts[0], out var networkAddress))
                continue;

            if (!int.TryParse(parts[1], out var prefixLength))
                continue;

            var networkBytes = networkAddress.GetAddressBytes();
            if (addressBytes.Length != networkBytes.Length)
                continue;

            if (IsInSubnet(addressBytes, networkBytes, prefixLength))
                return true;
        }

        return false;
    }

    private static bool IsInSubnet(byte[] address, byte[] network, int prefixLength)
    {
        var byteLength = address.Length;
        var prefixFullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        // Tam byte'ları kontrol et
        for (var i = 0; i < prefixFullBytes && i < byteLength; i++)
        {
            if (address[i] != network[i])
                return false;
        }

        // Kalan bitleri kontrol et
        if (remainingBits > 0 && prefixFullBytes < byteLength)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((address[prefixFullBytes] & mask) != (network[prefixFullBytes] & mask))
                return false;
        }

        return true;
    }

    private static string GetIPAddress(HttpContext context)
    {
        // X-Forwarded-For header'ını kontrol et
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Remote IP adresini al
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // IPv6 ise IPv4'e dönüştür
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }
            return remoteIp.ToString();
        }

        return "0.0.0.0";
    }

    private static string GetDomain(HttpContext context)
    {
        // Host header'ından domain'i al
        var host = context.Request.Headers["Host"].FirstOrDefault();
        if (string.IsNullOrEmpty(host))
        {
            return string.Empty;
        }

        // Port numarasını kaldır (örn: example.com:8080 -> example.com)
        var domain = host.Split(':')[0].ToLower().Trim();
        return domain;
    }

    private bool IsDomainAllowed(string domain)
    {
        if (string.IsNullOrEmpty(domain))
        {
            // Domain bilgisi yoksa varsayılan politikayı uygula
            return _options.DefaultAllow;
        }

        // Domain direkt olarak yasaklı listede mi?
        if (IsDomainInList(domain, _options.BlockedDomains))
            return false;

        // Domain direkt olarak izin verilen listede mi?
        if (IsDomainInList(domain, _options.WhitelistedDomains))
            return true;

        // Hiçbir kural eşleşmezse varsayılan politikayı uygula
        return _options.DefaultAllow;
    }

    private static bool IsDomainInList(string domain, List<string> domainList)
    {
        foreach (var listedDomain in domainList)
        {
            var normalizedListedDomain = listedDomain.ToLower().Trim();
            
            // Tam eşleşme kontrolü
            if (domain.Equals(normalizedListedDomain, System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Wildcard domain kontrolü (örn: *.example.com)
            if (normalizedListedDomain.StartsWith("*."))
            {
                var baseDomain = normalizedListedDomain.Substring(2); // "*." kısmını kaldır
                if (domain.EndsWith("." + baseDomain, System.StringComparison.OrdinalIgnoreCase) ||
                    domain.Equals(baseDomain, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}