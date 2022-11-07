namespace VX.PlanningPoker.Data
{
    public class Card
    {
        public string Title { get; set; }

        public double? Value { get; set; }

        public static bool operator ==(Card? arg1, Card? arg2) => arg1?.Title == arg2?.Title;

        public static bool operator !=(Card? arg1, Card? arg2) => arg1?.Title != arg2?.Title;

        public override bool Equals(object? obj) => Title == (obj as Card)?.Title;

        public override int GetHashCode() => Title.GetHashCode();

        public override string ToString() => Title;
    }
}
