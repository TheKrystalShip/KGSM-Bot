namespace TheKrystalShip.KGSM.Domain
{
    public record Game(string internalName, string displayName, string channelId)
    {
        public Game(string internalName) : this(internalName, "", "") {}

        public override string ToString()
        {
            return displayName;
        }
    }
}
