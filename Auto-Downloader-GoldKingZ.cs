using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Auto_Downloader_GoldKingZ.Config;


namespace Auto_Downloader_GoldKingZ;

public class MainPlugin : BasePlugin
{
    public override string ModuleName => "Automatically Download/Precaches Addons Depend Map Config + Update Manually";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    public Globals g_Main = new();
    public override void Load(bool hotReload)
    {
        Instance = this;
        Configs.Load(ModuleDirectory);
        
        Helper.RemoveRegisterCommandsAndHooks();
        Helper.ClearVariables();

        bool success = Helper.LoadJSON("config/precache_config.json", out JObject? loadedData);
        if (success)
        {
            g_Main.JsonData = loadedData;
        }

        Helper.RegisterCommandsAndHooks();
       

        if (hotReload)
        {
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables();

            bool success2 = Helper.LoadJSON("config/precache_config.json", out JObject? loadedData2);
            if (success2)
            {
                g_Main.JsonData = loadedData2;
            }
            
            Helper.RegisterCommandsAndHooks();
        }
    }



    public void OnServerPrecacheResources(ResourceManifest manifest)
    {
        try
        {
            var serverIds = AddonManagerConfig.Get_mm_extra_addons();
            var clientIds = AddonManagerConfig.Get_mm_client_extra_addons();

            if (serverIds.Count == 0 && clientIds.Count == 0)
            {
                Helper.DebugMessage(@"[Error] No Addons IDs Found In Either [mm_extra_addons] or [mm_client_extra_addons] within cfg\multiaddonmanager\multiaddonmanager.cfg", true);
                return;
            }

            List<string> missingServerIds = new();
            List<string> missingClientIds = new();

            foreach (var id in serverIds)
            {
                if (string.IsNullOrEmpty(VpkParser.FindVpkFile(id)))
                {
                    missingServerIds.Add(id);
                }
            }

            if (missingServerIds.Count > 0)
            {
                string joinedServerIds = string.Join(",", missingServerIds);
                Helper.DebugMessage($"[Server Addon] Addons IDs [{joinedServerIds}] Missing", true);
            }

            foreach (var id in clientIds)
            {
                if (string.IsNullOrEmpty(VpkParser.FindVpkFile(id)))
                {
                    missingClientIds.Add(id);
                }
            }

            if (missingClientIds.Count > 0)
            {
                string joinedClientIds = string.Join(",", missingClientIds);
                Helper.DebugMessage($"[Client Addon] Addons IDs [{joinedClientIds}] Missing", true);
            }

            var allMissing = missingServerIds.Concat(missingClientIds).Distinct().ToList();
            if (allMissing.Count > 0 && Configs.Instance.ForceDownloadMissing)
            {
                AddTimer(5.0f, () =>
                {
                    if (missingServerIds.Count > 0)
                    {
                        string joinedServer = string.Join(",", missingServerIds);
                        Helper.DebugMessage($"[Server Addon] Force Downloading [{joinedServer}]", true);

                        foreach (var id in missingServerIds)
                        {
                            Server.ExecuteCommand("mm_remove_addon " + id);
                            AddTimer(1.0f, () => Server.ExecuteCommand("mm_add_addon " + id), TimerFlags.STOP_ON_MAPCHANGE);
                            AddTimer(3.0f, () => Server.ExecuteCommand("mm_download_addon " + id), TimerFlags.STOP_ON_MAPCHANGE);
                        }
                    }

                    if (missingClientIds.Count > 0)
                    {
                        string joinedClient = string.Join(",", missingClientIds);
                        Helper.DebugMessage($"[Client Addon] Force Downloading [{joinedClient}]", true);

                        foreach (var id in missingClientIds)
                        {
                            Server.ExecuteCommand("mm_remove_client_addon " + id);
                            AddTimer(1.0f, () => Server.ExecuteCommand("mm_add_client_addon " + id), TimerFlags.STOP_ON_MAPCHANGE);
                            AddTimer(3.0f, () => Server.ExecuteCommand("mm_download_addon " + id), TimerFlags.STOP_ON_MAPCHANGE);
                        }
                    }

                    g_Main.Timer?.Kill(); 
                    g_Main.Timer = null!;
                    
                    g_Main.Timer = AddTimer(5.0f, () =>
                    {
                        List<string> pendingIds = new List<string>();

                        foreach (var id in allMissing)
                        {
                            if (!VpkParser.IsWorkshopDownloaded(id))
                            {
                                pendingIds.Add(id);
                            }
                        }

                        if (pendingIds.Count > 0)
                        {
                            string joinedIds = string.Join(",", pendingIds);
                            Helper.DebugMessage($"[Waiting] [{joinedIds}] Is Still Downloading...", true);
                        }
                        else
                        {
                            g_Main.Timer?.Kill();
                            g_Main.Timer = null!;
                            
                            Helper.DebugMessage($"*****************************************", true);
                            Helper.DebugMessage($"[Success] All Server And Client Addons Ready!", true);
                            Helper.DebugMessage($"Restarting Map To Precache In 5 Seconds...", true);
                            Helper.DebugMessage($"*****************************************", true);

                            AddTimer(5.0f, () => Helper.RestartMap(), TimerFlags.STOP_ON_MAPCHANGE);
                        }

                    }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

                }, TimerFlags.STOP_ON_MAPCHANGE);
            }

            var serverWorkshopIds = AddonManagerConfig.Get_mm_extra_addons();
            var clientWorkshopIds = AddonManagerConfig.Get_mm_client_extra_addons();
            var combinedWorkshopIds = serverWorkshopIds.Concat(clientWorkshopIds).Distinct().ToList();

            var settings = Helper.GetSettings(Server.MapName);
            if (settings == null) return;
            
            var getmapname = settings.MapName?.Trim();
            if (!string.IsNullOrEmpty(getmapname))
            {
                var Workshop_These_Only = settings.Workshop_These_Only;
                var Workshop_All_Exclude_These = settings.Workshop_All_Exclude_These;
                var Custom_Include = settings.Custom_Include;
                
                bool message_onetime = false;
                foreach (var getworkshopIds in combinedWorkshopIds)
                {
                    if (string.IsNullOrEmpty(VpkParser.FindVpkFile(getworkshopIds))) continue;

                    var eventsForId = VpkParser.GetEventsFromVpk(getworkshopIds);
                    if(eventsForId == null)continue;

                    var sortedEvents = eventsForId
                    .Where(evt => !string.IsNullOrEmpty(evt))
                    .OrderBy(evt => 
                    {
                        if (Workshop_These_Only != null)
                            return Workshop_These_Only.Any(f => evt.StartsWith(f));
                        
                        if (Workshop_All_Exclude_These != null)
                            return !Workshop_All_Exclude_These.Any(f => evt.StartsWith(f));
                        
                        return true;
                    })
                    .ToList();

                    bool message_onetime_2 = false;
                    foreach (var evt in sortedEvents)
                    {
                        if (!message_onetime)
                        {
                            Helper.DebugMessage($"===== Starting Precache [Map {Server.MapName}] =====");
                            Helper.DebugMessage($"[Config Match]: {getmapname}");
                            message_onetime = true;
                        }

                        if (!message_onetime_2)
                        {
                            Helper.DebugMessage($"--- [Workshop ID: {getworkshopIds}] ---");
                            message_onetime_2 = true;
                        }
                        
                        if (Workshop_These_Only != null)
                        {
                            if (Workshop_These_Only.Any(f => evt.StartsWith(f)))
                            {
                                manifest.AddResource(evt);
                                Helper.DebugMessage($"[✓] {evt}");
                            }
                            else
                            {
                                Helper.DebugMessage($"[X] {evt}");
                            }
                        }
                        else if (Workshop_All_Exclude_These != null)
                        {
                            if (!Workshop_All_Exclude_These.Any(f => evt.StartsWith(f)))
                            {
                                manifest.AddResource(evt);
                                Helper.DebugMessage($"[✓] {evt}");
                            }
                            else
                            {
                                Helper.DebugMessage($"[X] {evt}");
                            }
                        }
                        else
                        {
                            manifest.AddResource(evt);
                            Helper.DebugMessage($"[✓] {evt}");
                        }
                    }
                }

                if (Custom_Include != null && Custom_Include.Count > 0)
                {
                    Helper.DebugMessage($"--- [Custom Includes] ---");
                    foreach (var directEntry in Custom_Include)
                    {
                        if (!string.IsNullOrWhiteSpace(directEntry))
                        {
                            manifest.AddResource(directEntry);
                            Helper.DebugMessage($"[✓] {directEntry}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"OnServerPrecacheResources Error: {ex.Message}");
        }
    }
    
    public void OnMapEnd()
    {
        try
        {
            Helper.ClearVariables();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"OnMapEnd Error: {ex.Message}", true);
        }
    }

    public override void Unload(bool hotReload)
    {
        try
        {
            Helper.RemoveRegisterCommandsAndHooks();
            Helper.ClearVariables();

        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Unload Error: {ex.Message}", true);
        }

        if (hotReload)
        {
            try
            {
                Helper.RemoveRegisterCommandsAndHooks();
                Helper.ClearVariables();
            }
            catch (Exception ex)
            {
                Helper.DebugMessage($"Unload hotReload Error: {ex.Message}", true);
            }
        }
    }

    [ConsoleCommand("gkz_download", "Force Update/Download Addons")]
    [ConsoleCommand("gkz_forcedownload", "Force Update/Download Addons")]
    [ConsoleCommand("gkz_update", "Force Update/Download Addons")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void ForceUpdate(CCSPlayerController? player, CommandInfo commandInfo)
    {
        void SendReply(string message)
        {
            if (player == null)
            {
                Helper.DebugMessage(message, true);
            }
            else
            {
                commandInfo.ReplyToCommand($"[Auto Precache]: {message}");
            }
        }
        
        string targetId = commandInfo.GetArg(1).Trim();
        if (string.IsNullOrEmpty(targetId))
        {
            SendReply("[Invalid] <AddonsID>");
            return;
        }

        if (!targetId.All(char.IsDigit))
        {
            SendReply($"[Invalid] Addons ID: [{targetId}]. The ID Must Be Numbers Only");
            return;
        }

        if (g_Main.Timer != null)
        {
            SendReply("[Wait] A Download Or Update Sequence Is Already In Progress. Please Wait For Completion");
            return;
        }

        var serverIds = AddonManagerConfig.Get_mm_extra_addons();
        var clientIds = AddonManagerConfig.Get_mm_client_extra_addons();

        bool isServerAddon = serverIds.Contains(targetId);
        bool isClientAddon = clientIds.Contains(targetId);

        if (!isServerAddon && !isClientAddon)
        {
            SendReply($@"[Error] ID [{targetId}] Not Found In Either [mm_extra_addons] or [mm_client_extra_addons] within cfg\multiaddonmanager\multiaddonmanager.cfg");
            return;
        }

        string vpkPath = VpkParser.FindAnyVpkFile(targetId);
        if (!string.IsNullOrEmpty(vpkPath))
        {
            string? folderPath = Path.GetDirectoryName(vpkPath);
            if (folderPath != null && Directory.Exists(folderPath))
            {
                try
                {
                    SendReply($"[Deleting] Addons ID {targetId}...");
                    Directory.Delete(folderPath, true); 
                }
                catch (Exception)
                {
                    SendReply($"[Critical] File Locked Cannot Be Deleted: The Engine Is Still Using Folder [{folderPath}]");
                    SendReply($"[Critical] To Update, Follow These Steps:");
                    SendReply($"-------------------------------------------");
                    SendReply($"1- Shutdown The Server");
                    SendReply($"2- Delete This Folder: {folderPath}");
                    SendReply($"3- Start The Server And Execute The Command Again If ForceDownloadMissing Set To [False] OtherWise It Will Auto Download No Need To Execute The Command Again");
                    return;
                }
            }
        }
        else
        {
            SendReply($"[Update] Addons ID [{targetId}] Not Found In Server. Starting Fresh Download...");
        }

        if (isServerAddon)
        {
            SendReply($"[Server Addon] Force Downloading [{targetId}]");
            Server.ExecuteCommand("mm_remove_addon " + targetId);
            AddTimer(1.0f, () => Server.ExecuteCommand("mm_add_addon " + targetId));
            AddTimer(3.0f, () => Server.ExecuteCommand("mm_download_addon " + targetId));
        }
        
        if (isClientAddon)
        {
            SendReply($"[Client Addon] Force Downloading [{targetId}]");
            Server.ExecuteCommand("mm_remove_client_addon " + targetId);
            AddTimer(1.0f, () => Server.ExecuteCommand("mm_add_client_addon " + targetId));
            AddTimer(3.0f, () => Server.ExecuteCommand("mm_download_addon " + targetId));
        }

        g_Main.Timer?.Kill();
        g_Main.Timer = null!;
        g_Main.Timer = AddTimer(5.0f, () =>
        {
            if (!VpkParser.IsWorkshopDownloaded(targetId))
            {
                SendReply($"[Waiting] [{targetId}] Is Still Downloading...");
            }
            else
            {
                g_Main.Timer?.Kill();
                g_Main.Timer = null!;
                
                SendReply($"*****************************************");
                SendReply($"[Success] Addon {targetId} Updated!");
                SendReply($"Restarting Map In 5 Seconds...");
                SendReply($"*****************************************");

                AddTimer(5.0f, () => Helper.RestartMap());
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

    }

    /* [ConsoleCommand("css_test", "test")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void test(CCSPlayerController? player, CommandInfo commandInfo)
    {

    } */

}