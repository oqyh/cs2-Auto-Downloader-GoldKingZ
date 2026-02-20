
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
  // Priority: 1. Full Name (de_dust2) | 2. Prefix_ (de_) | 3. Global (*)
  // Note: If You Want To Use Prefixes, MUST End With ""_""
  //
  // - Workshop_These_Only         -> Precache ONLY listed files from Workshop VPKs.
  // - Workshop_All_Exclude_These  -> Precache ALL from Workshop EXCEPT listed.
  // - Custom_Include              -> Custom Precache From CS2 / Workshop / Any
  //==========================================================

  ""*"": 
  {
    // Default Settings For All Maps
  },
  ""de_"": // Prefix Of The Map Shortcut
  {
	""Custom_Include"":
	[
      ""soundevents/game_sounds_ui.vsndevts"",
      ""models/vehicles/airplane_medium_01/airplane_medium_01_landed.vmdl"",
    ]
  },
  ""de_dust22"": //Overide Prefix de_ On Map de_dust22 Only
  {
    ""Workshop_These_Only"":
	[
      ""models/dev/"",
      ""materials/dev/""
    ]
  },
  ""cs_office"": // Full Map Name
  {
    ""Workshop_All_Exclude_These"":
	[
      ""scripts/weapons.vdata"",
      ""models/goldkingz/snowball/snowball.vmdl"",
      ""panorama/images/icons/equipment/decoy.vsvg""
    ],
    ""Custom_Include"":
	[
       ""models/vehicles/airplane_small_01/airplane_small_01.vmdl""
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
        public List<string>? Workshop_These_Only { get; set; }
        public List<string>? Workshop_All_Exclude_These { get; set; }
        public List<string>? Custom_Include { get; set; }
    }

    public static Settings? GetSettings(string currentMap)
    {
        var jsonData = MainPlugin.Instance.g_Main.JsonData;
        if (jsonData == null) return null;

        var keys = jsonData.Properties().Select(p => p.Name).ToList();
        string? matchedKey = currentMap.GetBestMapMatch(keys);
        
        if (matchedKey == null)
        {
            if (keys.Contains("*"))
            {
                matchedKey = "*";
            }
            else
            {
                return null;
            }
        }

        JToken mapSection = jsonData[matchedKey]!;

        return new Settings
        {
            MapName = matchedKey,
            Workshop_These_Only = mapSection["Workshop_These_Only"]?.ToObject<List<string>>(),
            Workshop_All_Exclude_These = mapSection["Workshop_All_Exclude_These"]?.ToObject<List<string>>(),
            Custom_Include = mapSection["Custom_Include"]?.ToObject<List<string>>()
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