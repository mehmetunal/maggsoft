# ApiResponseMiddleware Mesaj Yapılandırması

## Varsayılan Kullanım

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddleware();
```

## Özelleştirilmiş Mesajlarla Kullanım

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
    options.UseCamelCase = true;
    options.IgnoreAcceptHeader = ["image/", "pdf/"];
    
    // Mesajları özelleştir
    options.Messages.ValidationFailed = "Doğrulama başarısız. Lütfen giriş verilerinizi kontrol edin.";
    options.Messages.AnErrorOccurred = "Bir hata oluştu";
    options.Messages.MultipleErrorsOccurred = "Birden fazla hata oluştu";
    options.Messages.ResponseProcessingError = "Yanıt işlenirken bir hata oluştu";
    options.Messages.RequestProcessingError = "İsteğiniz işlenirken bir hata oluştu";
    
    // HTTP Status Code Mesajları - Kullanıcı Dostu
    options.Messages.BadRequest = "İsteğiniz işlenemedi";
    options.Messages.Unauthorized = "Bu kaynağa erişim için kimlik doğrulama gerekli";
    options.Messages.Forbidden = "Bu kaynağa erişim izniniz yok";
    options.Messages.NotFound = "İstenen kaynak bulunamadı";
    options.Messages.InternalServerError = "Dahili bir hata oluştu. Lütfen daha sonra tekrar deneyin";
    
    // HTTP Status Code Mesajları - Teknik
    options.Messages.TechnicalBadRequest = "Hatalı İstek - İstek geçersiz veya kusurlu";
    options.Messages.TechnicalUnauthorized = "Yetkisiz - Kimlik doğrulama gerekli";
    options.Messages.TechnicalForbidden = "Yasak - Erişim reddedildi";
    options.Messages.TechnicalNotFound = "Bulunamadı - İstenen kaynak bulunamadı";
    options.Messages.TechnicalInternalServerError = "Dahili Sunucu Hatası - Beklenmeyen bir hata oluştu";
});
```

## Sadece Belirli Mesajları Değiştirme

```csharp
// Program.cs
builder.Services.AddGlobalResponseMiddlewareWithOptions(options =>
{
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

## Örnek Response Formatları

### Validation Hatası
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

### 404 Hatası
```json
{
  "isSuccess": false,
  "data": null,
  "message": "İstenen kaynak bulunamadı",
  "errors": [
    "Bulunamadı - İstenen kaynak bulunamadı"
  ]
}
```

### Development Ortamında JSON Parse Hatası
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Yanıt işlenirken bir hata oluştu",
  "errors": [
    "JSON Parse Error: Unexpected character at position 15"
  ]
}
```
