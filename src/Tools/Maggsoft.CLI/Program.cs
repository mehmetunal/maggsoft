using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    static string DefaultTemplate => @"{
  ""solutionName"": ""Maggsoft"",
  ""prefix"": ""MAG"",
  ""projects"": [
    {
      ""name"": ""{prefix}.Data.Mssql"",
      ""type"": ""ClassLibrary"",
      ""folderPath"": ""src/Libraries/Data""
    },
    {
      ""name"": ""{prefix}.Dto.Mssql"",
      ""type"": ""ClassLibrary"",
      ""folderPath"": ""src/Libraries/Dto""
    },
    {
      ""name"": ""{prefix}.Endpoints.Api"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Mssql.Services"" ],
      ""folderPath"": ""src/Libraries/Endpoints""
    },
    {
      ""name"": ""{prefix}.IdentityManager"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"" ],
      ""folderPath"": ""src/Libraries""
    },
    {
      ""name"": ""{prefix}.Mssql"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"" ],
      ""folderPath"": ""src/Libraries""
    },
    {
      ""name"": ""{prefix}.Mssql.Services"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"", ""{prefix}.Dto.Mssql"", ""{prefix}.Mssql"" ],
      ""folderPath"": ""src/Libraries""
    },
    {
      ""name"": ""{prefix}.Api"",
      ""type"": ""WebApi"",
      ""references"": [
        ""{prefix}.IdentityManager"",
        ""{prefix}.Mssql.Services"",
        ""{prefix}.Mssql"",
        ""{prefix}.Data.Mssql"",
        ""{prefix}.Dto.Mssql"",
        ""{prefix}.Endpoints.Api""
      ],
      ""folderPath"": ""src/Presentation""
    },
    {
      ""name"": ""{prefix}.Web"",
      ""type"": ""AspNetMvc"",
      ""references"": [ ""{prefix}.Dto.Mssql"", ""{prefix}.Web.Framework"" ],
      ""folderPath"": ""src/Presentation""
    },
    {
      ""name"": ""{prefix}.Web.Framework"",
      ""type"": ""ClassLibrary"",
      ""folderPath"": ""src/Presentation""
    }
  ]
}";

    static void Main(string[] args)
    {
        try
        {
            //args = new string[] { "cp", "--template" };

            if (args.Contains("--template"))
            {
                HandleTemplateMode(args);
            }
            else
            {
                if (args.Length < 4 || !args.Contains("-s") || !args.Contains("-p") || args.Contains("--h") || args.Contains("--help"))
                {
                    Console.WriteLine("Kullanım: maggsoft cp -s <SolutionName> [-db <DatabaseType>] -p <ProjectType1> <ProjectName1> [<FolderPath1>] -p <ProjectType2> <ProjectName2> [<FolderPath2>] ...");
                    //          Console.WriteLine("Kullanım: maggsoft cp -s <SolutionName> -p <ProjectType1> <ProjectName1> [<FolderPath1>] -p <ProjectType2> <ProjectName2> [<FolderPath2>] ...");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi src/api -p ClassLibrary Taksi.Library src/lib -p WebApi Taksi.WebApi2 src/api");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library -p WebApi Taksi.WebApi2 src/api");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library -p WebApi Taksi.WebApi2");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library");
                    Console.WriteLine(@"maggsoft cp -s MySolution -db <DatabaseType>] -p ClassLibrary Taksi.Library");
                    Console.WriteLine(@"maggsoft cp --template template.json");
                    Console.WriteLine(@"maggsoft cp --template template.json -name SolutionName -prefix SSA");
                    Console.WriteLine(@"Database Types :sqlserver,mysql,postgresql");
                    return;
                }

                string command = args[0].ToLower();
                if (command != "cp")
                {
                    Console.WriteLine($"Bilinmeyen komut: {command}");
                    return;
                }

                string solutionName = string.Empty;
                string databaseType = null; // Veritabanı tipi için değişken

                List<Tuple<string, string, string>> projects = [];

                try
                {
                    int solutionIndex = Array.IndexOf(args, "-s") + 1;
                    if (solutionIndex < 1 || solutionIndex >= args.Length)
                        throw new ArgumentException("Çözüm adı (-s) parametresi eksik veya geçersiz.");
                    solutionName = args[solutionIndex];

                    // Veritabanı tipini kontrol et
                    int dbIndex = Array.IndexOf(args, "-db");
                    if (dbIndex > -1 && dbIndex + 1 < args.Length)
                        databaseType = args[dbIndex + 1];

                    int i = Array.IndexOf(args, "-p") + 1;
                    while (i < args.Length)
                    {
                        string projectType = args[i];
                        if (projectType == "-p")
                        {
                            projectType = args[i + 1];
                            i++;
                        }
                        if (i + 1 >= args.Length)
                            throw new ArgumentException("Proje adı eksik.");
                        string projectName = args[i + 1];
                        string folderPath = (i + 2 < args.Length && !args[i + 2].StartsWith("-")) ? args[i + 2] : null;

                        projects.Add(Tuple.Create(projectType, projectName, folderPath));
                        i += folderPath != null ? 3 : 2; // Eğer klasör yolu varsa 3, yoksa 2 adım ileri git
                    }

                    CreateSolution(solutionName, databaseType, projects);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }

    private static void HandleTemplateMode(string[] args)
    {
        string solutionName = GetArgument(args, "-name");
        string prefix = GetArgument(args, "-prefix") ?? "MAG";
        string templatePath = GetArgument(args, "--template");

        ProjectTemplate template = null;

        if (templatePath != null && File.Exists(templatePath))
        {
            template = JsonSerializer.Deserialize<ProjectTemplate>(File.ReadAllText(templatePath));
        }

        template ??= JsonSerializer.Deserialize<ProjectTemplate>(DefaultTemplate);

        if (!string.IsNullOrEmpty(solutionName))
        {
            template.SolutionName = solutionName;
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            template.Prefix = prefix;
        }

        CreateSolutionFromTemplate(template);
    }

    private static string GetArgument(string[] args, string option)
    {
        int index = Array.IndexOf(args, option);
        return (index > -1 && index + 1 < args.Length) ? args[index + 1] : null;
    }
    private static void CreateSolution(string solutionName, string databaseType, List<Tuple<string, string, string>> projects)
    {
        try
        {
            // Çözüm oluşturuluyor
            Console.WriteLine($"Çözüm oluşturuluyor: {solutionName}");
            RunCommand($"dotnet new sln -n {solutionName}");

            foreach (var project in projects)
            {
                string projectType = project.Item1;
                string projectName = project.Item2;
                string folderPath = project.Item3 ?? Path.Combine(solutionName, projectName); // Klasör yolu belirtilmediyse varsayılan yol

                string template = GetTemplateFromType(projectType);

                if (template == null)
                {
                    Console.WriteLine($"Geçersiz proje türü: {projectType}. Atlanıyor.");
                    continue;
                }

                // Proje oluşturma klasörü
                Directory.CreateDirectory(folderPath);

                // Proje oluşturuluyor
                Console.WriteLine($"Proje oluşturuluyor: {projectName} ({template})");
                RunCommand($"dotnet new {template} -n {projectName} -o {folderPath}");

                // Projeyi çözüme ekle
                Console.WriteLine($"Proje çözüm dosyasına ekleniyor: {projectName}");
                RunCommand($"dotnet sln {solutionName}.sln add {Path.Combine(folderPath, $"{projectName}.csproj")}");

                // WebAPI projeleri için Program.cs özelleştirme
                if (projectType.ToLower() == "webapi")
                {
                    CustomizeWebApiProgramCs(folderPath, databaseType);
                    AddDatabaseConfig(folderPath, databaseType);
                }
            }

            Console.WriteLine("Çözüm ve projeler başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }
    private static void CreateSolutionFromTemplate(ProjectTemplate template)
    {
        string solutionPath = Path.Combine(Directory.GetCurrentDirectory(), template.SolutionName);
        Directory.CreateDirectory(solutionPath);

        Console.WriteLine($"Çözüm oluşturuluyor: {template.SolutionName}");
        RunCommand($"dotnet new sln -n {template.SolutionName} -o {solutionPath}");

        CreateDirectoryPackagesProps(solutionPath);

        var projectPaths = new Dictionary<string, string>();

        foreach (var project in template.Projects)
        {
            string projectName = project.Name.Replace("{prefix}", template.Prefix);
            string projectFolder = Path.Combine(solutionPath, project.FolderPath, projectName);

            Directory.CreateDirectory(projectFolder);

            Console.WriteLine($"Proje oluşturuluyor: {projectName}");
            string projectType = GetTemplateFromType(project.Type);
            RunCommand($"dotnet new {projectType} -n {projectName} -o {projectFolder}");

            projectPaths[projectName] = projectFolder;

            RunCommand($"dotnet sln {Path.Combine(solutionPath, template.SolutionName)}.sln add {Path.Combine(projectFolder, $"{projectName}.csproj")}");
        }

        foreach (var project in template.Projects)
        {
            if (project.References == null || project.References.Count == 0) continue;

            string projectName = project.Name.Replace("{prefix}", template.Prefix);
            string projectFolder = projectPaths[projectName];

            foreach (var reference in project.References)
            {
                string referencedProjectName = reference.Replace("{prefix}", template.Prefix);
                if (projectPaths.ContainsKey(referencedProjectName))
                {
                    RunCommand($"dotnet add {Path.Combine(projectFolder, $"{projectName}.csproj")} reference {Path.Combine(projectPaths[referencedProjectName], $"{referencedProjectName}.csproj")}");
                }
            }
        }

        Console.WriteLine("Tüm projeler ve çözüm başarıyla oluşturuldu.");
    }

    private static void CreateDirectoryPackagesProps(string solutionPath)
    {
        string propsFilePath = Path.Combine(solutionPath, "Directory.Packages.props");

        string propsContent = @"
<Project>
  <ItemGroup>
    <PackageVersion Include=""Newtonsoft.Json"" Version=""13.0.3"" />
    <PackageVersion Include=""Serilog"" Version=""2.12.0"" />
  </ItemGroup>
</Project>";

        File.WriteAllText(propsFilePath, propsContent);
        RunCommand($"dotnet sln {Path.Combine(solutionPath, Path.GetFileName(solutionPath))}.sln add {propsFilePath}");
    }

    private static string GetTemplateFromType(string projectType) => projectType.ToLower() switch
    {
        "webapi" => "webapi",
        "classlibrary" => "classlib",
        "aspnetmvc" => "mvc",
        _ => throw new ArgumentException($"Geçersiz proje türü: {projectType}")
    };

    private static void RunCommand(string command)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();
    }
    private static void CustomizeWebApiProgramCs(string folderPath, string databaseType)
    {
        string programFilePath = Path.Combine(folderPath, "Program.cs");
        if (File.Exists(programFilePath))
        {
            Console.WriteLine("Program.cs dosyası güncelleniyor...");
            File.WriteAllText(programFilePath, GetPredefinedProgramCsContent(databaseType));
        }
    }
    private static void AddDatabaseConfig(string folderPath, string databaseType)
    {
        string appSettingsPath = Path.Combine(folderPath, "appsettings.json");
        if (!File.Exists(appSettingsPath)) return;

        string dbConnectionString = databaseType?.ToLower() switch
        {
            "sqlserver" => "Server=localhost;Database=MyDatabase;User Id=myUsername;Password=myPassword;",
            "mysql" => "Server=localhost;Database=MyDatabase;User=myUsername;Password=myPassword;",
            "postgresql" => "Host=localhost;Database=MyDatabase;Username=myUsername;Password=myPassword;",
            _ => null
        };
        
        if (dbConnectionString != null)
        {
            Console.WriteLine($"Veritabanı bağlantı dizesi ekleniyor: {databaseType}");
            File.WriteAllText(appSettingsPath, $@"
{{
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""{dbConnectionString}""
  }}
}}");
        }
    }
    private static string GetPredefinedProgramCsContent(string databaseType)
    {
        return $@"
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Hosting;

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Add database configuration here
        builder.Services.AddDbContext<MyDbContext>(options =>
            options.Use{databaseType ?? "SqlServer"}(builder.Configuration.GetConnectionString(""DefaultConnection"")));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {{
            app.UseDeveloperExceptionPage();
        }}

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
        ";
    }
    public class ProjectTemplate
    {
        [JsonPropertyName("solutionName")]
        public string SolutionName { get; set; }

        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }

        [JsonPropertyName("projects")]
        public List<Project> Projects { get; set; }
    }

    public class Project
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("references")]
        public List<string> References { get; set; }

        [JsonPropertyName("folderPath")]
        public string FolderPath { get; set; }
    }
}
