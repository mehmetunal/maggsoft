# ApiResponseMiddleware Event Kullanım Kılavuzu - Pratik Örnekler

## 🎯 Olay Zinciri Nasıl Çalışır?

### 1. İstek Gelir
```
HTTP POST /api/users
Headers: X-Language: tr-TR
Body: { "email": "", "name": "a" }
```

### 2. Middleware Devreye Girer
```csharp
// Middleware response'u yakalayıp işler
var statusCode = 400; // Bad Request (validation error)
var messageKey = "ApiResponse.ValidationFailed";
var defaultMessage = "Validation failed. Please check your input data.";
```

### 3. Event Tetiklenir
```csharp
var eventArgs = new MessageLocalizationEventArgs
{
    MessageKey = "ApiResponse.ValidationFailed",
    DefaultMessage = "Validation failed. Please check your input data.",
    FormatArgs = null,
    Culture = "tr-TR", // X-Language header'ından
    HttpContext = context,
    LocalizedMessage = null // Event handler dolduracak
};

options.OnMessageLocalization?.Invoke(this, eventArgs);
```

### 4. Event Handler Çalışır
```csharp
options.OnMessageLocalization += (sender, e) =>
{
    Console.WriteLine($"Event tetiklendi!");
    Console.WriteLine($"  MessageKey: {e.MessageKey}"); 
    // Çıktı: ApiResponse.ValidationFailed
    
    Console.WriteLine($"  Culture: {e.Culture}");      
    // Çıktı: tr-TR
    
    Console.WriteLine($"  Default: {e.DefaultMessage}"); 
    // Çıktı: Validation failed. Please check your input data.

    // Kendi localization sistemini kullan
    var messages = GetMyMessages(e.Culture);
    e.LocalizedMessage = messages[e.MessageKey];
    // e.LocalizedMessage = "Doğrulama hatası! Lütfen verilerinizi kontrol edin."
};
```

### 5. Final Response Oluşur
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Doğrulama hatası! Lütfen verilerinizi kontrol edin.",
  "errors": [
    "Email: E-posta adresi gerekli",
    "Name: İsim en az 3 karakter olmalı"
  ]
}
```

## 📋 Detaylı Kullanım Örnekleri

### Örnek 1: Basit Dictionary Localization

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += (sender, e) =>
    {
        // Debug için event bilgilerini logla
        Console.WriteLine($"🔥 Event tetiklendi:");
        Console.WriteLine($"   Key: {e.MessageKey}");
        Console.WriteLine($"   Culture: {e.Culture}");
        Console.WriteLine($"   Default: {e.DefaultMessage}");
        
        var culture = e.Culture ?? "en";
        var messages = GetMyLocalizedMessages(culture);
        
        if (messages.TryGetValue(e.MessageKey, out var localizedMessage))
        {
            e.LocalizedMessage = localizedMessage;
            Console.WriteLine($"   ✅ Localized: {e.LocalizedMessage}");
        }
        else
        {
            Console.WriteLine($"   ❌ Localization bulunamadı, default kullanılacak");
        }
    };
});

private static Dictionary<string, string> GetMyLocalizedMessages(string culture)
{
    return culture.ToLower() switch
    {
        "tr" or "tr-tr" => new Dictionary<string, string>
        {
            { "ApiResponse.ValidationFailed", "❌ Doğrulama hatası! Formunuzu kontrol edin." },
            { "ApiResponse.BadRequest", "🚫 İsteğiniz kabul edilemedi!" },
            { "ApiResponse.NotFound", "🔍 Aradığınız kaynak bulunamadı!" },
            { "ApiResponse.InternalServerError", "⚠️ Sistem hatası! Lütfen daha sonra deneyin." }
        },
        "en" or "en-us" => new Dictionary<string, string>
        {
            { "ApiResponse.ValidationFailed", "❌ Validation error! Check your form." },
            { "ApiResponse.BadRequest", "🚫 Your request was rejected!" },
            { "ApiResponse.NotFound", "🔍 The resource you're looking for was not found!" },
            { "ApiResponse.InternalServerError", "⚠️ System error! Please try again later." }
        },
        _ => new Dictionary<string, string>() // Fallback
    };
}
```

### Örnek 2: JSON Resource Dosyası ile Localization

```csharp
// appsettings.json
{
  "Localization": {
    "tr-TR": {
      "ApiResponse.ValidationFailed": "Doğrulama başarısız! 📝",
      "ApiResponse.BadRequest": "Hatalı istek! ❌",
      "ApiResponse.NotFound": "Kaynak bulunamadı! 🔍"
    },
    "en-US": {
      "ApiResponse.ValidationFailed": "Validation failed! 📝",
      "ApiResponse.BadRequest": "Bad request! ❌", 
      "ApiResponse.NotFound": "Resource not found! 🔍"
    }
  }
}

// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += (sender, e) =>
    {
        var configuration = e.HttpContext?.RequestServices?.GetService<IConfiguration>();
        if (configuration != null)
        {
            var culture = e.Culture ?? "en-US";
            var localizationKey = $"Localization:{culture}:{e.MessageKey}";
            var localizedMessage = configuration[localizationKey];
            
            if (!string.IsNullOrEmpty(localizedMessage))
            {
                e.LocalizedMessage = e.FormatArgs?.Length > 0 
                    ? string.Format(localizedMessage, e.FormatArgs) 
                    : localizedMessage;
                    
                Console.WriteLine($"📄 JSON'dan alındı: {e.LocalizedMessage}");
            }
        }
    };
});
```

### Örnek 3: Database ile Dynamic Localization

```csharp
// Entity
public class LocalizedMessage
{
    public string Key { get; set; }
    public string Culture { get; set; }
    public string Value { get; set; }
}

// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += async (sender, e) =>
    {
        var dbContext = e.HttpContext?.RequestServices?.GetService<MyDbContext>();
        if (dbContext != null)
        {
            var culture = e.Culture ?? "en-US";
            
            Console.WriteLine($"🗄️ Database'den aranıyor: {e.MessageKey} - {culture}");
            
            var localizedMessage = await dbContext.LocalizedMessages
                .Where(x => x.Key == e.MessageKey && x.Culture == culture)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
                
            if (!string.IsNullOrEmpty(localizedMessage))
            {
                e.LocalizedMessage = e.FormatArgs?.Length > 0 
                    ? string.Format(localizedMessage, e.FormatArgs) 
                    : localizedMessage;
                    
                Console.WriteLine($"✅ Database'den bulundu: {e.LocalizedMessage}");
            }
            else
            {
                Console.WriteLine($"❌ Database'de bulunamadı");
            }
        }
    };
});
```

## 🧪 Test Senaryoları

### Test 1: FluentValidation Hatası
```bash
# İstek
curl -X POST "https://localhost:5001/api/users" \
  -H "X-Language: tr-TR" \
  -H "Content-Type: application/json" \
  -d '{"email": "", "name": "a"}'

# Event Tetiklenir
MessageKey: ApiResponse.ValidationFailed
Culture: tr-TR
DefaultMessage: Validation failed. Please check your input data.

# Event Handler Response
LocalizedMessage: Doğrulama hatası! Formunuzu kontrol edin.

# Final API Response
{
  "isSuccess": false,
  "data": null,
  "message": "Doğrulama hatası! Formunuzu kontrol edin.",
  "errors": [
    "Email: E-posta adresi gerekli",
    "Name: İsim en az 3 karakter olmalı"
  ]
}
```

### Test 2: 404 Not Found
```bash
# İstek  
curl -X GET "https://localhost:5001/api/users/999" \
  -H "X-Language: en-US"

# Event Tetiklenir
MessageKey: ApiResponse.NotFound
Culture: en-US
DefaultMessage: The requested resource was not found

# Event Handler Response
LocalizedMessage: The resource you're looking for was not found!

# Final API Response
{
  "isSuccess": false,
  "data": null,
  "message": "The resource you're looking for was not found!",
  "errors": ["404 - Resource Not Found"]
}
```

### Test 3: JSON Parse Error (Format Parameters)
```bash
# İstek
curl -X POST "https://localhost:5001/api/data" \
  -H "X-Language: tr-TR" \
  -H "Content-Type: application/json" \
  -d '{ invalid json }'

# Event Tetiklenir
MessageKey: ApiResponse.JsonParseError
Culture: tr-TR
DefaultMessage: JSON Parse Error: {0}
FormatArgs: ["Unexpected character at position 2"]

# Event Handler Response (format uygulanır)
LocalizedMessage: JSON Hata: Unexpected character at position 2

# Final API Response
{
  "isSuccess": false,
  "data": null,
  "message": "Yanıt işlenirken bir hata oluştu",
  "errors": ["JSON Hata: Unexpected character at position 2"]
}
```

## 🔧 Debug ve Troubleshooting

### Event Çalışıyor mu Kontrol Et
```csharp
options.OnMessageLocalization += (sender, e) =>
{
    // Event çalışıp çalışmadığını kontrol et
    Console.WriteLine("🔥 EVENT TETİKLENDİ!");
    Console.WriteLine($"   Time: {DateTime.Now:HH:mm:ss.fff}");
    Console.WriteLine($"   Thread: {Thread.CurrentThread.ManagedThreadId}");
    Console.WriteLine($"   Sender: {sender?.GetType().Name}");
    Console.WriteLine($"   MessageKey: {e.MessageKey}");
    Console.WriteLine($"   Culture: {e.Culture}");
    Console.WriteLine($"   HttpContext null?: {e.HttpContext == null}");
    Console.WriteLine($"   Request Path: {e.HttpContext?.Request?.Path}");
    
    // Basit test mesajı
    e.LocalizedMessage = $"TEST MESSAGE: {e.MessageKey} - {e.Culture}";
    
    Console.WriteLine($"   Set LocalizedMessage: {e.LocalizedMessage}");
    Console.WriteLine("🔥 EVENT COMPLETED!");
};
```

### Hangi Header'lar Geliyor?
```csharp
options.OnMessageLocalization += (sender, e) =>
{
    Console.WriteLine("📨 HEADERS:");
    if (e.HttpContext?.Request?.Headers != null)
    {
        foreach (var header in e.HttpContext.Request.Headers)
        {
            Console.WriteLine($"   {header.Key}: {header.Value}");
        }
    }
    
    // Kültür öncelik sırası test
    var xLanguage = e.HttpContext?.Request?.Headers["X-Language"];
    var acceptLanguage = e.HttpContext?.Request?.Headers["Accept-Language"];
    var queryCulture = e.HttpContext?.Request?.Query["culture"];
    
    Console.WriteLine($"📍 CULTURE SOURCES:");
    Console.WriteLine($"   X-Language: {xLanguage}");
    Console.WriteLine($"   Accept-Language: {acceptLanguage}");
    Console.WriteLine($"   Query culture: {queryCulture}");
    Console.WriteLine($"   Final Culture: {e.Culture}");
};
```

## 🚀 Production Önerileri

### 1. Performance İçin Cache Kullanın
```csharp
private static readonly MemoryCache _localizationCache = new MemoryCache(new MemoryCacheOptions());

options.OnMessageLocalization += (sender, e) =>
{
    var cacheKey = $"{e.MessageKey}_{e.Culture}";
    
    if (_localizationCache.TryGetValue(cacheKey, out string cachedMessage))
    {
        e.LocalizedMessage = cachedMessage;
        return;
    }
    
    // Database'den al
    var localizedMessage = GetFromDatabase(e.MessageKey, e.Culture);
    
    if (!string.IsNullOrEmpty(localizedMessage))
    {
        _localizationCache.Set(cacheKey, localizedMessage, TimeSpan.FromMinutes(30));
        e.LocalizedMessage = localizedMessage;
    }
};
```

### 2. Error Handling Ekleyin
```csharp
options.OnMessageLocalization += (sender, e) =>
{
    try
    {
        // Localization logic
        var localizedMessage = GetLocalizedMessage(e.MessageKey, e.Culture);
        e.LocalizedMessage = localizedMessage;
    }
    catch (Exception ex)
    {
        // Log error ama event'i break etme
        Console.WriteLine($"❌ Localization error: {ex.Message}");
        // e.LocalizedMessage null kalır, default kullanılır
    }
};
```

### 3. Async Operations için
```csharp
options.OnMessageLocalization += async (sender, e) =>
{
    try
    {
        using var scope = e.HttpContext?.RequestServices?.CreateScope();
        var dbContext = scope?.ServiceProvider?.GetService<MyDbContext>();
        
        if (dbContext != null)
        {
            var localizedMessage = await dbContext.LocalizedMessages
                .Where(x => x.Key == e.MessageKey && x.Culture == e.Culture)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
                
            e.LocalizedMessage = localizedMessage;
        }
    }
    catch (Exception ex)
    {
        // Silent fail - default message kullanılır
        Console.WriteLine($"Database localization failed: {ex.Message}");
    }
};
```
