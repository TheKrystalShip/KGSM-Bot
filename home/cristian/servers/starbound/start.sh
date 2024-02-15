#!/bin/bash

WORKING_DIR="/home/$USER/servers/starbound"

# Move into directory because server uses pwd as a config
(cd "$WORKING_DIR/install/linux" && exec ./starbound_server)
