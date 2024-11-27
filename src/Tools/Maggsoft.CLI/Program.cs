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
                Console.WriteLine("Şablon dışındaki mod henüz desteklenmiyor.");
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
