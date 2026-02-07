using Newtonsoft.Json.Linq;

namespace Auto_Downloader_GoldKingZ;

public class Globals
{
    public CounterStrikeSharp.API.Modules.Timers.Timer Timer = null!;
    public JObject? JsonData { get; set; }


    public void Clear()
    {
        Timer?.Kill(); 
        Timer = null!;
    }
}