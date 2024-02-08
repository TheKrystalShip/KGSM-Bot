namespace TheKrystalShip.Admiral.Domain
{
    public record Game(string internalName, string displayName, string channelId) {

        public override string ToString()
        {
            return displayName;
        }
    }
}
