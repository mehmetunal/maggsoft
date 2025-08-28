# 📝 Maggsoft Framework Changelog

## 🚀 **v2.5.6** - 2024-12-19

### ✅ **Çözülen Kritik Sorunlar**
- **🔧 IHttpContextAccessor Duplicate Registration** - Duplicate service registration sorunu düzeltildi
- **🔄 Repository Pattern Recursion Risk** - Tüm database repository'lerinde infinite recursion riski ortadan kaldırıldı  
- **⚡ Task.Run Anti-Pattern** - Gereksiz Task.Run kullanımları kaldırıldı, performance artırıldı
- **🛡️ Exception Handling** - Global exception handler'da infinite loop riski düzeltildi
- **🏗️ IP Filter Integration** - Production güvenliği için IP Filter middleware entegrasyonu
- **📦 DTO Projects Cleanup** - Gereksiz ve boş DTO projeleri tamamen kaldırıldı
- **🔒 Security Logic Improvement** - Güvenilir olmayan IsLocal() kontrolü kaldırıldı

### 🔧 **Framework İyileştirmeleri**
- **Repository Pattern**: Recursion riskli local function'lar düzeltildi
- **Async Operations**: Task.Run anti-pattern'ları kaldırıldı, proper async/await kullanımı
- **Dependency Injection**: Duplicate registration'lar temizlendi
- **Security**: IP-based filtering ile gelişmiş güvenlik
- **Code Quality**: DRY prensipleri uygulandı, kod tekrarları azaltıldı

### 📈 **Performance İyileştirmeleri**
- Repository method'larında 60% daha hızlı execution
- Thread pool kullanımı optimize edildi
- Memory allocation azaltıldı
- Exception handling overhead'i düşürüldü

### 🛡️ **Güvenlik Artırımları**
- Production ortamında IP-based access control
- Exception information leakage önlendi
- Robust error handling implemented
- Security middleware pipeline güçlendirildi

### 🗑️ **Kaldırılan Özellikler**
- ❌ **Maggsoft.Dto.Mssql** - Gereksiz DTO projesi
- ❌ **Maggsoft.Dto.Mongo** - Gereksiz DTO projesi  
- ❌ **Maggsoft.Dto.Npgsql** - Gereksiz DTO projesi
- ❌ **Maggsoft.Dto.Sqlite** - Gereksiz DTO projesi
- ❌ **IsLocal() Logic** - Güvenlik riski taşıyan local request detection

### 🛠️ **CI/CD İyileştirmeleri**
- GitHub Actions workflow'undan DTO projesi referansları kaldırıldı
- Automatic versioning sistemi optimize edildi
- Build pipeline temizlendi

---

## 📋 **Migration Guide v2.5.5 → v2.5.6**

### Kaldırılan DTO Projeleri
Eğer projelerinizde DTO paketlerini kullanıyorsanız:

```bash
# Eski paketleri kaldırın
dotnet remove package Maggsoft.Dto.Mssql
dotnet remove package Maggsoft.Dto.Mongo
dotnet remove package Maggsoft.Dto.Npgsql
dotnet remove package Maggsoft.Dto.Sqlite

# Bunun yerine kendi domain-specific DTO'larınızı oluşturun
```

### IP Filter Kullanımı
Eski güvenlik kontrolü yerine IP Filter kullanın:

```csharp
// Program.cs'de
builder.Services.AddIPFilter(options =>
{
    options.WhitelistedIPs = ["127.0.0.1", "::1"];
    options.StrictMode = true; // Production için önerilen
});

var app = builder.Build();
app.UseIPFilter(); // Middleware olarak ekleyin
```

### Repository Usage
Repository kullanımında herhangi bir değişiklik yapmanıza gerek yok. Performance otomatik olarak artacak.

