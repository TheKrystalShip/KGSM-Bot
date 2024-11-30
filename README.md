# KGSM-Bot

KGSM-Bot is a Discord bot designed to interact with [KGSM][1], providing an 
intuitive interface to manage and control game servers directly from Discord.

## Features

- Server management: Install, start, stop, uninstall game servers through 
Discord commands.
- Live notifications: Realtime notifications for game server startup and 
shutdown.

## Getting started

KGSM-Bot was designed to work on GNU/Linux systems. There is no compatibility 
with Windows, try at your own risk.

### Prerequisites

- Discord Bot Token: Create a bot via the [Discord Developer Portal][2]
- [KGSM][1], version 1.6+

### Configuration

Copy the contents of `appsettings.example.json` into a new file, 
`appsettings.json` and fill in the configuration.

```json
{
    "Discord": {
        "Token": "YOUR_DISCORD_TOKEN_HERE_DO_NOT_SHARE",
        ...
    },
    "KGSM": {
        "Path": "path/to/kgsm.sh",
        "SocketPath": "path/to/kgsm.sock"
    }
}
```

### Installation

1. Clone the repository:
```sh
git clone https://github.com/TheKrystalShip/KGSM-Bot.git

```

2. Navigate to the project directory
```sh
cd KGSM-Bot
```

3. Install dependencies
```sh
dotnet restore
```

4. Build the project
```sh
dotnet run
```

## Usage

Run the bot using:

```sh
dotnet run
```

Invite the bot to your Discord server an use commands, such as:
- `/install [blueprint]`
- `/start [instance]`
- `/status [instance]`
- `/uninstall [instance]`

## Contributing

Contributions are welcome! Plase open an issue or submit a pull request.

## License

This project is licensed under the MIT license. See the [LICENSE][3] file for 
details.

## Acknowledgements
- [Discord.Net][4] for the Discord API wrapper.
- [KGSM][1] fot the game server management.

[1]: https://github.com/TheKrystalShip/KGSM
[2]: https://discord.com/developers/applications
[3]: LICENSE
[4]: https://github.com/discord-net/Discord.Net
