namespace VX.PlanningPoker.Data;

public record JSRoom(string Name, string? Deck);

public enum PlayState
{
    PickCard,
    Timer,
    Show
}

public class RoomPlayer
{
    public Player Player { get; protected set; }
    public string? pickedCard = null;

    public RoomPlayer(Player player)
    {
        Player = player;
    }

}

public class Room
{
    static Room() => Reload();

    protected static Dictionary<string, Room> rooms;

    public static void Reload()
    {
        rooms = Config.Instance.Rooms.ToDictionary(x => x.Name, x => new Room {
            Name = x.Name,
            Deck = !string.IsNullOrEmpty(x.Deck) && Deck.IsDeckExists(x.Deck) ? Deck.Get(x.Deck) : null
        });
    }

    public static IEnumerable<Room> Rooms => rooms.Values;

    public static bool IsRoomExists(string name) => rooms.ContainsKey(name);

    public static void AddRoom(Room room)
    {
        if (!rooms.ContainsKey(room.Name)) rooms.Add(room.Name, room);
        else rooms[room.Name] = room;
    }

    public static Room Get(string name) => rooms[name];


    public static implicit operator JSRoom(Room room) => new JSRoom(room.Name, room.Deck?.Name);

    public static implicit operator Room(JSRoom room)
    {
        if (!rooms.ContainsKey(room.Name)) 
            rooms.Add(room.Name, new Room() { 
                Name = room.Name, 
                Deck = !string.IsNullOrEmpty(room.Name) && Deck.IsDeckExists(room.Name) ? Deck.Get(room.Name) : null 
            });
        return rooms[room.Name];
    }

    public string Name { get; set; } = "";

    public Deck? Deck { get; set; }


    public Dictionary<Guid, RoomPlayer> Players { get; set; } = new Dictionary<Guid, RoomPlayer>();
    public PlayState PlayState { get; protected set; } = PlayState.PickCard;

    public event Action<Player>? OnPlayerJoined;

    public void Join(Player player)
    {
        Players.Add(player.Id, new RoomPlayer(player));
        OnPlayerJoined?.Invoke(player);
    }

    public event Action<Player>? OnPlayerLeaved;

    public void Leave(Player player)
    {
        Players.Remove(player.Id);
        OnPlayerLeaved?.Invoke(player);
    }

    public event Action<Player, string?>? OnCardPicked;

    public void PickCard(Player player, string? value)
    {
        Players[player.Id].pickedCard = value;
        OnCardPicked?.Invoke(player, value);
    }

    public event Action<string>? OnTimer;
    public void Timer()
    {
        PlayState = PlayState.Timer;

        OnTimer?.Invoke("3");

        Task.Delay(1000).ContinueWith(tRes =>
        {
            OnTimer?.Invoke("2");
            Task.Delay(1000).ContinueWith(tRes =>
            {
                OnTimer?.Invoke("1");
                Task.Delay(1000).ContinueWith(tRes =>
                {
                    Show();
                });
            });
        });
    }

    public event Action? OnShow;
    public void Show()
    {
        PlayState = PlayState.Show;
        OnShow?.Invoke();
    }

    public event Action? OnNewRound;
    public void NewRound()
    {
        PlayState = PlayState.PickCard;
        OnNewRound?.Invoke();
    }

    public override string ToString() => Name;
}