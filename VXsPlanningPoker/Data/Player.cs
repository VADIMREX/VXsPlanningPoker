using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace VX.PlanningPoker.Data;

public record JSPlayer (Guid Id, string Name);

public class Player
{
    static Player() => Reload();

    protected static Dictionary<Guid, Player> players;

    public static IEnumerable<Player> Players => players.Values;

    public static void Reload()
    {
        players = Config.Instance.Players.ToDictionary(x => x.Id, x => new Player
        {
            Id = x.Id,
            Name = x.Name
        });
    }

    public static void AddPlayer(Player player)
    {
        players.Add(player.Id, player);
    }

    public static Player Get(Guid id) => players[id];

    public static implicit operator JSPlayer(Player player) => new JSPlayer(player.Id, player.Name);

    public static implicit operator Player(JSPlayer player)
    {
        if (!players.ContainsKey(player.Id)) players.Add(player.Id, new Player() { Id = player.Id, Name = player.Name });
        return players[player.Id];
    }

    public static async Task<Player> InitPlayer(IJSRuntime JS)
    {
        var value = await JS.InvokeAsync<string>("getPlayerData");
        
        if (!string.IsNullOrEmpty(value))
            return JsonConvert.DeserializeObject<JSPlayer>(value);

        var player = new Player();
        value = JsonConvert.SerializeObject((JSPlayer)player);
        await JS.InvokeAsync<JSPlayer>("setPlayerData", value);
        return player;
    }

    public static async Task Save(IJSRuntime JS, Player player)
    {
        string value = JsonConvert.SerializeObject((JSPlayer)player);
        await JS.InvokeAsync<JSPlayer>("setPlayerData", value);
    }


    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = "";

    public override string ToString() => Name;
}