using System.Diagnostics;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Kullanım: maggsoft create-solution <SolutionName> <ProjectType1> <ProjectName1> [<ProjectType2> <ProjectName2> ...]");
            return;
        }

        string command = args[0].ToLower();
        if (command != "create-solution")
        {
            Console.WriteLine($"Bilinmeyen komut: {command}");
            return;
        }

        string solutionName = args[1];
        string[] projectTypes = args[2..];

        CreateSolution(solutionName, projectTypes);
    }

    private static void CreateSolution(string solutionName, string[] projects)
    {

        try
        {
            // Çözüm oluştur
            Console.WriteLine($"Çözüm oluşturuluyor: {solutionName}");
            RunCommand($"dotnet new sln -n {solutionName}");

            for (int i = 0; i < projects.Length; i += 2)
            {
                string projectType = projects[i].ToLower(CultureInfo.CurrentCulture);
                string projectName = projects[i + 1];
                string template = GetTemplateFromType(projectType);

                if (template == null)
                {
                    Console.WriteLine($"Geçersiz proje türü: {projectType}. Atlanıyor.");
                    continue;
                }

                // Proje oluştur
                Console.WriteLine($"Proje oluşturuluyor: {projectName} ({template})");
                RunCommand($"dotnet new {template} -n {projectName}");

                // Projeyi çözüme ekle
                Console.WriteLine($"Proje çözüm dosyasına ekleniyor: {projectName}");
                RunCommand($"dotnet sln {solutionName}.sln add {projectName}/{projectName}.csproj");

                #region Add Nuget
                //string projectPath = Path.Combine(projectName, $"{projectName}.csproj");
                //AddNuGetPackage(projectPath, "Newtonsoft.Json");
                //AddNuGetPackage(projectPath, "Swashbuckle.AspNetCore", "6.5.0");
                #endregion

                // Program.cs özelleştirme
                if (projectType == "webapi")
                {
                    CustomizeWebApiProgramCs(projectName);
                }
            }

            Console.WriteLine("Çözüm ve projeler başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }
    private static void CustomizeWebApiProgramCs(string projectName)
    {
        try
        {
            string programFilePath = Path.Combine(projectName, "Program.cs");

            if (File.Exists(programFilePath))
            {
                Console.WriteLine($"Program.cs dosyası özelleştiriliyor: {programFilePath}");
                File.WriteAllText(programFilePath, GetPredefinedProgramCsContent());
            }
            else
            {
                Console.WriteLine($"Program.cs bulunamadı: {programFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Program.cs güncellenirken hata oluştu: {ex.Message}");
        }
    }

    private static string GetTemplateFromType(string projectType) => projectType switch
    {
        "webapi" => "webapi",
        "classlibrary" => "classlib",
        "console" => "console",
        "xunit" => "xunit",
        _ => null
    };

    private static string GetPredefinedProgramCsContent()
        => @"
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Hosting;

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
        ";

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

    private static void AddNuGetPackage(string projectPath, string packageName, string version = null)
    {
        try
        {
            string versionArgument = version != null ? $"--version {version}" : "";
            string command = $"dotnet add {projectPath} package {packageName} {versionArgument}";

            Console.WriteLine($"NuGet paketi ekleniyor: {packageName}");
            RunCommand(command);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }
}
