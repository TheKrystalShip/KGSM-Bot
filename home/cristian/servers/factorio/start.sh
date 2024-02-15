#!/bin/bash

WORKING_DIR="/home/$USER/servers/factorio/install/latest"

exec "$WORKING_DIR/bin/x64/factorio" --start-server "$WORKING_DIR/saves/tks.zip"
