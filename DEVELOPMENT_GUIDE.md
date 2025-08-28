# 🛠️ Maggsoft Framework Development Guide

## 🧪 **1. Unit Testing Strategy**

### Test Projesi Yapısı
```bash
src/
├── Tests/
│   ├── Maggsoft.Core.Tests/           # Core kütüphane testleri
│   ├── Maggsoft.Framework.Tests/      # Framework testleri  
│   ├── Maggsoft.Mssql.Tests/          # MSSQL repository testleri
│   ├── Maggsoft.Cache.Tests/          # Cache testleri
│   └── Integration.Tests/             # Integration testleri
```

### Kritik Test Alanları

#### **Repository Tests**
```csharp
[TestClass]
public class MssqlRepositoryTests
{
    [TestMethod]
    public async Task FindAllAsync_ShouldNotCauseRecursion()
    {
        // Düzelttiğimiz recursion sorunu için test
        var repository = new Repository<TestEntity>(mockContext);
        
        var result = await repository.FindAllAsync(q => q.Take(10));
        
        Assert.IsNotNull(result);
        // Recursion olmadığını doğrula
    }
    
    [TestMethod]
    public async Task UpdateAsync_ShouldNotUseTaskRun()
    {
        // Task.Run anti-pattern düzeltmesini test et
        var repository = new Repository<TestEntity>(mockContext);
        var entity = new TestEntity { Id = 1 };
        
        var stopwatch = Stopwatch.StartNew();
        await repository.UpdateAsync(entity);
        stopwatch.Stop();
        
        // Performance'ın artığını doğrula
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);
    }
}
```

#### **Framework Integration Tests**
```csharp
[TestClass]
public class ServiceCollectionTests
{
    [TestMethod]
    public void AddInfrastructure_ShouldNotRegisterDuplicateServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        
        services.AddInfrastructure(config);
        
        // IHttpContextAccessor'ın sadece bir kere register edildiğini doğrula
        var httpContextAccessorServices = services.Where(s => 
            s.ServiceType == typeof(IHttpContextAccessor)).ToList();
        
        Assert.AreEqual(1, httpContextAccessorServices.Count);
    }
}
```

---

## ⚡ **2. Performance Testing**

### Load Testing Setup
```bash
# k6 ile load testing
npm install -g k6

# Test scriptleri
mkdir performance-tests
cd performance-tests
```

### Performance Test Script Örneği
```javascript
// performance-tests/repository-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // 100 kullanıcıya kadar
    { duration: '5m', target: 100 }, // 5 dakika 100 kullanıcı
    { duration: '2m', target: 200 }, // 200'e çık
    { duration: '5m', target: 200 }, // 5 dakika 200 kullanıcı
    { duration: '2m', target: 0 },   // Sıfırla
  ],
};

export default function () {
  // Repository performance testi
  let response = http.get('http://localhost:5000/api/users');
  
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
  });
  
  sleep(1);
}
```

### Benchmark Testleri
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class RepositoryBenchmarks
{
    private Repository<TestEntity> _repository;
    private DbContext _context;
    
    [GlobalSetup]
    public void Setup()
    {
        // Setup mock context
        _context = CreateInMemoryContext();
        _repository = new Repository<TestEntity>(_context);
    }
    
    [Benchmark]
    public async Task<List<TestEntity>> FindAllAsync_New()
    {
        // Yeni düzeltilmiş version
        return await _repository.FindAllAsync(q => q.Take(1000));
    }
    
    [Benchmark]
    public async Task<TestEntity> UpdateAsync_New()
    {
        // Task.Run olmayan version
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        return await _repository.UpdateAsync(entity);
    }
}
```

---

## 📚 **3. Documentation Güncelleme**

### API Documentation
```bash
# Swagger/OpenAPI documentation
# Program.cs'de zaten mevcut, ama daha detaylandır:

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Maggsoft Framework API", 
        Version = "v2.5.6",
        Description = "Modern ASP.NET Core 8 Framework",
        Contact = new OpenApiContact
        {
            Name = "Maggsoft",
            Email = "info@maggsoft.com",
            Url = new Uri("https://maggsoft.com")
        }
    });
    
    // XML comments için
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### Code Documentation
```csharp
/// <summary>
/// Repository pattern implementation for MSSQL database operations.
/// This version fixes recursion issues and removes Task.Run anti-patterns.
/// </summary>
/// <typeparam name="T">Entity type implementing IEntity</typeparam>
/// <remarks>
/// Version 2.5.6 improvements:
/// - Fixed infinite recursion in FindAll methods
/// - Removed Task.Run anti-patterns for better performance
/// - Added proper async/await patterns
/// </remarks>
public sealed class Repository<T> : IMssqlRepository<T> where T : BaseEntity, IEntity
{
    /// <summary>
    /// Retrieves all entities with optional query transformation.
    /// </summary>
    /// <param name="func">Optional query transformation function</param>
    /// <returns>List of entities</returns>
    /// <exception cref="ArgumentNullException">Thrown when func parameter is invalid</exception>
    public IList<T> FindAll(Func<IQueryable<T>, IQueryable<T>> func = null)
    {
        var query = func != null ? func(_dbSet) : _dbSet;
        return query.ToList();
    }
}
```

---

## 🔄 **4. CI/CD Pipeline Optimization**

### GitHub Actions Improvements
```yaml
# .github/workflows/main.yml improvements

name: Enhanced CI/CD Pipeline
on:
  push:
    branches: [ "main", "develop" ]
  pull_request:
    branches: [ "main" ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      # .NET Setup
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
          
      # Test Discovery ve Execution
      - name: Run Unit Tests
        run: |
          dotnet test --configuration Release \
                     --logger trx \
                     --collect:"XPlat Code Coverage" \
                     --results-directory TestResults/
                     
      # Code Coverage
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          directory: TestResults/
          
  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      # Security Scanning
      - name: Run Security Scan
        uses: securecodewarrior/github-action-add-sarif@v1
        with:
          sarif-file: security-scan-results.sarif
          
  performance-test:
    runs-on: ubuntu-latest
    needs: test
    steps:
      - uses: actions/checkout@v4
      
      # Performance Testing
      - name: Run Performance Tests
        run: |
          # k6 performance tests
          docker run --rm -v $PWD:/app grafana/k6 run /app/performance-tests/load-test.js
          
  build-and-deploy:
    runs-on: ubuntu-latest
    needs: [test, security-scan, performance-test]
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      
      # NuGet Package Creation
      - name: Create NuGet Packages
        run: |
          for proj in $(find . -name '*.csproj' -path "*/src/Libraries/*" -o -path "*/src/Presentation/*"); do
            dotnet pack "$proj" \
                       --configuration Release \
                       --output ./nupkgs \
                       --include-symbols \
                       --include-source
          done
          
      # Automatic Versioning
      - name: Auto Version
        run: |
          dotnet run --project src/Tools/Maggsoft.VersioningTool
          
      # Docker Image (opsiyonel)
      - name: Build Docker Image
        run: |
          docker build -t maggsoft/framework:${{ github.sha }} .
          docker tag maggsoft/framework:${{ github.sha }} maggsoft/framework:latest
```

### Quality Gates
```yaml
quality-gate:
  runs-on: ubuntu-latest
  steps:
    # Code Quality Checks
    - name: SonarCloud Scan
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        
    # Dependency Scanning
    - name: Dependency Check
      run: |
        dotnet list package --vulnerable \
                            --include-transitive \
                            --framework net8.0
```

---

## 🎯 **Sonraki Adımlar Öncelik Sırası**

### **Hemen Yapılacaklar (1 hafta):**
1. ✅ Critical bug fixes (TAMAMLANDI)
2. 🔄 Unit test projesi oluştur
3. 📊 Performance benchmark baseline'ı oluştur

### **Kısa Vadeli (1 ay):**
1. 🧪 %80+ test coverage hedefle
2. 📈 Performance regression testleri kur
3. 📚 API documentation tamamla
4. 🔒 Security audit gerçekleştir

### **Orta Vadeli (3 ay):**
1. 🏗️ Microservices template'leri oluştur
2. 🐳 Docker containerization
3. ☁️ Cloud deployment guides
4. 📦 NuGet package optimization

Bu plan ile framework'ünüz enterprise-ready hale gelecek! 🚀
