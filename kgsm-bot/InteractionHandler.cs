﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace TheKrystalShip.KGSM;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly AppSettingsManager _settingsManager;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService handler,
        IServiceProvider services,
        AppSettingsManager settingsManager
    )
    {
        _client = client;
        _handler = handler;
        _services = services;
        _settingsManager = settingsManager;
    }

    public async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the
        // InteractionService
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions
        // commands
        _client.InteractionCreated += HandleInteraction;

        // Also process the result of the command execution.
        _handler.InteractionExecuted += HandleInteractionExecute;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        // Register the commands globally.
        // alternatively you can use _handler.RegisterCommandsToGuildAsync()
        // to register commands to a specific guild.
        // await _handler.RegisterCommandsGloballyAsync();

        await _handler.RegisterCommandsGloballyAsync();

        //await _handler.RegisterCommandsToGuildAsync(
        //    _settingsManager.Settings.Discord.GuildId,
        //    true
        //);
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type
            // parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _handler.ExecuteCommandAsync(context, _services);

            // Due to async nature of InteractionFramework, the result here may
            // always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        await interaction.RespondAsync(result.ErrorReason);
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the
            // original interaction acknowledgement will persist.
            //
            // It is a good idea to delete the original response, or at least
            // let the user know that something went wrong during the command
            // execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (m) => await m.Result.DeleteAsync());
        }
    }

    private async Task HandleInteractionExecute(
        ICommandInfo commandInfo,
        IInteractionContext context,
        IResult result
    )
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                default:
                    await context.Interaction.RespondAsync(result.ErrorReason ?? "There was an error");
                    break;
            }
    }
}
