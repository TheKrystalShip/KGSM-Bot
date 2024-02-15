#!/bin/bash

WORKING_DIR="/home/$USER/servers/factorio/install"

exec "$WORKING_DIR/bin/x64/factorio" --start-server "$WORKING_DIR/saves/tks.zip"
