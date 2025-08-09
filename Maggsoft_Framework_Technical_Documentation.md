# Maggsoft Framework - Teknik Dokümantasyon

## Genel Bakış

Maggsoft Framework, ASP.NET Core 8 tabanlı modern web uygulamaları geliştirmek için tasarlanmış kapsamlı bir framework'tür. Clean Architecture prensiplerini takip eden, modüler yapıda ve çoklu veritabanı desteği sunan bir çözümdür.

### Framework'ün Ana Amaçları

1. **Hızlı Geliştirme**: Hazır bileşenler ve extension'lar ile hızlı proje geliştirme
2. **Modüler Yapı**: Bağımsız ve yeniden kullanılabilir modüller
3. **Çoklu Veritabanı Desteği**: MSSQL, PostgreSQL, SQLite, MongoDB desteği
4. **Event-Driven Architecture**: Mikroservis mimarisi için event bus desteği
5. **Caching**: Memory ve Redis cache desteği
6. **AOP (Aspect-Oriented Programming)**: Cross-cutting concerns için aspect desteği
7. **Clean Architecture**: Katmanlı mimari ve dependency injection

---

## 1. Core Katmanı (Maggsoft.Core)

### 1.1 Base Sınıfları

#### Result<T> ve Result Sınıfları

API yanıtlarını standartlaştırmak için kullanılan generic result sınıfları.

```csharp
// Başarılı yanıt örneği
var result = Result<User>.Success(user, SuccessMessage.None);

// Hata yanıtı örneği
var errorResult = Result<User>.Failure(Error.None);

// Kullanım örneği
public async Task<Result<User>> GetUserByIdAsync(int id)
{
    var user = await _userRepository.FindByIdAsync(id);
    if (user == null)
        return Result<User>.Failure(new Error("400", "Kullanıcı bulunamadı"));
    
    return Result<User>.Success(user, new SuccessMessage("200", "Kullanıcı başarıyla getirildi"));
}
```

#### Error ve SuccessMessage

Hata ve başarı mesajlarını standartlaştırmak için kullanılan record'lar.

```csharp
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public sealed record SuccessMessage(string Code, string Description)
{
    public static readonly SuccessMessage None = new(string.Empty, string.Empty);
}
```

### 1.2 Repository Pattern

#### IRepository<T> Interface

Generic repository pattern implementasyonu.

```csharp
public interface IRepository<T> where T : IEntity
{
    // READ Operations
    IQueryable<T> Get();
    Task<IEnumerable<T>> GetAsync();
    T Find(Expression<Func<T, bool>> where);
    Task<T> FindAsync(Expression<Func<T, bool>> where);
    
    // WRITE Operations
    T Add(T entity);
    Task<T> AddAsync(T entity);
    T Update(T entity);
    Task<T> UpdateAsync(T entity);
    T Delete(T entity);
    Task<T> DeleteAsync(T entity);
    
    // Utility Operations
    int Count();
    bool Any();
    Task<int> CountAsync();
    Task<bool> AnyAsync();
}
```

### 1.3 Extension Methods

Framework'te birçok extension method bulunmaktadır:

#### StringExtensions
```csharp
// String işlemleri için extension'lar
var slug = "Merhaba Dünya".ToSlug(); // merhaba-dunya
var isEmail = "test@example.com".IsEmail(); // true
var isPhone = "+905551234567".IsPhone(); // true
```

#### DateTimeExtensions
```csharp
// Tarih işlemleri için extension'lar
var startOfWeek = DateTime.Now.StartOfWeek();
var endOfMonth = DateTime.Now.EndOfMonth();
var isWeekend = DateTime.Now.IsWeekend();
```

#### EnumerableExtensions
```csharp
// Koleksiyon işlemleri için extension'lar
var distinctBy = users.DistinctBy(x => x.Email);
var chunked = users.Chunk(10);
```

---

## 2. Data Katmanı

### 2.1 Base Entity

Tüm entity'ler için temel sınıf.

```csharp
public interface IBaseEntity<TKey> : IBaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    TKey Id { get; set; }

    [Required]
    DateTime CreatedDate { get; set; }

    [Required]
    [StringLength(50)]
    string CreatorIP { get; set; }

    [Required]
    Guid CreatorUserId { get; set; }

    DateTime? UpdatedDate { get; set; }
    string UpdatedIP { get; set; }
    Guid? UpdatedByUserId { get; set; }
}
```

### 2.2 Veritabanı Desteği

#### MSSQL Desteği (Maggsoft.Mssql)
```csharp
// Startup.cs'de konfigürasyon
services.AddMssqlConfig<AppContext>(Configuration)
    .AddFluentMigratorConfig(Configuration);

// Repository kullanımı
services.AddScoped<IMssqlRepository<User>, Repository<User>>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

#### PostgreSQL Desteği (Maggsoft.Npgsql)
```csharp
// Startup.cs'de konfigürasyon
services.AddNpgsqlConfig<AppContext>(Configuration);

// Repository kullanımı
services.AddScoped<INpgsqlRepository<User>, NpgsqlRepository<User>>();
```

#### SQLite Desteği (Maggsoft.Sqlite)
```csharp
// Startup.cs'de konfigürasyon
services.AddSqliteConfig<AppContext>(Configuration);

// Repository kullanımı
services.AddScoped<ISqliteRepository<User>, SqliteRepository<User>>();
```

#### MongoDB Desteği (Maggsoft.Mongo)
```csharp
// Startup.cs'de konfigürasyon
services.AddMongoConfig(Configuration);

// Repository kullanımı
services.AddScoped<IMongoRepository<User>, MongoRepository<User>>();
```

---

## 3. Cache Katmanı

### 3.1 Memory Cache (Maggsoft.Cache.MemoryCache)

```csharp
// Startup.cs'de konfigürasyon
services.AddMaggsoftDistributedMemoryCache(typeof(IService));

// Kullanım örneği
public class UserService
{
    private readonly ICache _cache;
    
    public UserService(ICache cache)
    {
        _cache = cache;
    }
    
    public async Task<User> GetUserAsync(int id)
    {
        var cacheKey = $"user_{id}";
        
        return await _cache.GetAsync<User>(cacheKey, TimeSpan.FromMinutes(30), async () =>
        {
            // Veritabanından kullanıcı getir
            return await _userRepository.FindByIdAsync(id);
        });
    }
    
    public async Task SetUserAsync(User user)
    {
        var cacheKey = $"user_{user.Id}";
        await _cache.SetAsync(cacheKey, TimeSpan.FromMinutes(30), true, user);
    }
    
    public async Task RemoveUserAsync(int id)
    {
        var cacheKey = $"user_{id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

### 3.2 Redis Cache (Maggsoft.Cache.Redis)

```csharp
// Startup.cs'de konfigürasyon
services.AddMaggsoftDistributedRedisCache(Configuration);

// Kullanım Memory Cache ile aynıdır
```

---

## 4. Event Bus Sistemi

### 4.1 Event Tanımlama

```csharp
// Integration Event
public class UserCreatedEvent : IntegrationEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

// Event Handler
public class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;
    
    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(UserCreatedEvent @event)
    {
        _logger.LogInformation($"Yeni kullanıcı oluşturuldu: {@event.Email}");
        
        // Email gönderme, bildirim gönderme vb. işlemler
        await SendWelcomeEmailAsync(@event.Email);
    }
}
```

### 4.2 Event Publishing

```csharp
public class UserService
{
    private readonly IEventPublisher _eventPublisher;
    
    public UserService(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        // Kullanıcı oluştur
        var createdUser = await _userRepository.AddAsync(user);
        
        // Event yayınla
        var userCreatedEvent = new UserCreatedEvent
        {
            UserId = createdUser.Id,
            Email = createdUser.Email,
            Name = createdUser.Name
        };
        
        await _eventPublisher.PublishAsync(userCreatedEvent);
        
        return createdUser;
    }
}
```

### 4.3 Event Bus Konfigürasyonu

```csharp
// Startup.cs'de
services.AddEventBus(Configuration);

// Event handler'ları kaydet
services.RegisterEventConsumer();
```

---

## 5. Aspect-Oriented Programming (AOP)

### 5.1 Aspect Tanımlama

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class LoggingAspectAttribute : AspectAttribute
{
    public override IAspect CreateAspect()
    {
        return new LoggingAspect();
    }
}

public class LoggingAspect : IAspect
{
    private ILogger _logger;
    
    public AspectAttribute LoadDependencies(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger>();
        return new LoggingAspectAttribute();
    }
    
    public void OnBefore(MethodExecutionArgs args)
    {
        _logger.LogInformation($"Metod başlatıldı: {args.MethodName}");
    }
    
    public void OnAfter(MethodExecutionArgs args)
    {
        _logger.LogInformation($"Metod tamamlandı: {args.MethodName}");
    }
    
    public void OnException(MethodExecutionArgs args)
    {
        _logger.LogError($"Metod hatası: {args.MethodName}, Hata: {args.Exception.Message}");
    }
    
    public void OnSuccess(MethodExecutionArgs args)
    {
        _logger.LogInformation($"Metod başarılı: {args.MethodName}");
    }
}
```

### 5.2 Aspect Kullanımı

```csharp
public class UserService
{
    [LoggingAspect]
    public async Task<User> GetUserAsync(int id)
    {
        return await _userRepository.FindByIdAsync(id);
    }
    
    [LoggingAspect]
    [CacheAspect]
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAsync();
    }
}
```

---

## 6. Framework Katmanı (Maggsoft.Framework)

### 6.1 Infrastructure Konfigürasyonu

Framework'te tüm konfigürasyonları tek seferde yapmak için `AddInfrastructure` extension method'u bulunmaktadır:

```csharp
// Program.cs'de veya Startup.cs'de
services.AddInfrastructure(configuration);

// Bu method şu konfigürasyonları yapar:
// - Controllers ve JSON options
// - CORS konfigürasyonu
// - API Versioning
// - Swagger
// - Service registration
// - Exception handler
// - Problem details
// - Response compression
// - Global response middleware
// - Model state response factory

// WebApplication için de extension method:
app.AddInfrastructure();
```

### 6.2 Middleware'ler

#### Exception Middleware

Global exception handling için kullanılır.

```csharp
// Startup.cs'de konfigürasyon
app.UseExceptionHandler("/Error");

// Program.cs'de
builder.Services.AddExceptionHandler<ExceptionMiddleware>();
```

#### API Response Middleware

API yanıtlarını standartlaştırmak için kullanılır.

```csharp
// Startup.cs'de
app.UseMiddleware<ApiResponseMiddleware>();
```

#### IP Filter Middleware

IP bazlı erişim kontrolü için kullanılır.

```csharp
// Startup.cs'de
app.UseMiddleware<IPFilterMiddleware>();
```

### 6.2 Security

#### JWT Authentication

```csharp
// Startup.cs'de konfigürasyon
services.AddJwtAuthentication(Configuration);

// Controller'da kullanım
[Authorize]
[ApiController]
public class UserController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Implementation
    }
}
```

### 6.3 CORS Konfigürasyonu

```csharp
// Startup.cs'de CORS konfigürasyonu
services.AddAdminApiCors(TokenOptions); // TokenOptions ile CORS ayarları
app.UseCorsConfig();

// AddAdminApiCors method'u şu ayarları yapar:
// - TokenOptions.CorsAllowAnyOrigin = true ise AllowAnyOrigin()
// - TokenOptions.CorsAllowOrigins array'i ile özel origin'ler
// - SetIsOriginAllowed((host) => true)
// - AllowAnyHeader()
// - AllowAnyMethod()
// - AccessControlAllowHeaders: "Content-Type"

// UseCorsConfig method'u şu CORS ayarlarını yapar:
// - AllowAnyHeader()
// - AllowAnyMethod()
// - SetIsOriginAllowed((host) => true)
// - AllowCredentials()
```

### 6.4 API Versioning

```csharp
// Startup.cs'de API Versioning konfigürasyonu
services.AddApiVersioningConfig(configuration);

// Bu extension method API versioning için gerekli servisleri ekler
// appsettings.json'da version bilgileri:
{
  "ApiVersion": {
    "MajorVersion": "1",
    "MinorVersion": "0"
  }
}
```

### 6.5 Swagger Konfigürasyonu

```csharp
// Startup.cs'de
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Maggsoft API", Version = "v1" });
    
    // JWT authentication için
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});
```

### 6.6 FluentValidation Auto Validation

```csharp
// Startup.cs'de FluentValidation konfigürasyonu
services.AddFluentValidationAutoValidation();

// Validator örneği
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MaximumLength(100).WithMessage("İsim 100 karakterden uzun olamaz");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");
    }
}
```

### 6.7 Problem Details

```csharp
// Startup.cs'de Problem Details konfigürasyonu
services.AddProblemDetails();

// Bu servis, HTTP hata yanıtlarını RFC 7807 Problem Details formatında döndürür
// Örnek yanıt:
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Email": ["Email alanı zorunludur"]
    }
}
```

### 6.8 Global Response Middleware

```csharp
// Startup.cs'de Global Response Middleware konfigürasyonu
services.AddGlobalResponseMiddlewareWithOptions(p => p.IgnoreAcceptHeader = ["image/", "txt"]);

// Bu middleware, tüm API yanıtlarını standart Result<T> formatına dönüştürür
// IgnoreAcceptHeader ile belirtilen content type'lar için dönüşüm yapılmaz

// Örnek kullanım:
public class IgnoreResponseOption
{
    public string[] IgnoreAcceptHeader { get; set; } = ["image/"];
}
```

### 6.9 Exception Handler

```csharp
// Program.cs'de Exception Handler konfigürasyonu
builder.Services.AddExceptionHandler<ExceptionMiddleware>();

// Bu servis, global exception handling için kullanılır
// Tüm yakalanmamış exception'ları yakalar ve standart formatta yanıt döner
```

### 6.10 JSON Konfigürasyonu

```csharp
// Startup.cs'de JSON konfigürasyonu
services.AddControllers().AddJsonOptionsConfig();

// Bu extension method şu ayarları yapar:
// - PropertyNameCaseInsensitive = true
// - PropertyNamingPolicy = null
// - AllowTrailingCommas = true
// - DecimalToStringConverter ekler
// - TimeSpanConverter ekler

// Custom JSON Converter'lar:
public class DecimalToStringConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        // Türkçe kültür ayarları ile decimal parsing
        var cultureInfo = CultureInfo.CreateSpecificCulture("tr-TR");
        // Implementation...
    }
    
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
```

### 6.11 Response Compression

```csharp
// Startup.cs'de Response Compression konfigürasyonu
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
});

services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});
```

### 6.12 Model State Response Factory

```csharp
// Startup.cs'de Model State Response Factory konfigürasyonu
services.Configure<ApiBehaviorOptions>(options => 
{ 
    options.InvalidModelStateResponseFactory = ctx => new ModelStateFeatureFilter(); 
});

// Bu factory, validation hatalarını standart Result formatında döner
public class ModelStateFeatureFilter : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        var modelState = context.ModelState.GetErrorMessages();
        throw new ModelStateException(JsonConvert.SerializeObject(modelState));
    }
}
```

### 6.13 IP Filter Middleware

```csharp
// Startup.cs'de IP Filter konfigürasyonu
services.Configure<IPFilterOptions>(configuration.GetSection("IPFilter"));

// appsettings.json'da IP Filter ayarları:
{
  "IPFilter": {
    "WhitelistedIPs": ["192.168.1.1", "10.0.0.1"],
    "BlockedIPs": ["192.168.1.100"],
    "AllowedIPRanges": ["192.168.1.0/24"],
    "BlockedIPRanges": ["10.0.0.0/8"],
    "MaxRequestsPerMinute": 100,
    "DefaultAllow": true,
    "ExemptPaths": ["/health", "/metrics"]
  }
}

// Middleware kullanımı
app.UseMiddleware<IPFilterMiddleware>();

// IP Filter Options sınıfı:
public class IPFilterOptions
{
    public List<string> WhitelistedIPs { get; set; } = new();
    public List<string> BlockedIPs { get; set; } = new();
    public List<string> AllowedIPRanges { get; set; } = new();
    public List<string> BlockedIPRanges { get; set; } = new();
    public int MaxRequestsPerMinute { get; set; } = 100;
    public bool DefaultAllow { get; set; } = true;
    public List<string> ExemptPaths { get; set; } = new();
}
```

### 6.14 Request Pipeline Konfigürasyonu

```csharp
// Startup.cs'de Request Pipeline konfigürasyonu
app.ConfigureRequestPipeline();

// Bu extension method, MaggsoftContext üzerinden request pipeline'ı konfigüre eder
// Özelleştirilmiş middleware'ler ve pipeline ayarları için kullanılır
```

### 6.15 HTTP Request Extensions

```csharp
// HTTP Request için extension method'lar
public static class HttpRequestExtensions
{
    // Request'in local olup olmadığını kontrol eder
    public static bool IsLocal(this HttpRequest request)
    {
        // Implementation...
    }
    
    // Request'in AJAX olup olmadığını kontrol eder
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        // Implementation...
    }
}
```

### 6.16 API Token Options

```csharp
// appsettings.json'da API Token ayarları:
{
  "TokenOptions": {
    "AccessTokenExpiration": 60,
    "ApiName": "Maggsoft API",
    "ApiVersion": "v1",
    "ApiBaseUrl": "https://localhost:5001",
    "IdentityServerBaseUrl": "https://identity.example.com",
    "OidcSwaggerUIClientId": "maggsoft_api_swaggerui",
    "OidcApiName": "maggsoft_api",
    "AdministrationRole": "Administrator",
    "RequireHttpsMetadata": true,
    "CorsAllowAnyOrigin": true,
    "CorsAllowOrigins": [],
    "IgnoreUrls": [],
    "SecurityKey": "your-secret-key-here"
  }
}

// API Token Options sınıfı:
public class ApiTokenOptions
{
    public string ApiName { get; set; }
    public string ApiVersion { get; set; }
    public string IdentityServerBaseUrl { get; set; }
    public string ApiBaseUrl { get; set; }
    public string OidcSwaggerUIClientId { get; set; }
    public bool RequireHttpsMetadata { get; set; }
    public string OidcApiName { get; set; }
    public string AdministrationRole { get; set; }
    public bool CorsAllowAnyOrigin { get; set; }
    public string[] CorsAllowOrigins { get; set; }
    public int AccessTokenExpiration { get; set; }
    public string SecurityKey { get; set; }
    public string[] IgnoreUrls { get; set; }
}
```

### 6.17 Excel Operations

```csharp
// Excel export işlemleri için helper sınıfı
public static class ExcelOperations
{
    // Generic liste'yi Excel'e export etme
    public static byte[] ExportToExcel<T>(IEnumerable<T> data)
    {
        return ExportToExcel(data, "Sheet1");
    }
    
    // Özel sheet adı ile export
    public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
    {
        // OpenXML kullanarak Excel dosyası oluşturma
        // Implementation...
    }
}

// Controller'da kullanım:
[HttpGet("export")]
public IActionResult ExportUsers()
{
    var users = _userService.GetAllUsers();
    var excelData = ExcelOperations.ExportToExcel(users, "Kullanıcılar");
    
    return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
}
```

### 6.18 API Versioning Error Response Provider

```csharp
// API versioning hatalarını özelleştirmek için
public class ApiVersioningErrorResponseProvider : DefaultErrorResponseProvider
{
    public override IActionResult CreateResponse(ErrorResponseContext context)
    {
        throw new ApiVersioningException(context.Message);
    }
}

// Bu provider, API versioning hatalarını standart exception formatında döner
```

### 6.19 Shell Helpers

```csharp
// Linux/Unix shell komutlarını çalıştırmak için
public static class ShellHelpers
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        if (File.Exists("/bin/bash"))
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
        return string.Empty;
    }
}

// Kullanım örneği:
var result = "ls -la".Bash();
```

### 6.20 Status Code Pages

```csharp
// Startup.cs'de Status Code Pages konfigürasyonu
app.UseStatusCodePages(new StatusCodePagesOptions()
{
    HandleAsync = (ctx) =>
    {
        if (ctx.HttpContext.Response.StatusCode == 404)
        {
            throw new NotFoundException($"Not Found Page");
        }
        return Task.FromResult(0);
    }
});

// Bu konfigürasyon, 404 gibi HTTP hata kodlarını yakalar ve standart exception formatında döner
```

### 6.21 Local Request Filtering

```csharp
// Production ortamında local request'leri engelleme
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.Use(async (context, next) =>
    {
        if (context.Request.IsLocal())
        {
            // Forbidden http status code
            context.Response.StatusCode = 403;
            return;
        }
        await next.Invoke();
    });
}

// Bu middleware, production ortamında local request'leri güvenlik için engeller
```

---

## 7. Services Katmanı

### 7.1 Service Registration

```csharp
// Startup.cs'de
services.RegisterAll<IService>();

// Service interface'i
public interface IUserService : IService
{
    Task<User> GetUserAsync(int id);
    Task<List<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}

// Service implementasyonu
public class UserService : IUserService
{
    private readonly IMssqlRepository<User> _userRepository;
    private readonly ICache _cache;
    private readonly IEventPublisher _eventPublisher;
    
    public UserService(
        IMssqlRepository<User> userRepository,
        ICache cache,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _cache = cache;
        _eventPublisher = eventPublisher;
    }
    
    // Implementation methods...
}
```

---

## 8. Örnek Proje Yapısı

### 8.1 Maggsoft Framework Proje Yapısı

```
maggsoft-framework-project (13 projects)
├── Libraries/                           # Core kütüphaneler (9 projects)
│   ├── Data/                           # Veritabanı katmanı (1 project)
│   │   └── Maggsoft.Data.Mssql/        # MSSQL desteği
│   ├── Dto/                            # DTO katmanı (1 project)
│   │   └── Maggsoft.Dto.Mssql/         # MSSQL DTO'ları
│   ├── Endpoints/                      # Endpoint katmanı (2 projects)
│   │   ├── Maggsoft.Endpoints.Api/     # API Endpoints
│   │   └── Maggsoft.Endpoints.BackgroundServicesApi/ # Background Services API
│   ├── SmsProvider/                    # SMS Provider (1 project)
│   │   └── Maggsoft.SmsProviders/      # SMS servis sağlayıcıları
│   ├── Maggsoft.BackgroundServices/    # Background Services
│   ├── Maggsoft.IdentityManager/       # Identity Management
│   ├── Maggsoft.Mssql/                 # MSSQL Repository
│   └── Maggsoft.Mssql.Services/        # MSSQL Services
├── Presentation/                       # Sunum katmanı (4 projects)
│   ├── Api/                           # API projeleri (2 projects)
│   │   ├── Maggsoft.Api/              # Ana API projesi
│   │   └── Maggsoft.BackgroundServices.API/ # Background Services API
│   └── Web/                           # Web projeleri (2 projects)
│       ├── Maggsoft.Web/              # Ana Web projesi
│       └── Maggsoft.Web.Framework/    # Web Framework
└── Solution Items/                     # Konfigürasyon dosyaları
    ├── api-deploy.yml                 # API deployment
    ├── api-deploy-stage.yml           # API staging deployment
    ├── Directory.Packages.props       # NuGet paket yönetimi
    ├── job-deploy.yml                 # Background job deployment
    ├── nuget.config                   # NuGet konfigürasyonu
    ├── web-deploy.yml                 # Web deployment
    └── web-deploy-stage.yml           # Web staging deployment
```

### 8.2 Startup.cs Örneği

```csharp
public class Startup
{
    public IConfiguration Configuration { get; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddControllers();
        services.AddHttpContextAccessor();
        
        // Database configuration
        services.AddMssqlConfig<AppContext>(Configuration)
            .AddFluentMigratorConfig(Configuration);
        
        // AutoMapper
        services.AddAutoMapperConfig(p => p.AddProfile<AutoMapping>(), typeof(Startup));
        
        // Event bus
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.RegisterEventConsumer();
        
        // Cache
        services.AddMaggsoftDistributedMemoryCache(typeof(IService));
        
        // Repositories
        services.AddScoped<IMssqlRepository<User>, Repository<User>>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Services
        services.RegisterAll<IService>();
        
        // Infrastructure konfigürasyonu (tüm konfigürasyonları tek seferde yapar)
        services.AddInfrastructure(Configuration);
        
        // JWT Authentication
        services.AddJwtAuthentication(Configuration);
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        // Infrastructure konfigürasyonu (tüm middleware'leri tek seferde yapar)
        app.AddInfrastructure();
        
        // Database migration
        app.AddUpMigrate();
    }
}
```

### 8.3 Controller Örneği

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(Result<List<User>>.Success(users));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        if (user == null)
            return NotFound(Result<User>.Failure(new Error("400", "Kullanıcı bulunamadı")));
        
        return Ok(Result<User>.Success(user));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email
        };
        
        var createdUser = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, 
            Result<User>.Success(createdUser));
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.GetUserAsync(id);
        if (user == null)
            return NotFound(Result<User>.Failure(new Error("400", "Kullanıcı bulunamadı")));
        
        user.Name = dto.Name;
        user.Email = dto.Email;
        
        var updatedUser = await _userService.UpdateUserAsync(user);
        return Ok(Result<User>.Success(updatedUser));
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(Result.Success(new SuccessMessage("USER_DELETED", "Kullanıcı başarıyla silindi")));
    }
}
```

---

## 9. Best Practices

### 9.1 Dependency Injection

```csharp
// Interface-based programming
public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

public class UserService : IUserService
{
    // Implementation
}

// Service registration
services.AddScoped<IUserService, UserService>();
```

### 9.2 Error Handling

```csharp
// Custom exceptions
public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(int userId) 
        : base($"Kullanıcı bulunamadı: {userId}")
    {
    }
}

// Exception handling in service
public async Task<User> GetUserAsync(int id)
{
    var user = await _userRepository.FindByIdAsync(id);
    if (user == null)
        throw new UserNotFoundException(id);
    
    return user;
}
```

### 9.3 Validation

```csharp
// FluentValidation kullanımı
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MaximumLength(100).WithMessage("İsim 100 karakterden uzun olamaz");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");
    }
}
```

### 9.4 Logging

```csharp
// Structured logging
public class UserService
{
    private readonly ILogger<UserService> _logger;
    
    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        _logger.LogInformation("Kullanıcı oluşturuluyor: {Email}", user.Email);
        
        try
        {
            var createdUser = await _userRepository.AddAsync(user);
            _logger.LogInformation("Kullanıcı başarıyla oluşturuldu: {UserId}", createdUser.Id);
            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı oluşturulurken hata oluştu: {Email}", user.Email);
            throw;
        }
    }
}
```

---

## 10. Konfigürasyon

### 10.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyProject;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "ExpirationInMinutes": 60
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 30,
    "SlidingExpiration": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ApiVersion": {
    "MajorVersion": "1",
    "MinorVersion": "0"
  },
  "TokenOptions": {
    "AccessTokenExpiration": 60,
    "ApiName": "Maggsoft API",
    "ApiVersion": "v1",
    "ApiBaseUrl": "https://localhost:5001",
    "IdentityServerBaseUrl": "https://identity.example.com",
    "OidcSwaggerUIClientId": "maggsoft_api_swaggerui",
    "OidcApiName": "maggsoft_api",
    "AdministrationRole": "Administrator",
    "RequireHttpsMetadata": true,
    "CorsAllowAnyOrigin": true,
    "CorsAllowOrigins": [],
    "IgnoreUrls": [],
    "SecurityKey": "your-secret-key-here"
  }
}
```

---

## Sonuç

Maggsoft Framework, modern ASP.NET Core uygulamaları geliştirmek için kapsamlı bir çözüm sunmaktadır. Clean Architecture prensiplerini takip eden, modüler yapıda ve çoklu veritabanı desteği olan bu framework ile:

- Hızlı proje geliştirme
- Standart kod yapısı
- Kolay bakım ve genişletme
- Yüksek performans
- Güvenlik odaklı geliştirme

mümkün olmaktadır. Framework'ün sunduğu hazır bileşenler ve extension'lar sayesinde geliştiriciler temel altyapı ile uğraşmak yerine iş mantığına odaklanabilirler. 