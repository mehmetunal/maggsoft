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

                // 2. Proje Oluştur
                Console.WriteLine($"Proje oluşturuluyor: {projectName} ({template})");
                RunCommand($"dotnet new {template} -n {projectName}");

                // 3. Projeyi Çözüme Ekle
                Console.WriteLine($"Proje çözüm dosyasına ekleniyor: {projectName}");
                RunCommand($"dotnet sln {solutionName}.sln add {projectName}/{projectName}.csproj");
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
}
