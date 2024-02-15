#!/bin/bash

WORKING_DIR="/home/$USER/servers/starbound/install/latest"

# Move into directory because server uses pwd as a config
(cd "$WORKING_DIR/linux" && exec ./starbound_server)
