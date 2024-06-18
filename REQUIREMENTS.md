List of actions the bot must be able to do

## Hard requirements

- ✅ Connect to discord
- ✅ Receive commands via discord messages in a specific channel in order for all commands to be publicly seen.
- ✅ Must be able to start/stop/restart/status game servers
- ✅ Change discord channel name to represent the change on game server status. (🟢online, 🔴offline)
- ✅ Detect game server status on boot and update discord accordingly
- ✅ Must run as a service and start @ boot

## Soft requirements

- ❌ Detect when server has a restart pending and automatically update _all_ game servers to "🟡restarting" status.
  - Not possible due to discord rate limiting
- ❌ Check for new game server versions and update discord status to "🔵needs updating".
- ❌ Update servers using discord command (scripts are already in place, they just need to be called when there's an update)
  - Updates are better handled manually because of config/mods.
- ✅ Periodically run a status check on all servers to make sure the status is correctly reflected on discord.
  - Handled externally

## Nice to have

- Send commands to running servers. (Example: `/input terraria save`).
