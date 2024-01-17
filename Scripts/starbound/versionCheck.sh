#!/bin/bash

# Params
APP_ID="211820"
WORKING_DIR="/home/$USER/servers/starbound"
VERSION_FILE="$WORKING_DIR/installed_version"

# Check for existing file or create new
test -f "$VERSION_FILE" || touch "$VERSION_FILE"

# Read old version number
INSTALLED_VERSION=$(cat "$VERSION_FILE")

# Fetch latest version from Steam
NEW_VERSION=$(steamcmd +login anonymous +app_info_update 1 +app_info_print $APP_ID +quit | tr '\n' ' ' | grep --color=NEVER -Po '"branches"\s*{\s*"public"\s*{\s*"buildid"\s*"\K(\d*)')

# If new version exists, echo it and exit 1
if [ "$NEW_VERSION" != "$INSTALLED_VERSION" ]; then
    # Output new version number to stderr
    echo >&2 "$NEW_VERSION"
    exit 1
fi

exit 0
