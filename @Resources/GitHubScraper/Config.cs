using System.Text.Json;

namespace GitHubScraper;

class Config
{
    public string ApiUrl { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string DataFilePath { get; set; } = "";
    public int NumDays { get; set; }

    public static Config LoadConfiguration()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties", "appsettings.json");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found at {configPath}");
        }
        string jsonString = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<Config>(jsonString) ?? throw new InvalidOperationException("Failed to deserialize configuration");
    }
}
