#!/bin/bash

WORKING_DIR="/home/$USER/servers/terraria"

exec "$WORKING_DIR/install/latest/Linux/TerrariaServer.bin.x86_64" -config "$WORKING_DIR/install/serverconfig.txt"
