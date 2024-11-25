using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Kullanım: mycli create-solution <SolutionName> <Project1Type> [Project2Type] ...");
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

    static void CreateSolution(string solutionName, string[] projectTypes)
    {
        try
        {
            // 1. Çözüm (Solution) Oluştur
            Console.WriteLine($"Çözüm oluşturuluyor: {solutionName}");
            RunCommand($"dotnet new sln -n {solutionName}");

            foreach (var projectType in projectTypes)
            {
                string projectName = $"{solutionName}.{projectType}";
                string template = GetTemplateFromType(projectType);

                if (template == null)
                {
                    Console.WriteLine($"Geçersiz proje türü: {projectType}");
                    continue;
                }

                string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), solutionName);

                string srcFolder = Path.Combine(baseDirectory, "src");
                Directory.CreateDirectory(srcFolder);

                Console.WriteLine($"Klasör yapısı oluşturuldu:\n{baseDirectory}");



                // Çözümü oluştur
                RunCommand($"dotnet new sln -n {solutionName}", baseDirectory);

                // WebApi projesini oluştur
                string webApiProject = Path.Combine(srcFolder, $"{projectName}");
                RunCommand($"dotnet new {template} -n {projectName}", srcFolder);


                // Çözüm dosyasına projeleri ekle
                RunCommand($"dotnet sln add {Path.Combine(webApiProject, $"{projectName}.csproj")}", baseDirectory);




                // 2. Proje Oluştur
                //Console.WriteLine($"Proje oluşturuluyor: {projectName} ({template})");
                //RunCommand($"dotnet new {template} -n {projectName}", srcFolder);

                // 3. Projeyi Çözüme Ekle
                //Console.WriteLine($"Proje çözüm dosyasına ekleniyor: {projectName}");
                //RunCommand($"dotnet sln {solutionName}.sln add {projectName}/{projectName}.csproj", baseDirectory);

                #region Add Nuget
                //string projectPath = Path.Combine(projectName, $"{projectName}.csproj");
                //AddNuGetPackage(projectPath, "Newtonsoft.Json");
                //AddNuGetPackage(projectPath, "Swashbuckle.AspNetCore", "6.5.0");
                #endregion

                if (projectType == "webapi")
                {
                    string programFilePath = Path.Combine(projectName, "Program.cs");
                    if (File.Exists(programFilePath))
                    {
                        Console.WriteLine("Program.cs dosyası güncelleniyor...");
                        File.WriteAllText(programFilePath, GetPredefinedProgramCsContent());
                    }
                }
            }

            Console.WriteLine("Çözüm ve projeler başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }

    static string GetTemplateFromType(string projectType)
    {
        return projectType.ToLower() switch
        {
            "webapi" => "webapi",
            "classlibrary" => "classlib",
            "console" => "console",
            _ => null
        };
    }

    static string GetPredefinedProgramCsContent()
        => @"
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Hosting;

        Console.WriteLine(""Çözüm ve projeler başarıyla oluşturuldu!"");

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

    static void RunCommand(string command)
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

    static void RunCommand(string command, string workingDirectory)
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
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
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

    static void AddNuGetPackage(string projectPath, string packageName, string version = null)
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
