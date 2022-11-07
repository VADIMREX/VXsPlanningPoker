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
    public Card? pickedCard = null;

    public RoomPlayer(Player player)
    {
        Player = player;
    }

}

public class Room
{
    #region Хранение всех комнат
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

    #endregion

    public string Name { get; set; } = "";

    public Deck? Deck { get; set; }

    public override string ToString() => Name;

    #region игра

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

    public event Action<Player, Card?>? OnCardPicked;

    public void PickCard(Player player, Card? value)
    {
        Players[player.Id].pickedCard = value;
        OnCardPicked?.Invoke(player, value);
    }

    public event Action<string>? OnTimer;
    public void Timer()
    {
        lock (this)
        {
            if (PlayState.Timer == PlayState) return;
            PlayState = PlayState.Timer;
        }
        
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

    public string GetAverage()
    {
        var q = this.Players
                    .Where(x => null != x.Value.pickedCard?.Value)
                    .ToArray();

        if (0 == q.Length) return "";

        var i = q.Average(x => x.Value.pickedCard.Value) ?? 0;
        return i.ToString("F");
    }

    public string GetMediane()
    {
        var q = this.Players
                    .Where(x => null != x.Value.pickedCard?.Value)
                    .Select(x => x.Value.pickedCard.Value.Value)
                    .OrderBy(x => x)
                    .ToArray();

        if (0 == q.Length) return GetTextMediane();

        var mediane = q[q.Length / 2];

        if (0 != (q.Length % 2)) return mediane.ToString("F");

        mediane += q[q.Length / 2 - 1];

        mediane /= 2;

        return mediane.ToString("F");
    }

    public string GetTextMediane()
    {
        var q = this.Players
                    .Where(x => !string.IsNullOrEmpty(x.Value.pickedCard?.Title))
                    .Select(x => x.Value.pickedCard.Title)
                    .OrderBy(x => x)
                    .ToArray();

        if (0 == q.Length) return "";

        var mediane = q[q.Length / 2];

        if (0 != (q.Length % 2)) return mediane;

        var medianeLower = q[q.Length / 2 - 1];

        if (mediane == medianeLower) return mediane;

        return $"{medianeLower}-{mediane}";
    }

    public string Average { get; set; } = "";
    public string Mediane { get; set; } = "";

    public event Action? OnShow;
    public void Show()
    {
        lock (this)
        {
            if (PlayState.Show == PlayState) return;
            PlayState = PlayState.Show;
        }
        Average = GetAverage();
        Mediane = GetMediane();
        OnShow?.Invoke();
    }

    public event Action? OnNewRound;
    public void NewRound()
    {
        lock (this)
        {
            if (PlayState.PickCard == PlayState) return;
            PlayState = PlayState.PickCard;
        }
        PlayState = PlayState.PickCard;
        OnNewRound?.Invoke();
    }

    #endregion
    
}