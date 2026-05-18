using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Maggsoft.Framework.Utilities;

/// <summary>
/// Reverse proxy / Cloudflare arkasında gerçek istemci IPv4 adresini çözer.
/// </summary>
public static class HttpContextClientIpResolver
{
    private const string CfConnectingIpHeader = "CF-Connecting-IP";
    private const string TrueClientIpHeader = "True-Client-IP";
    private const string XForwardedForHeader = "X-Forwarded-For";
    private const string XRealIpHeader = "X-Real-IP";

    /// <summary>
    /// İstek için etkili istemci IP'sini döndürür (whatismyipaddress ile uyumlu IPv4).
    /// </summary>
    public static string GetClientIpAddress(HttpContext context)
    {
        var cfIp = NormalizeIpToken(context.Request.Headers[CfConnectingIpHeader].FirstOrDefault());
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp;
        }

        var trueClientIp = NormalizeIpToken(context.Request.Headers[TrueClientIpHeader].FirstOrDefault());
        if (!string.IsNullOrEmpty(trueClientIp))
        {
            return trueClientIp;
        }

        var realIp = NormalizeIpToken(context.Request.Headers[XRealIpHeader].FirstOrDefault());
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            var remoteIpString = NormalizeIpToken(remoteIp.ToString());
            if (!string.IsNullOrEmpty(remoteIpString))
            {
                return remoteIpString;
            }
        }

        var forwardedFor = context.Request.Headers[XForwardedForHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstHop = forwardedFor.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            var parsedForwarded = NormalizeIpToken(firstHop);
            if (!string.IsNullOrEmpty(parsedForwarded))
            {
                return parsedForwarded;
            }
        }

        return "0.0.0.0";
    }

    private static string? NormalizeIpToken(string? ipToken)
    {
        if (string.IsNullOrWhiteSpace(ipToken))
        {
            return null;
        }

        var token = ipToken.Trim();

        if (token.StartsWith('[') && token.Contains(']'))
        {
            var closingIndex = token.IndexOf(']');
            if (closingIndex > 1)
            {
                token = token.Substring(1, closingIndex - 1);
            }
        }

        var colonCount = token.Count(c => c == ':');
        if (colonCount == 1 && token.Contains('.'))
        {
            token = token.Split(':', 2)[0];
        }

        if (!IPAddress.TryParse(token, out var ipAddress))
        {
            return null;
        }

        if (IPAddress.IsLoopback(ipAddress))
        {
            return "127.0.0.1";
        }

        return ipAddress.MapToIPv4().ToString();
    }
}
