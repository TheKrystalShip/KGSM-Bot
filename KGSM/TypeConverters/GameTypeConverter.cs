using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

using TheKrystalShip.Logging;

using System.Text.RegularExpressions;

namespace TheKrystalShip.KGSM;

internal sealed partial class GameTypeConverter : TypeConverter
{
    private readonly Logger<GameTypeConverter> _logger;
    private readonly IConfiguration _configuration;

    [GeneratedRegex("[^a-zA-Z0-9_.-]+", RegexOptions.Compiled)]
    private static partial Regex ChannelNameRegex();

    public GameTypeConverter(IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = new();
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
        string channelName = (option.Value as SocketGuildChannel)?.Name ?? string.Empty;

        if (channelName == string.Empty)
        {
            _logger.LogError($"Failed to load channel name");
            return Task.FromResult(TypeConverterResult.FromError(new ArgumentNullException(nameof(channelName))));
        }

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
