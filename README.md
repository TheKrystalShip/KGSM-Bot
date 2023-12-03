# Admiral

A little discord bot meant to control game servers through `systemctl` commands.

## Development requirements

## To run

## Game servers folder structure

Service & socket files are located in `/etc/systemd/system`:
```
/etc/systemd/system
├── 7dtd.service
├── factorio.service
├── projectzomboid.service
├── projectzomboid.socket
├── starbound.service
├── terraria.service
├── terraria.socket
└── valheim.service
```

All `*.service` files call `/home/[USER]/servers/[GAME]/start.sh` for starting up a game server.
Some of the `*.service` files have custom commands and settings in order to cater to whatever the game server needs when running or shutting down (Example: execute a world save before stopping).
Once set up they don't need to be modified.

Game server files are located under `/home/[USER]/servers`:
```
/home/[USER]/servers
├── 7dtd
│   ├── install
│   ├── installed_version
│   ├── start.sh
│   ├── update.sh
│   └── versionCheck.sh
├── corekeeper
│   ├── install
│   ├── installed_version
│   ├── start.sh
│   ├── update.sh
│   └── versionCheck.sh
├── factorio
│   ├── install
│   ├── start.sh
│   ├── update.sh
│   └── versionCheck.sh
├── projectzomboid
│   ├── install
│   ├── installed_version
│   ├── projectzomboid.stdin
│   ├── start.sh
│   ├── stop.sh
│   ├── update.sh
│   └── versionCheck.sh
├── starbound
│   ├── install
│   ├── start.sh
│   ├── update.sh
│   └── versionCheck.sh
├── terraria
│   ├── install
│   ├── installed_version
│   ├── start.sh
│   ├── stop.sh
│   ├── terraria.stdin
│   ├── update.sh
│   └── versionCheck.sh
└── valheim
    ├── install
    ├── installed_version
    ├── start.sh
    ├── update.sh
    └── versionCheck.sh
```

Note that all servers have some important files:

- `update.sh` is used to fetch the latest version of the game from the source (either steam directly or their own download sites) and extract it in the current folder. So these files are self-contained and made specifically for each game.
This file is executed manually, no service is aware of it.

- `start.sh` is called by the service file in order to start up the server. These files are created with specific configuration for each game. By having this file start up the game server with whatever config is needed, we avoid having to reload `systemctl` in order to detect changes done to the services. Just modify the `start.sh` file, save and you can immediately run `systemctl start [GAME].service`.

Terraria & Project Zomboid a `stop.sh` file which is used to issue commands to the running service because they require a world save before shutting down. They have an active socket where commands can be written to and passed to the service process in order to gracefully shut down.
All of this is handled by `systemctl` so there's no need to do anything from the coding side.

These sockets (`*.stdin`) are automatically opened/closed alongside the service thanks to https://unix.stackexchange.com/a/730423

- `versionCheck.sh` fetches the latest version from the source (Steam or other) and updates the `installed_version` file.
DOES NOT YET UPDATE THE GAMES

- `install` is the folder containing the game installation.
