#!/bin/bash

trap 'kill 0' EXIT

echo "*** Status Watchdog ***"

./log-reader.sh --service "7dtd" --online-string "INF Loaded (local): worldglobal" --offline-string "Stopped 7 Days To Die Dedicated Server" &

./log-reader.sh --service "corekeeper" --online-string "Game ID" --offline-string "Stopped CoreKeeper Dedicated Server" &

./log-reader.sh --service "factorio" --online-string "Hosting game at IP ADDR" --offline-string "Stopped Factorio Dedicated Server" &

./log-reader.sh --service "minecraft" --online-string "For help, type \"help\"" --offline-string "Stopped Minecraft Dedicated Server" &

./log-reader.sh --service "projectzomboid" --online-string "SERVER STARTED" --offline-string "Stopped Project Zomboid Dedicated Server" &

./log-reader.sh --service "starbound" --online-string "UniverseServer: listening for incoming TCP connections" --offline-string "Stopped Starbound Dedicated Server" &

./log-reader.sh --service "terrarria" --online-string "Server started" --offline-string "Stopped Terraria Dedicated Server" &

./log-reader.sh --service "valheim" --online-string "unused Assets to reduce memory usage. Loaded Objects now" --offline-string "Stopped Valheim Dedicated Server" &

wait
