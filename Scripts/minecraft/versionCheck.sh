#!/bin/bash

# Working directory
WORKING_DIR="/home/$USER/servers/minecraft"
MC_VERSIONS_CACHE="$WORKING_DIR/version_cache.json"
VERSION_FILE="$WORKING_DIR/installed_version"

# Check for existing file or create new
test -f "$VERSION_FILE" || touch "$VERSION_FILE"

# Read old version number
INSTALLED_VERSION=$(cat "$VERSION_FILE")

# Fetch latest version manifest
curl -sS https://launchermeta.mojang.com/mc/game/version_manifest.json >"$MC_VERSIONS_CACHE"

NEW_VERSION=$(cat "$MC_VERSIONS_CACHE" | jq -r '{latest: .latest.release} | .[]')

# Cleanup
rm "$MC_VERSIONS_CACHE"

# If new version found, echo it and exit 1
if [ "$INSTALLED_VERSION" != "$NEW_VERSION" ]; then
  # Output new version number to stderr
  echo >&2 "$NEW_VERSION"
  exit 1
fi

exit 0
