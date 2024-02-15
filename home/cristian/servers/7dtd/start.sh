#!/bin/bash

WORKING_DIR="/home/cristian/servers/7dtd/install/latest"
CONFIG_FILE="$WORKING_DIR/serverconfig.xml"

exec "$WORKING_DIR"/7DaysToDieServer.x86_64 -quit -batchmode -nographics -headless -dedicated -configfile="$CONFIG_FILE"
