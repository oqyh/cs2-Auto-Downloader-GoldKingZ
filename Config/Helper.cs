
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

    public static void CreateResource(string jsonFilePath)
    {
        
    string defaultJson = 
@"{
  //==========================================================
  //                    RESOURCE SETTINGS
  //==========================================================
  // ""*"" = Global config for maps not listed.
  //
  // Include_These_Only           -> Precache ONLY listed files.
  // Include_All_Exclude_These    -> Precache ALL except listed.
  //
  // If both are missing or empty -> Precache EVERYTHING.
  //===================================================

  ""*"":
  {
    // Default settings for all maps
  },
  ""de_dust22"": // Map Name
  {
    ""Include_These_Only"":
    [
      ""models/dev/"",
      ""materials/dev/""
    ]
  },
  ""de_miragee"": // Map Name
  {
    ""Include_All_Exclude_These"":
    [
      ""scripts/weapons.vdata"",
	  ""models/goldkingz/snowball/snowball.vmdl"",
	  ""models/goldkingz/snowball/ag2/snowball_ag2.vmdl"",
	  ""panorama/images/icons/equipment/decoy.vsvg""
    ]
  },
  ""cs_officee"": // Map Name
  {
    ""Include_All_Exclude_These"":
    [
      ""scripts/weapons.vdata""
    ]
  }
}";

        try
        {
            if (!File.Exists(jsonFilePath))
            {
                File.WriteAllText(jsonFilePath, defaultJson);
                return;
            }

            string currentContent = File.ReadAllText(jsonFilePath);
            if (string.IsNullOrWhiteSpace(currentContent))
            {
                File.WriteAllText(jsonFilePath, defaultJson);
            }
        }
        catch
        {
        }
    }

    public static void AdvancedPlayerPrintToChat(CCSPlayerController player, CounterStrikeSharp.API.Modules.Commands.CommandInfo commandInfo, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(message)) return;

        for (int i = 0; i < args.Length; i++)
        {
            message = message.Replace($"{{{i}}}", args[i]?.ToString() ?? "");
        }

        if (Regex.IsMatch(message, "{nextline}", RegexOptions.IgnoreCase))
        {
            string[] parts = Regex.Split(message, "{nextline}", RegexOptions.IgnoreCase);
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                trimmedPart = trimmedPart.ReplaceColorTags();
                if (!string.IsNullOrEmpty(trimmedPart))
                {
                    if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
                    {
                        player.PrintToConsole(" " + trimmedPart);
                    }
                    else
                    {
                        player.PrintToChat(" " + trimmedPart);
                    }
                }
            }
        }
        else
        {
            message = message.ReplaceColorTags();
            if (commandInfo != null && commandInfo.CallingContext == CounterStrikeSharp.API.Modules.Commands.CommandCallingContext.Console)
            {
                player.PrintToConsole(message);
            }
            else
            {
                player.PrintToChat(message);
            }
        }
    }
    
    public static void DebugMessage(string message, bool important = false)
    {
        if (!Configs.Instance.EnableDebug && !important) return;

        if (important)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
        }

        string output = $"[Auto Downloader]: {message}";
        Console.WriteLine(output);

        Console.ResetColor();
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


    public static bool LoadJSON(string relativePath, out JObject? jsonData, CCSPlayerController player = null!, CommandInfo info = null!)
    {
        jsonData = null;

        void Notify(string message, bool success = false, bool important = false)
        {
            if (player != null && player.IsValid)
            {
                string color = success ? "\x06" : "\x02";
                AdvancedPlayerPrintToChat(player, info, $" \x04[Auto Downloader]: {color}{message}");
            }
            DebugMessage(message, important);
        }

        try
        {
            string path = Path.Combine(MainPlugin.Instance.ModuleDirectory, relativePath);

            if (!File.Exists(path))
            {
                Notify($"{path} file does not exist.", false, true);
                return false;
            }

            string[] allLines = File.ReadAllLines(path);
            string jsonContent = string.Join("\n",allLines.Where(l => !l.TrimStart().StartsWith("//")));

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                Notify($"{path} content is empty.", false, true);
                return false;
            }

            jsonData = JObject.Parse(jsonContent);

            Notify($"{path} loaded successfully.", true, false);
            return true;
        }
        catch (JsonReaderException ex)
        {
            Notify($"JSON syntax error in {relativePath}: {ex.Message}", false, true);
            return false;
        }
        catch (Exception ex)
        {
            Notify($"Error loading {relativePath}: {ex.Message}", false, true);
            return false;
        }
    }

    
    public class Settings
    {
        public string? MapName { get; set; }
        public List<string>? Include_These_Only { get; set; }
        public List<string>? Include_All_Exclude_These { get; set; }
    }

    public static Settings? GetSettings(string currentMap)
    {
        var JsonData = MainPlugin.Instance.g_Main.JsonData;
        if (JsonData == null || string.IsNullOrEmpty(currentMap)) return null;

        JToken? mapSection = JsonData[currentMap];

        if (mapSection == null)
        {
            mapSection = JsonData["*"];
            currentMap = "*";
        }

        if (mapSection == null) return null;

        return new Settings
        {
            MapName = currentMap ?? "",
            Include_These_Only = mapSection["Include_These_Only"]?.ToObject<List<string>>() ?? null,
            Include_All_Exclude_These = mapSection["Include_All_Exclude_These"]?.ToObject<List<string>>() ?? null
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