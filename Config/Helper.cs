
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using Auto_Downloader_GoldKingZ.Config;

namespace Auto_Downloader_GoldKingZ;

public class Helper
{
    public static void DebugMessage(string message, bool important = false)
    {
        if (!Configs.Instance.EnableDebug && !important) return;
        var color = important ? Con.Red : Con.Magenta;
        string output = $"[Auto Downloader]: {message}";
        Con.WriteLine(color + output);
    }

    public static void ClearVariables()
    {
        var g_Main = MainPlugin.Instance.g_Main;

        g_Main.Clear();
    }
    
    public static void RegisterCommandsAndHooks(bool Late_Hook = false)
    {
        Server.ExecuteCommand("sv_hibernate_when_empty false");

        MainPlugin.Instance.RegisterListener<Listeners.OnServerPrecacheResources>(MainPlugin.Instance.OnServerPrecacheResources);
        MainPlugin.Instance.RegisterListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);
    }
    
    public static void RemoveRegisterCommandsAndHooks()
    {
        MainPlugin.Instance.RemoveListener<Listeners.OnServerPrecacheResources>(MainPlugin.Instance.OnServerPrecacheResources);
        MainPlugin.Instance.RemoveListener<Listeners.OnMapEnd>(MainPlugin.Instance.OnMapEnd);
    }

    public static void RemoveLegacyPrecacheConfig()
    {
        try
        {
            string legacyPath = Path.Combine(MainPlugin.Instance.ModuleDirectory, "config/precache_config.json");

            if (File.Exists(legacyPath))
            {
                File.Delete(legacyPath);
            }
        }
        catch
        {
            
        }
    }
    public class Settings
    {
        public string? MapName { get; set; }
        public List<string>? Workshop_These_Only { get; set; }
        public List<string>? Workshop_All_Exclude_These { get; set; }
        public List<string>? Custom_Include { get; set; }
    }

    public static Settings? GetSettings(string currentMap)
    {
        var filter = Configs.Instance.Precache_Filter;

        if (filter == null || filter.Count == 0)
        {
            return new Settings
            {
                MapName = "(default - precache all)",
                Workshop_These_Only = null,
                Workshop_All_Exclude_These = null,
                Custom_Include = null
            };
        }

        Precache_Rule? matched = null;

        matched = filter.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.MapName) &&
            !r.MapName.Equals("ANY", StringComparison.OrdinalIgnoreCase) &&
            !r.MapName.EndsWith("_") &&
            r.MapName.Equals(currentMap, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
        {
            matched = filter
                .Where(r => !string.IsNullOrWhiteSpace(r.MapName) &&
                            r.MapName.EndsWith("_") &&
                            currentMap.StartsWith(r.MapName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.MapName.Length)
                .FirstOrDefault();
        }

        if (matched == null)
        {
            matched = filter.FirstOrDefault(r =>
                string.IsNullOrWhiteSpace(r.MapName) ||
                r.MapName.Equals("ANY", StringComparison.OrdinalIgnoreCase));
        }

        if (matched == null) return null;

        static List<string>? Clean(List<string>? src) =>
            (src == null) ? null
            : src.Where(s => !string.IsNullOrWhiteSpace(s)).ToList() is { Count: > 0 } list
                ? list : null;

        return new Settings
        {
            MapName = string.IsNullOrWhiteSpace(matched.MapName) ? "ANY" : matched.MapName,
            Workshop_These_Only = Clean(matched.Workshop_These_Only),
            Workshop_All_Exclude_These = Clean(matched.Workshop_All_Exclude_These),
            Custom_Include = Clean(matched.Custom_Include)
        };
    }

    public static void RestartMap()
    {
        var MapName = Server.MapName;
        var MapWorkShop_ID = Server_Utils.GetAddonID();

        if(!string.IsNullOrEmpty(MapWorkShop_ID))
        {
            Server.ExecuteCommand("host_workshop_map " + MapWorkShop_ID);
        }else
        {
            Server.ExecuteCommand("changelevel " + MapName);
        }
    }
}