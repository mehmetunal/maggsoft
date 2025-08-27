# ApiResponseMiddleware Mesaj Yapılandırması

## Varsayılan Kullanım

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddleware();
```

## Event ile Dış Proje Localization Kullanımı

### 1. Temel Event Handler Kurulumu

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.UseCamelCase = true;
    options.IgnoreAcceptHeader = ["image/", "pdf/"];
    
    // Event handler'ı bağla - Dış projenin kendi localization sistemini kullanır
    options.OnMessageLocalization += (sender, e) =>
    {
        // Kültür bilgisini al (X-Language, Accept-Language, query parameter sırasıyla)
        var culture = e.Culture ?? "en";
        
        // Kendi localization servisinizi kullanın
        var localizer = e.HttpContext?.RequestServices?.GetService<IStringLocalizer>();
        
        if (localizer != null)
        {
            var localizedString = localizer[e.MessageKey];
            if (!localizedString.ResourceNotFound)
            {
                e.LocalizedMessage = e.FormatArgs?.Length > 0 
                    ? string.Format(localizedString.Value, e.FormatArgs) 
                    : localizedString.Value;
            }
        }
    };
});
```

### 2. IStringLocalizer ile Event Handler

```csharp
// Program.cs
builder.Services.AddLocalization();
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += HandleMessageLocalization;
});

// Event handler method
private static void HandleMessageLocalization(object sender, MessageLocalizationEventArgs e)
{
    var localizer = e.HttpContext?.RequestServices?.GetService<IStringLocalizer<ApiResponseMiddleware>>();
    
    if (localizer != null)
    {
        var localizedString = localizer[e.MessageKey];
        if (!localizedString.ResourceNotFound)
        {
            e.LocalizedMessage = e.FormatArgs?.Length > 0 
                ? string.Format(localizedString.Value, e.FormatArgs) 
                : localizedString.Value;
        }
    }
}
```

### 3. Kendi Dictionary-Based Localization

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += (sender, e) =>
    {
        var culture = e.Culture ?? "en";
        var messages = GetLocalizedMessages(culture);
        
        if (messages.TryGetValue(e.MessageKey, out var localizedMessage))
        {
            e.LocalizedMessage = e.FormatArgs?.Length > 0 
                ? string.Format(localizedMessage, e.FormatArgs) 
                : localizedMessage;
        }
    };
});

private static Dictionary<string, string> GetLocalizedMessages(string culture)
{
    return culture.ToLower() switch
    {
        "tr" or "tr-tr" => new Dictionary<string, string>
        {
            { ApiResponseMessages.KEY_ValidationFailed, "Doğrulama hatası!" },
            { ApiResponseMessages.KEY_BadRequest, "İstek hatalı" },
            { ApiResponseMessages.KEY_NotFound, "Kaynak bulunamadı" },
            { ApiResponseMessages.KEY_InternalServerError, "Sistem hatası" }
        },
        "de" or "de-de" => new Dictionary<string, string>
        {
            { ApiResponseMessages.KEY_ValidationFailed, "Validierung fehlgeschlagen!" },
            { ApiResponseMessages.KEY_BadRequest, "Anfrage fehlerhaft" },
            { ApiResponseMessages.KEY_NotFound, "Ressource nicht gefunden" }
        },
        _ => new Dictionary<string, string>() // Fallback to default
    };
}
```

### 4. Database'den Localization

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.OnMessageLocalization += async (sender, e) =>
    {
        var dbContext = e.HttpContext?.RequestServices?.GetService<YourDbContext>();
        if (dbContext != null)
        {
            var culture = e.Culture ?? "en";
            var localizedMessage = await dbContext.LocalizedMessages
                .Where(x => x.Key == e.MessageKey && x.Culture == culture)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
                
            if (!string.IsNullOrEmpty(localizedMessage))
            {
                e.LocalizedMessage = e.FormatArgs?.Length > 0 
                    ? string.Format(localizedMessage, e.FormatArgs) 
                    : localizedMessage;
            }
        }
    };
});
```

## Özelleştirilmiş Mesajlarla Kullanım (Fallback)

Event handler mevcut değilse bu mesajlar kullanılır:

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    // Event yok ise bu mesajlar kullanılır
    options.Messages.ValidationFailed = "Doğrulama başarısız!";
    options.Messages.BadRequest = "İsteğiniz işlenemedi";
    options.Messages.NotFound = "İstenen kaynak bulunamadı";
    options.Messages.InternalServerError = "Sistem hatası oluştu";
});
```

## İngilizce Mesajlarla Kullanım

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.UseCamelCase = true;
    
    // İngilizce mesajlar (varsayılan değerler zaten İngilizce)
    options.Messages.ValidationFailed = "Validation failed. Please check your input data.";
    options.Messages.BadRequest = "Your request could not be processed";
    options.Messages.NotFound = "The requested resource was not found";
    options.Messages.InternalServerError = "An internal error occurred. Please try again later";
    
    // Teknik mesajlar
    options.Messages.TechnicalBadRequest = "400 - Bad Request";
    options.Messages.TechnicalNotFound = "404 - Not Found";
});
```

## Sadece Belirli Mesajları Değiştirme

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.IgnoreAcceptHeader = ["image/", "txt"];
    options.UseCamelCase = true; // JSON property'leri camelCase formatında döndürülecek
    // Sadece validation mesajlarını Türkçe yap
    options.Messages.ValidationFailed = "Doğrulama hatası! Lütfen formu kontrol edin.";
    options.Messages.BadRequest = "İsteğiniz işlenemedi";
    options.Messages.TechnicalBadRequest = "400 - Hatalı İstek";
    
    // Diğer mesajlar varsayılan değerlerinde kalır
});
```

## Çoklu Dil Desteği için Configuration'dan Okuma

```csharp
// appsettings.json
{
  "ApiResponseMessages": {
    "ValidationFailed": "Validation failed. Please check your input data.",
    "AnErrorOccurred": "An error occurred",
    "BadRequest": "Your request could not be processed",
    "NotFound": "The requested resource was not found"
  }
}

// Program.cs
builder.Services.Configure<IgnoreResponseOption>(options =>
{
    var config = builder.Configuration.GetSection("ApiResponseMessages");
    options.Messages.ValidationFailed = config["ValidationFailed"] ?? options.Messages.ValidationFailed;
    options.Messages.AnErrorOccurred = config["AnErrorOccurred"] ?? options.Messages.AnErrorOccurred;
    options.Messages.BadRequest = config["BadRequest"] ?? options.Messages.BadRequest;
    options.Messages.NotFound = config["NotFound"] ?? options.Messages.NotFound;
    // Diğer mesajlar için de aynı şekilde...
});

builder.Services.AddGlobalResponseMiddleware();
```

## Mesaj Kategorileri

### Ana Hata Mesajları
- `ValidationFailed`: Doğrulama hatalarında gösterilir
- `AnErrorOccurred`: Tekil hata durumlarında
- `MultipleErrorsOccurred`: Çoklu hata durumlarında
- `ResponseProcessingError`: Yanıt işleme hatalarında
- `RequestProcessingError`: İstek işleme hatalarında

### Geliştirici Mesajları
- `JsonParseError`: JSON parse hatalarında (Development)
- `ProblemDetailsParseError`: ProblemDetails parse hatalarında (Development)
- `ResponseValidationFailed`: Yanıt doğrulama hatalarında (Production)
- `ErrorDetailsProcessingFailed`: Hata detayı işleme hatalarında (Production)

### HTTP Status Code Mesajları (Kullanıcı Dostu)
- `BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, vb.

### HTTP Status Code Mesajları (Teknik)
- `TechnicalBadRequest`, `TechnicalUnauthorized`, `TechnicalForbidden`, vb.

## Kullanılabilir Mesaj Tipleri

### Ana Hata Mesajları
- `ValidationFailed`: Doğrulama hatalarında gösterilir
- `AnErrorOccurred`: Tekil hata durumlarında
- `MultipleErrorsOccurred`: Çoklu hata durumlarında
- `ResponseProcessingError`: Yanıt işleme hatalarında
- `JsonParseError`: JSON parse hatalarında (format: {0} parametreli)
- `ResponseValidationFailed`: Yanıt doğrulama hatalarında
- `RequestProcessingError`: İstek işleme hatalarında
- `ProblemDetailsParseError`: ProblemDetails parse hatalarında (format: {0} parametreli)
- `ErrorDetailsProcessingFailed`: Hata detayı işleme hatalarında

### HTTP Status Code Mesajları (Kullanıcı Dostu)
- `BadRequest` (400)
- `Unauthorized` (401) 
- `Forbidden` (403)
- `NotFound` (404)
- `MethodNotAllowed` (405)
- `Conflict` (409)
- `UnprocessableEntity` (422)
- `TooManyRequests` (429)
- `InternalServerError` (500)
- `NotImplemented` (501)
- `BadGateway` (502)
- `ServiceUnavailable` (503)
- `GatewayTimeout` (504)
- `DefaultError` (diğer status kodları)

### HTTP Status Code Mesajları (Teknik)
- `TechnicalBadRequest` (400)
- `TechnicalUnauthorized` (401)
- `TechnicalForbidden` (403)
- `TechnicalNotFound` (404)
- `TechnicalMethodNotAllowed` (405)
- `TechnicalConflict` (409)
- `TechnicalUnprocessableEntity` (422)
- `TechnicalTooManyRequests` (429)
- `TechnicalInternalServerError` (500)
- `TechnicalNotImplemented` (501)
- `TechnicalBadGateway` (502)
- `TechnicalServiceUnavailable` (503)
- `TechnicalGatewayTimeout` (504)
- `TechnicalDefaultError` (diğer status kodları, format: {0} parametreli)

## Örnek Response Formatları

### Event ile Türkçe Localization (X-Language: tr-TR)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Doğrulama başarısız. Lütfen giriş verilerinizi kontrol edin.",
  "errors": [
    "Email: E-posta adresi gerekli",
    "Name: İsim en az 3 karakter olmalı"
  ]
}
```

### Event ile İngilizce Localization (X-Language: en-US)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "The requested resource was not found",
  "errors": [
    "Not Found - The requested resource was not found"
  ]
}
```

### Event ile Almanca Localization (X-Language: de-DE)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Die angeforderte Ressource wurde nicht gefunden",
  "errors": [
    "Nicht gefunden - Die angeforderte Ressource wurde nicht gefunden"
  ]
}
```

### Fallback Chain Örneği
```
Request Headers:
X-Language: tr-TR          ← En yüksek öncelik (kullanılır)
Accept-Language: en-US     ← İkinci öncelik  
X-Culture: de-DE          ← En düşük öncelik

Sonuç: tr-TR kültürü kullanılır
```

### Event Handler Yok ise Fallback Mesaj
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Validation failed. Please check your input data.",
  "errors": [
    "Bad Request - The request was invalid or malformed"
  ]
}
```

## Configuration'dan Mesaj Okuma

```csharp
// appsettings.json
{
  "ApiResponseMessages": {
    "ValidationFailed": "Doğrulama başarısız. Lütfen verilerinizi kontrol edin.",
    "BadRequest": "İsteğiniz işlenemedi",
    "NotFound": "Kaynak bulunamadı",
    "InternalServerError": "Sistem hatası oluştu"
  }
}

// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    var messageConfig = builder.Configuration.GetSection("ApiResponseMessages");
    
    options.Messages.ValidationFailed = messageConfig["ValidationFailed"] ?? options.Messages.ValidationFailed;
    options.Messages.BadRequest = messageConfig["BadRequest"] ?? options.Messages.BadRequest;
    options.Messages.NotFound = messageConfig["NotFound"] ?? options.Messages.NotFound;
    options.Messages.InternalServerError = messageConfig["InternalServerError"] ?? options.Messages.InternalServerError;
});
```

## Dil Header'ı Kullanım Örnekleri

Middleware kültür bilgisini şu sırayla arar:
1. **X-Language** header (öncelik 1)
2. **Accept-Language** header (öncelik 2) 
3. **culture** query parameter (öncelik 3)
4. **X-Culture** header (öncelik 4)

### JavaScript ile X-Language Header Kullanımı

```javascript
// X-Language header ile (öncelik 1)
fetch('/api/users', {
  headers: {
    'X-Language': 'tr-TR', // Türkçe
    'Content-Type': 'application/json'
  }
});

fetch('/api/users', {
  headers: {
    'X-Language': 'en-US', // İngilizce
    'Content-Type': 'application/json'
  }
});

fetch('/api/users', {
  headers: {
    'X-Language': 'de-DE', // Almanca
    'Content-Type': 'application/json'
  }
});
```

### C# HttpClient ile X-Language Header

```csharp
using var httpClient = new HttpClient();

// Türkçe için
httpClient.DefaultRequestHeaders.Add("X-Language", "tr-TR");
var response = await httpClient.GetAsync("https://api.example.com/users");

// İngilizce için
httpClient.DefaultRequestHeaders.Clear();
httpClient.DefaultRequestHeaders.Add("X-Language", "en-US");
var response2 = await httpClient.GetAsync("https://api.example.com/users");
```

### Angular HttpInterceptor ile Otomatik X-Language

```typescript
@Injectable()
export class LanguageInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const language = localStorage.getItem('selectedLanguage') || 'en-US';
    
    const modifiedReq = req.clone({
      setHeaders: {
        'X-Language': language
      }
    });
    
    return next.handle(modifiedReq);
  }
}
```

### Axios ile X-Language Header

```javascript
// Axios interceptor ile otomatik ekleme
axios.interceptors.request.use((config) => {
  const language = localStorage.getItem('language') || 'en-US';
  config.headers['X-Language'] = language;
  return config;
});

// Tek seferlik kullanım
const response = await axios.get('/api/users', {
  headers: {
    'X-Language': 'tr-TR'
  }
});
```

## Dinamik Mesaj Değiştirme

```csharp
// Runtime'da mesaj değiştirme
public class MessageService
{
    private readonly IOptionsMonitor<IgnoreResponseOption> _options;
    
    public MessageService(IOptionsMonitor<IgnoreResponseOption> options)
    {
        _options = options;
    }
    
    public void UpdateMessages(string language)
    {
        var currentOptions = _options.CurrentValue;
        
        if (language == "tr")
        {
            currentOptions.Messages.ValidationFailed = "Doğrulama hatası!";
            currentOptions.Messages.NotFound = "Kaynak bulunamadı";
        }
        else
        {
            currentOptions.Messages.ValidationFailed = "Validation failed!";
            currentOptions.Messages.NotFound = "Resource not found";
        }
    }
}
```
