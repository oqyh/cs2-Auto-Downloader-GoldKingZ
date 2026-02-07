using SteamDatabase.ValvePak;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Cvars;

namespace Auto_Downloader_GoldKingZ;

public class AddonManagerConfig
{
    public static List<string> Get_mm_extra_addons()
    {
        return GetWorkshopIdsFromSource("mm_extra_addons");
    }

    public static List<string> Get_mm_client_extra_addons()
    {
        return GetWorkshopIdsFromSource("mm_client_extra_addons");
    }

    private static List<string> GetWorkshopIdsFromSource(string conVarName)
    {
        var conVar = ConVar.Find(conVarName);
        string cvValue = conVar?.StringValue ?? string.Empty;
        var workshopIds = ParseAndValidateIds(cvValue);

        if (workshopIds.Count > 0)
        {
            return workshopIds;
        }

        try
        {
            string configPath = GetConfigFilePath();
            if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
            {
                string configContent = File.ReadAllText(configPath);
                
                string pattern = $@"{conVarName}\s+""([^""]*)""";
                Match match = Regex.Match(configContent, pattern);
                
                if (match.Success)
                {
                    string fileValue = match.Groups[1].Value;
                    workshopIds = ParseAndValidateIds(fileValue);
                }
            }
        }
        catch { }

        return workshopIds;
    }

    private static List<string> ParseAndValidateIds(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput)) return new List<string>();

        return rawInput.Split(',')
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrWhiteSpace(id) && id.All(char.IsDigit))
            .Distinct()
            .ToList();
    }

    private static string GetConfigFilePath()
    {
        try
        {
            DirectoryInfo moduleDir = new DirectoryInfo(MainPlugin.Instance.ModuleDirectory);
            DirectoryInfo gameDir = moduleDir.Parent?.Parent?.Parent?.Parent!;
            
            if (gameDir == null || !gameDir.Exists) return null!;

            string configPath = Path.Combine(gameDir.FullName, "cfg", "multiaddonmanager", "multiaddonmanager.cfg");
            return File.Exists(configPath) ? configPath : null!;
        }
        catch { return null!; }
    }
}

public class VpkParser
{
    public static List<string> GetEventsFromVpk()
    {
        var allEvents = new List<string>();
        var serverAddons = AddonManagerConfig.Get_mm_extra_addons();
        var clientAddons = AddonManagerConfig.Get_mm_client_extra_addons();
        
        var workshopIds = serverAddons.Concat(clientAddons).Distinct().ToList();

        if (workshopIds.Count == 0) return allEvents;

        foreach (string workshopId in workshopIds)
        {
            try
            {                
                List<string> Events = GetEventsFromVpk(workshopId);
                allEvents.AddRange(Events);
            }
            catch { }
        }

        return allEvents.Distinct().Select(ConvertVsndevtsExtension).ToList();
    }

    public static List<string> GetEventsFromVpk(string workshopId)
    {
        var eventsSet = new HashSet<string>(); 

        try
        {
            string vpkPath = FindVpkFile(workshopId);
            if (string.IsNullOrEmpty(vpkPath)) return new List<string>();

            using var package = new Package();
            package.Read(vpkPath);
            
            if (package.Entries == null) return new List<string>();

            foreach (var fileType in package.Entries)
            {
                foreach (var entry in fileType.Value)
                {
                    string fullPath = entry.GetFullPath();
                    
                    if (fullPath.EndsWith("_c")) fullPath = fullPath[..^2];

                    eventsSet.Add(fullPath);
                }
            }
        }
        catch { }

        return eventsSet.ToList();
    }

    private static string ConvertVsndevtsExtension(string filePath)
    {
        return filePath.EndsWith("_c") ? filePath[..^2] : filePath;
    }

    public static bool IsWorkshopDownloaded(string workshopId)
    {
        return !string.IsNullOrEmpty(FindVpkFile(workshopId));
    }

    public static string FindVpkFile(string workshopId)
    {
        try
        {
            string workshopDir = GetWorkshopFolderPath(workshopId);
            if (string.IsNullOrEmpty(workshopDir)) return null!;

            string modernVpk = Path.Combine(workshopDir, $"{workshopId}_dir.vpk");
            if (File.Exists(modernVpk)) return modernVpk;

            string legacyVpk = Path.Combine(workshopDir, $"{workshopId}.vpk");
            if (File.Exists(legacyVpk)) return legacyVpk;

            return null!;
        }
        catch { return null!; }
    }
    public static string FindAnyVpkFile(string workshopId)
    {
        try
        {
            string workshopDir = GetWorkshopFolderPath(workshopId);
            if (string.IsNullOrEmpty(workshopDir) || !Directory.Exists(workshopDir)) 
                return null!;

            string? anyVpk = Directory.EnumerateFiles(workshopDir, "*.vpk", SearchOption.TopDirectoryOnly)
                                      .FirstOrDefault();

            return anyVpk ?? null!;
        }
        catch { return null!; }
    }
    public static string GetWorkshopFolderPath(string workshopId)
    {
        try
        {
            string startDirectory = MainPlugin.Instance.ModuleDirectory;
            var moduleDir = new DirectoryInfo(startDirectory);
            var gameDir = moduleDir.Parent?.Parent?.Parent?.Parent?.Parent;

            if (gameDir == null || !gameDir.Exists) return null!;

            var machine = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win64" : "linuxsteamrt64";

            string path = Path.Combine(
                gameDir.FullName, "bin", machine, "steamapps", "workshop", "content", "730", workshopId
            );

            return Directory.Exists(path) ? path : null!;
        }
        catch { return null!; }
    }
}