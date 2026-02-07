using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;

namespace Auto_Downloader_GoldKingZ;

public static class Server_Utils
{
    private static readonly IntPtr _networkServerService;
    private delegate IntPtr GetGameServerHandle(IntPtr networkServerService);
    private static readonly GetGameServerHandle _getGameServerHandleDelegate;
    private delegate IntPtr GetWorkshopId(IntPtr gameServer);
    private static readonly GetWorkshopId _getWorkshopIdDelegate;

    static unsafe Server_Utils()
    {
        _networkServerService = NativeAPI.GetValveInterface(0, "NetworkServerService_001");

        var gameServerOffset = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 24 : 23;
        IntPtr* gameServerHandle = *(IntPtr**)_networkServerService + gameServerOffset;
        _getGameServerHandleDelegate = Marshal.GetDelegateForFunctionPointer<GetGameServerHandle>(*gameServerHandle);

        var networkGameServer = _getGameServerHandleDelegate(_networkServerService);
        IntPtr* workshopHandle = *(IntPtr**)networkGameServer + 26;
        _getWorkshopIdDelegate = Marshal.GetDelegateForFunctionPointer<GetWorkshopId>(*workshopHandle);
    }

    public static string GetAddonID()
    {
        IntPtr networkGameServer = _getGameServerHandleDelegate(_networkServerService);
        IntPtr result = _getWorkshopIdDelegate(networkGameServer);
        var workshopString = Marshal.PtrToStringAnsi(result);

        if (string.IsNullOrEmpty(workshopString)) return "";

        string currentID = workshopString.Split(',')[0].Trim();

        List<string> extraAddons = GetExtraAddonsList();

        if (extraAddons.Contains(currentID))
        {
            return "";
        }

        return currentID;
    }

    private static List<string> GetExtraAddonsList()
    {
        var cv = ConVar.Find("mm_extra_addons");
        string cvValue = cv?.StringValue ?? "";
        var list = ParseIds(cvValue);

        if (list.Count > 0) return list;

        try
        {
            string configPath = GetConfigFilePath();
            if (!string.IsNullOrEmpty(configPath))
            {
                string content = File.ReadAllText(configPath);
                Match match = Regex.Match(content, @"mm_extra_addons\s+""([^""]*)""");
                if (match.Success)
                {
                    return ParseIds(match.Groups[1].Value);
                }
            }
        }
        catch { }

        return new List<string>();
    }

    private static List<string> ParseIds(string input)
    {
        if (string.IsNullOrEmpty(input)) return new List<string>();

        return input.Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x) && x.All(char.IsDigit))
            .ToList();
    }

    private static string GetConfigFilePath()
    {
        try
        {
            DirectoryInfo moduleDir = new DirectoryInfo(MainPlugin.Instance.ModuleDirectory);
            DirectoryInfo gameDir = moduleDir.Parent?.Parent?.Parent?.Parent!;
            if (gameDir == null || !gameDir.Exists) return null!;

            string path = Path.Combine(gameDir.FullName, "cfg", "multiaddonmanager", "multiaddonmanager.cfg");
            return File.Exists(path) ? path : null!;
        }
        catch { return null!; }
    }
}