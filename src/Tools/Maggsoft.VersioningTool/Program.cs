class Program
{
    static void Main(string[] args)
    {
        string pathName = args.Length > 0 ? args[0] : "";
        string versionFilePath = $"Version{(!string.IsNullOrEmpty(pathName) ? $"_{pathName}" : "")}.txt";

        string directoryPath = $"{Directory.GetCurrentDirectory()}/ProjectVersion";
        string filePath = Path.Combine(directoryPath, versionFilePath);

        string currentVersion = "1.0.0";
        if (File.Exists(filePath))   
        {
            currentVersion = File.ReadAllText(filePath).Trim();
        }

        if (!Directory.Exists(directoryPath))   
        { 
            Directory.CreateDirectory(directoryPath);
        }


        using (File.Create(filePath))
        {
            // Dosyaya yazma işlemleri buraya eklenebilir.
        }


        string[] versionParts = currentVersion.Split('.');
        int major = int.Parse(versionParts[0]);
        int minor = int.Parse(versionParts[1]);
        int patch = int.Parse(versionParts[2]);

        patch++; // Patch versiyonunu artır

        string newVersion = $"{major}.{minor}.{patch}";

        File.WriteAllText(filePath, newVersion);

        Console.WriteLine(newVersion);

    }
}