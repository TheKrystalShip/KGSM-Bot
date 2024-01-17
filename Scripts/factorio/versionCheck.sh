#!/bin/bash

# Not currently implemented
exit 0

# Params
WORKING_DIR="/home/$USER/servers/factorio"
VERSION_FILE="${WORKING_DIR}/installed_version"

# Check for existing file or create new
test -f $VERSION_FILE || touch $VERSION_FILE

# Read old version number
INSTALLED_VERSION=$(cat $VERSION_FILE)

# TODO: Fetch latest version
NEW_VERSION='0'

# If new version exists, echo it and exit 1
if [ "$NEW_VERSION" != "$INSTALLED_VERSION" ]; then
    echo >&2 "$NEW_VERSION"
    exit 1
fi

exit 0
