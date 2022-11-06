namespace VX.PlanningPoker.Data
{
    public class Deck
    {
        static Deck() => Reload();

        protected static Dictionary<string, Deck> decks;

        public static void Reload()
        {
            decks = Config.Instance.Decks.ToDictionary(x => x.Name, x => x);
        }

        public static IEnumerable<Deck> Decks => decks.Values;

        public static bool IsDeckExists(string name) => decks.ContainsKey(name);

        public static void AddDeck(Deck deck)
        {
            if (!decks.ContainsKey(deck.Name)) decks.Add(deck.Name, deck);
            else decks[deck.Name] = deck;
        }

        public static Deck Get(string name) => decks[name];

        public string Name { get; set; } = "";

        public List<string> Cards { get; set; } = new List<string>();

        public override string ToString() => Name;
    }
}
