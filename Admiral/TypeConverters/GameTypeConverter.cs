using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

using System.Text.RegularExpressions;

namespace TheKrystalShip.Admiral;

internal sealed partial class GameTypeConverter : TypeConverter
{
    private readonly IConfiguration _configuration;

    [GeneratedRegex("[^a-zA-Z0-9_.]+", RegexOptions.Compiled)]
    private static partial Regex ChannelNameRegex();

    public GameTypeConverter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override bool CanConvertTo(Type type)
    {
        return type == typeof(Domain.Game);
    }

    public override ApplicationCommandOptionType GetDiscordType()
    {
        return ApplicationCommandOptionType.Channel;
    }

    public override Task<TypeConverterResult> ReadAsync(
        IInteractionContext context,
        IApplicationCommandInteractionDataOption option,
        IServiceProvider services
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(
                (option.Value as SocketGuildChannel)?.Name ?? string.Empty,
                string.Empty
            );

        Domain.Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        return Task.FromResult(TypeConverterResult.FromSuccess(game));
    }
}
