//args = new string[] { "cp", "-s", "TaksiSolution", "-p", "WebApi", "Taksi.WebApi", "src/api", "-p", "ClassLibrary", "Taksi.Library", "src/lib" };
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    static void Main(string[] args)
    {
        //maggsoft cp -s TaksiSolution -p WebApi Taksi.WebApi src/api -p ClassLibrary Taksi.Library src/lib
        //args = new string[] { "cp", "-s", "TaksiSolution", "-p", "WebApi", "Taksi.WebApi", "D:\\MyProjeler\\test/src/api", "-p", "ClassLibrary", "Taksi.Library", "D:\\MyProjeler\\test/src/lib" };
        //args = new string[] { "cp", "--template", "tt.json", "-name", "MySolution", "-prefix", "SSA" };
        //args = new string[] { "cp", "--template" };

        if (args.Contains("--template"))
        {
            string solutionName = null;
            string templatePath = null;
            string prefix = null;

            int templateIndex = Array.IndexOf(args, "--template");
            if (templateIndex > -1 && templateIndex + 1 < args.Length)
                templatePath = args[templateIndex + 1];

            int solutionNameIndex = Array.IndexOf(args, "-name");
            if (solutionNameIndex > -1 && solutionNameIndex + 1 < args.Length)
                solutionName = args[solutionNameIndex + 1];

            int prefixIndex = Array.IndexOf(args, "-prefix");
            if (prefixIndex > -1 && prefixIndex + 1 < args.Length)
                prefix = args[prefixIndex + 1];

            if (!File.Exists(templatePath))
            {
                Console.WriteLine("Şablon yok o yüzden default template kullanılacak");
            }

            try
            {
                var templateTxt = @"{
  ""solutionName"": ""Maggsoft"",
  ""projects"": [
    {
      ""name"": ""{prefix}.Data.Mssql"",
      ""type"": ""ClassLibrary""
    },
    {
      ""name"": ""{prefix}.Dto.Mssql"",
      ""type"": ""ClassLibrary""
    },
    {
      ""name"": ""{prefix}.Endpoints.Api"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Mssql.Services"" ]
    },
    {
      ""name"": ""{prefix}.IdentityManager"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"" ]
    },
    {
      ""name"": ""{prefix}.Mssql"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"" ]
    },
    {
      ""name"": ""{prefix}.Mssql.Services"",
      ""type"": ""ClassLibrary"",
      ""references"": [ ""{prefix}.Data.Mssql"", ""{prefix}.Dto.Mssql"", ""{prefix}.Mssql"" ]
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
      ]
    },
    {
      ""name"": ""{prefix}.Web"",
      ""type"": ""AspNetMvc"",
      ""references"": [ ""{prefix}.Dto.Mssql"", ""{prefix}.Web.Framework"" ]
    },
    {
      ""name"": ""{prefix}.Web.Framework"",
      ""type"": ""ClassLibrary""
    }
  ]
}
";
                var template = JsonSerializer.Deserialize<ProjectTemplate>(templateTxt);
                if (templatePath != null)
                {
                    template = JsonSerializer.Deserialize<ProjectTemplate>(File.ReadAllText(templatePath));
                }

                if (template == null || string.IsNullOrEmpty(template.SolutionName))
                {
                    throw new ArgumentException("Geçersiz şablon dosyası.");
                }

                if (!string.IsNullOrEmpty(solutionName))
                {
                    template.SolutionName = solutionName;
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    template.Prefix = prefix;
                }
                else
                {
                    template.Prefix = template.SolutionName;
                }

                CreateSolutionFromTemplate(template);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }

        }
        else
        {
            if (args.Length < 4 || !args.Contains("-s") || !args.Contains("-p"))
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
        Console.WriteLine($"Çözüm oluşturuluyor: {template.SolutionName}");
        RunCommand($"dotnet new sln -n {template.SolutionName}");

        var projectPaths = new Dictionary<string, string>();

        foreach (var project in template.Projects)
        {
            var projectName = project.Name.Replace("{prefix}", template.Prefix);
            string projectFolder = Path.Combine(template.SolutionName, projectName);
            Directory.CreateDirectory(projectFolder);

            Console.WriteLine($"Proje oluşturuluyor: {projectName} ({project.Type})");
            string projectTypeTemplate = GetTemplateFromType(project.Type);
            if (projectTypeTemplate == null)
            {
                Console.WriteLine($"Geçersiz proje türü: {project.Type}");
                continue;
            }

            RunCommand($"dotnet new {projectTypeTemplate} -n {projectName} -o {projectFolder}");

            projectPaths[projectName] = projectFolder;

            Console.WriteLine($"Proje çözüm dosyasına ekleniyor: {projectName}");
            RunCommand($"dotnet sln {template.SolutionName}.sln add {Path.Combine(projectFolder, $"{projectName}.csproj")}");
        }

        foreach (var project in template.Projects)
        {
            if (project.References == null || project.References.Count == 0) continue;


            var projectName = project.Name.Replace("{prefix}", template.Prefix);

            foreach (var reference in project.References)
            {
                var rf = reference.Replace("{prefix}", template.Prefix);

                if (projectPaths.ContainsKey(rf))
                {
                    string projectPath = Path.Combine(projectPaths[projectName], $"{projectName}.csproj");
                    string referencePath = Path.Combine(projectPaths[rf], $"{rf}.csproj");

                    Console.WriteLine($"Proje referansı ekleniyor: {projectName} -> {rf}");
                    RunCommand($"dotnet add {projectPath} reference {referencePath}");
                }
            }
        }

        Console.WriteLine("Tüm projeler ve referanslar başarıyla oluşturuldu!");
    }
    private static string GetTemplateFromType(string projectType) => projectType.ToLower() switch
    {
        "webapi" => "webapi",
        "classlibrary" => "classlib",
        "console" => "console",
        "aspnetmvc" => "mvc",
        _ => null
    };
    private static void RunCommand(string command)
    {
        var process = new Process
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

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(output))
            Console.WriteLine(output);
        if (!string.IsNullOrEmpty(error))
            Console.WriteLine($"Hata: {error}");

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
    }
}

//private static void AddNuGetPackage(string projectPath, string packageName, string version = null)
//{
//    try
//    {
//        string versionArgument = version != null ? $"--version {version}" : "";
//        string command = $"dotnet add {projectPath} package {packageName} {versionArgument}";

//        Console.WriteLine($"NuGet paketi ekleniyor: {packageName}");
//        RunCommand(command);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Hata: {ex.Message}");
//    }
//}