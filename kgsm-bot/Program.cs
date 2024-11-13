namespace TheKrystalShip.KGSM;

public class Program
{
    public static async Task Main(string[] args)
    {
        await new KgsmBotStartup().RunAsync(args);
    }
}
