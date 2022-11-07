namespace VX.PlanningPoker.Data;

using System.IO;
using Newtonsoft.Json;

public class Config
{
    #region Save-Load
    protected static Config? instance;

    public static Config Instance
    {
        get {
            if (null != instance) return instance;
            Reload();
            return instance; 
        }
    }

    public static void Reload()
    {
        try
        {
            using StreamReader sr = new("config.json");
            var s = sr.ReadToEnd();
            instance = JsonConvert.DeserializeObject<Config>(s) ?? new Config();

            Deck.Reload();
            Room.Reload();
            Player.Reload();
        }
        catch
        {
            instance = new Config();
        }
    }

    public static void Save()
    {
        try
        {
            Instance.Decks = Deck.Decks.ToArray();
            Instance.Rooms = Room.Rooms.Select(x=>(JSRoom)x).ToArray();
            Instance.Players = Player.Players.Select(x=>(JSPlayer)x).ToArray();

            using StreamWriter sw = new("config.json");
            var s = JsonConvert.SerializeObject(instance);
            sw.Write(s);
        }
        catch
        {

        }
    }
    #endregion

    public Deck[] Decks = new Deck[0];

    public JSRoom[] Rooms = new JSRoom[0];

    public JSPlayer[] Players = new JSPlayer[0];
}