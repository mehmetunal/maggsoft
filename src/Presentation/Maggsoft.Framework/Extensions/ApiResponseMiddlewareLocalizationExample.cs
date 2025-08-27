using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maggsoft.Framework.Middleware.ApiResponseMiddleware;
using Microsoft.AspNetCore.Http;

namespace Maggsoft.Framework.Extensions;

/// <summary>
/// ApiResponseMiddleware OnMessageLocalization Event Kullanım Örnekleri
/// Detaylı örnek datalarla açıklanmıştır
/// </summary>
public static class ApiResponseMiddlewareLocalizationExample
{
    /// <summary>
    /// ÖRNEK 1: Basit Dictionary-Based Localization
    /// </summary>
    public static void Example1_BasicDictionaryLocalization()
    {
        // Program.cs'de bu şekilde kullanılır:
        /*
        builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
        {
            options.OnMessageLocalization += HandleBasicLocalization;
        });
        */
    }

    public static void HandleBasicLocalization(object sender, MessageLocalizationEventArgs e)
    {
        Console.WriteLine("=== ÖRNEK 1: Basic Dictionary Localization ===");
        Console.WriteLine($"Gelen Event Args:");
        Console.WriteLine($"  MessageKey: {e.MessageKey}");
        Console.WriteLine($"  DefaultMessage: {e.DefaultMessage}");
        Console.WriteLine($"  Culture: {e.Culture}");
        Console.WriteLine($"  FormatArgs: [{string.Join(", ", e.FormatArgs ?? new object[0])}]");

        var culture = e.Culture ?? "en";
        var messages = GetBasicLocalizedMessages(culture);
        
        if (messages.TryGetValue(e.MessageKey, out var localizedMessage))
        {
            // Format parametreleri varsa uygula
            e.LocalizedMessage = e.FormatArgs?.Length > 0 
                ? string.Format(localizedMessage, e.FormatArgs) 
                : localizedMessage;
                
            Console.WriteLine($"  Localized Message: {e.LocalizedMessage}");
        }
        else
        {
            Console.WriteLine($"  Localization bulunamadı, default kullanılacak: {e.DefaultMessage}");
        }
        Console.WriteLine();
    }

    private static Dictionary<string, string> GetBasicLocalizedMessages(string culture)
    {
        return culture.ToLower() switch
        {
            "tr" or "tr-tr" => new Dictionary<string, string>
            {
                { ApiResponseMessages.KEY_ValidationFailed, "Doğrulama hatası! Lütfen verilerinizi kontrol edin." },
                { ApiResponseMessages.KEY_AnErrorOccurred, "Bir hata oluştu" },
                { ApiResponseMessages.KEY_BadRequest, "İsteğiniz işlenemedi" },
                { ApiResponseMessages.KEY_NotFound, "Kaynak bulunamadı" },
                { ApiResponseMessages.KEY_InternalServerError, "Sistem hatası oluştu" },
                { ApiResponseMessages.KEY_JsonParseError, "JSON Hata: {0}" },
                { ApiResponseMessages.KEY_TechnicalNotFound, "404 - Kaynak Bulunamadı" }
            },
            "en" or "en-us" => new Dictionary<string, string>
            {
                { ApiResponseMessages.KEY_ValidationFailed, "Validation failed! Please check your data." },
                { ApiResponseMessages.KEY_AnErrorOccurred, "An error occurred" },
                { ApiResponseMessages.KEY_BadRequest, "Your request could not be processed" },
                { ApiResponseMessages.KEY_NotFound, "Resource not found" },
                { ApiResponseMessages.KEY_InternalServerError, "System error occurred" },
                { ApiResponseMessages.KEY_JsonParseError, "JSON Error: {0}" },
                { ApiResponseMessages.KEY_TechnicalNotFound, "404 - Resource Not Found" }
            },
            _ => new Dictionary<string, string>() // Fallback - default mesajlar kullanılır
        };
    }

    /// <summary>
    /// ÖRNEK 2: Database'den Localization (Simüle edilmiş)
    /// </summary>
    public static void Example2_DatabaseLocalization()
    {
        // Program.cs'de bu şekilde kullanılır:
        /*
        builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
        {
            options.OnMessageLocalization += HandleDatabaseLocalization;
        });
        */
    }

    public static async void HandleDatabaseLocalization(object sender, MessageLocalizationEventArgs e)
    {
        Console.WriteLine("=== ÖRNEK 2: Database Localization ===");
        Console.WriteLine($"Gelen Event Args:");
        Console.WriteLine($"  MessageKey: {e.MessageKey}");
        Console.WriteLine($"  Culture: {e.Culture}");
        Console.WriteLine($"  User IP: {e.HttpContext?.Connection?.RemoteIpAddress}");
        Console.WriteLine($"  Request Path: {e.HttpContext?.Request?.Path}");

        // Simüle edilmiş database call
        var localizedMessage = await GetMessageFromDatabase(e.MessageKey, e.Culture ?? "en");
        
        if (!string.IsNullOrEmpty(localizedMessage))
        {
            e.LocalizedMessage = e.FormatArgs?.Length > 0 
                ? string.Format(localizedMessage, e.FormatArgs) 
                : localizedMessage;
                
            Console.WriteLine($"  Database'den alınan mesaj: {e.LocalizedMessage}");
        }
        else
        {
            Console.WriteLine($"  Database'de bulunamadı, default kullanılacak");
        }
        Console.WriteLine();
    }

    // Simüle edilmiş database call
    private static async Task<string?> GetMessageFromDatabase(string messageKey, string culture)
    {
        await Task.Delay(1); // Simüle edilmiş async call
        
        // Gerçek projede bu şekilde olurdu:
        /*
        using var dbContext = serviceProvider.GetService<YourDbContext>();
        return await dbContext.LocalizedMessages
            .Where(x => x.Key == messageKey && x.Culture == culture)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();
        */
        
        // Simüle edilmiş data
        var simulatedDb = new Dictionary<(string key, string culture), string>
        {
            { (ApiResponseMessages.KEY_ValidationFailed, "tr-TR"), "VERİTABANI: Doğrulama başarısız!" },
            { (ApiResponseMessages.KEY_BadRequest, "tr-TR"), "VERİTABANI: Hatalı istek!" },
            { (ApiResponseMessages.KEY_ValidationFailed, "en-US"), "DATABASE: Validation failed!" },
            { (ApiResponseMessages.KEY_BadRequest, "en-US"), "DATABASE: Bad request!" }
        };
        
        return simulatedDb.TryGetValue((messageKey, culture), out var message) ? message : null;
    }

    /// <summary>
    /// ÖRNEK 3: Gelişmiş Localization - User Preferences + Fallback
    /// </summary>
    public static void Example3_AdvancedLocalization()
    {
        // Program.cs'de bu şekilde kullanılır:
        /*
        builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
        {
            options.OnMessageLocalization += HandleAdvancedLocalization;
        });
        */
    }

    public static void HandleAdvancedLocalization(object sender, MessageLocalizationEventArgs e)
    {
        Console.WriteLine("=== ÖRNEK 3: Advanced Localization ===");
        Console.WriteLine($"Event Details:");
        Console.WriteLine($"  MessageKey: {e.MessageKey}");
        Console.WriteLine($"  Culture from header: {e.Culture}");
        Console.WriteLine($"  Default Message: {e.DefaultMessage}");
        Console.WriteLine($"  Format Args Count: {e.FormatArgs?.Length ?? 0}");

        // 1. Kullanıcı tercihlerini kontrol et (simüle edilmiş)
        var userPreferredLanguage = GetUserLanguagePreference(e.HttpContext);
        Console.WriteLine($"  User Preferred Language: {userPreferredLanguage}");

        // 2. En uygun kültürü belirle
        var finalCulture = DetermineFinalCulture(e.Culture, userPreferredLanguage);
        Console.WriteLine($"  Final Culture: {finalCulture}");

        // 3. Localization'ı al
        var localizedMessage = GetAdvancedLocalizedMessage(e.MessageKey, finalCulture);
        
        if (!string.IsNullOrEmpty(localizedMessage))
        {
            e.LocalizedMessage = e.FormatArgs?.Length > 0 
                ? string.Format(localizedMessage, e.FormatArgs) 
                : localizedMessage;
                
            Console.WriteLine($"  Final Localized Message: {e.LocalizedMessage}");
        }
        else
        {
            Console.WriteLine($"  Localization bulunamadı, fallback kullanılacak");
        }
        Console.WriteLine();
    }

    private static string? GetUserLanguagePreference(HttpContext? context)
    {
        // Simüle edilmiş user preference
        // Gerçek projede user session, JWT claim, database vb. kontrol edilir
        
        if (context?.Request.Cookies.TryGetValue("UserLanguage", out var userLang) == true)
            return userLang;
            
        if (context?.User?.FindFirst("PreferredLanguage")?.Value is string claimLang)
            return claimLang;
            
        return null;
    }

    private static string DetermineFinalCulture(string? headerCulture, string? userPreference)
    {
        // Öncelik sırası: User Preference > Header > Default
        return userPreference ?? headerCulture ?? "en-US";
    }

    private static string? GetAdvancedLocalizedMessage(string messageKey, string culture)
    {
        // Çoklu kaynak kontrolü: Database > File > Memory
        var sources = new Dictionary<string, Dictionary<string, string>>
        {
            ["tr-TR"] = new()
            {
                { ApiResponseMessages.KEY_ValidationFailed, "GELİŞMİŞ: Form doğrulaması başarısız!" },
                { ApiResponseMessages.KEY_BadRequest, "GELİŞMİŞ: İstek kabul edilemedi!" },
                { ApiResponseMessages.KEY_NotFound, "GELİŞMİŞ: Aradığınız sayfa bulunamadı!" }
            },
            ["en-US"] = new()
            {
                { ApiResponseMessages.KEY_ValidationFailed, "ADVANCED: Form validation failed!" },
                { ApiResponseMessages.KEY_BadRequest, "ADVANCED: Request cannot be accepted!" },
                { ApiResponseMessages.KEY_NotFound, "ADVANCED: The page you're looking for was not found!" }
            }
        };

        return sources.TryGetValue(culture, out var messages) && 
               messages.TryGetValue(messageKey, out var message) 
               ? message : null;
    }

    /// <summary>
    /// ÖRNEK 4: Çalışma Zamanı Test Scenarios
    /// </summary>
    public static void ShowRuntimeScenarios()
    {
        Console.WriteLine("=== RUNTIME SCENARIO ÖRNEKLERİ ===\n");

        // Senaryo 1: Validation Error
        Console.WriteLine("SENARYO 1: FluentValidation Hatası");
        Console.WriteLine("Request: POST /api/users");
        Console.WriteLine("Headers: X-Language: tr-TR");
        Console.WriteLine("Body: { \"email\": \"\", \"name\": \"a\" }");
        Console.WriteLine("Event Tetiklenir:");
        Console.WriteLine("  MessageKey: ApiResponse.ValidationFailed");
        Console.WriteLine("  Culture: tr-TR");
        Console.WriteLine("  DefaultMessage: Validation failed. Please check your input data.");
        Console.WriteLine("Event Handler Response:");
        Console.WriteLine("  LocalizedMessage: Doğrulama hatası! Lütfen verilerinizi kontrol edin.");
        Console.WriteLine("Final API Response:");
        Console.WriteLine(@"  {
    ""isSuccess"": false,
    ""data"": null,
    ""message"": ""Doğrulama hatası! Lütfen verilerinizi kontrol edin."",
    ""errors"": [
      ""Email: E-posta adresi gerekli"",
      ""Name: İsim en az 3 karakter olmalı""
    ]
  }");
        Console.WriteLine();

        // Senaryo 2: 404 Error
        Console.WriteLine("SENARYO 2: 404 Not Found");
        Console.WriteLine("Request: GET /api/users/999");
        Console.WriteLine("Headers: X-Language: en-US");
        Console.WriteLine("Event Tetiklenir:");
        Console.WriteLine("  MessageKey: ApiResponse.NotFound");
        Console.WriteLine("  Culture: en-US");
        Console.WriteLine("  DefaultMessage: The requested resource was not found");
        Console.WriteLine("Event Handler Response:");
        Console.WriteLine("  LocalizedMessage: Resource not found");
        Console.WriteLine("Final API Response:");
        Console.WriteLine(@"  {
    ""isSuccess"": false,
    ""data"": null,
    ""message"": ""Resource not found"",
    ""errors"": [""404 - Resource Not Found""]
  }");
        Console.WriteLine();

        // Senaryo 3: JSON Parse Error with Parameters
        Console.WriteLine("SENARYO 3: JSON Parse Error (Development)");
        Console.WriteLine("Request: POST /api/data");
        Console.WriteLine("Headers: X-Language: tr-TR");
        Console.WriteLine("Body: { invalid json }");
        Console.WriteLine("Event Tetiklenir:");
        Console.WriteLine("  MessageKey: ApiResponse.JsonParseError");
        Console.WriteLine("  Culture: tr-TR");
        Console.WriteLine("  DefaultMessage: JSON Parse Error: {0}");
        Console.WriteLine("  FormatArgs: [\"Unexpected character at position 2\"]");
        Console.WriteLine("Event Handler Response:");
        Console.WriteLine("  LocalizedMessage: JSON Hata: Unexpected character at position 2");
        Console.WriteLine("Final API Response:");
        Console.WriteLine(@"  {
    ""isSuccess"": false,
    ""data"": null,
    ""message"": ""Yanıt işlenirken bir hata oluştu"",
    ""errors"": [""JSON Hata: Unexpected character at position 2""]
  }");
        Console.WriteLine();
    }
}

/// <summary>
/// Test için simüle edilmiş LocalizedMessage entity
/// </summary>
public class LocalizedMessage
{
    public string Key { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
