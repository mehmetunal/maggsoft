//args = new string[] { "cp", "-s", "TaksiSolution", "-p", "WebApi", "Taksi.WebApi", "src/api", "-p", "ClassLibrary", "Taksi.Library", "src/lib" };
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        //maggsoft cp -s TaksiSolution -p WebApi Taksi.WebApi src/api -p ClassLibrary Taksi.Library src/lib
        //args = new string[] { "cp", "-s", "TaksiSolution", "-p", "WebApi", "Taksi.WebApi", "D:\\MyProjeler\\test/src/api", "-p", "ClassLibrary", "Taksi.Library", "D:\\MyProjeler\\test/src/lib" };

        if (args.Length < 4 || !args.Contains("-s") || !args.Contains("-p"))
        {
            Console.WriteLine("Kullanım: maggsoft cp -s <SolutionName> -p <ProjectType1> <ProjectName1> [<FolderPath1>] -p <ProjectType2> <ProjectName2> [<FolderPath2>] ...");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi src/api -p ClassLibrary Taksi.Library src/lib -p WebApi Taksi.WebApi2 src/api");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library -p WebApi Taksi.WebApi2 src/api");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library -p WebApi Taksi.WebApi2");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi");
            Console.WriteLine(@"maggsoft cp -s MySolution -p WebApi Taksi.WebApi -p ClassLibrary Taksi.Library");
            Console.WriteLine(@"maggsoft cp -s MySolution -p ClassLibrary Taksi.Library");
            return;
        }

        string command = args[0].ToLower();
        if (command != "cp")
        {
            Console.WriteLine($"Bilinmeyen komut: {command}");
            return;
        }

        string solutionName = string.Empty;
        List<Tuple<string, string, string>> projects = new List<Tuple<string, string, string>>();

        try
        {
            int solutionIndex = Array.IndexOf(args, "-s") + 1;
            if (solutionIndex < 1 || solutionIndex >= args.Length)
                throw new ArgumentException("Çözüm adı (-s) parametresi eksik veya geçersiz.");
            solutionName = args[solutionIndex];

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

            CreateSolution(solutionName, projects);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }

    private static void CreateSolution(string solutionName, List<Tuple<string, string, string>> projects)
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
                    CustomizeWebApiProgramCs(folderPath);
                }
            }

            Console.WriteLine("Çözüm ve projeler başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
        }
    }

    private static string GetTemplateFromType(string projectType) => projectType.ToLower() switch
    {
        "webapi" => "webapi",
        "classlibrary" => "classlib",
        "console" => "console",
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

    private static void CustomizeWebApiProgramCs(string folderPath)
    {
        string programFilePath = Path.Combine(folderPath, "Program.cs");
        if (File.Exists(programFilePath))
        {
            Console.WriteLine("Program.cs dosyası güncelleniyor...");
            File.WriteAllText(programFilePath, GetPredefinedProgramCsContent());
        }
    }

    private static string GetPredefinedProgramCsContent()
    {
        return @"
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