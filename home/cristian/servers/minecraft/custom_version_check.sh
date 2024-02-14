#!/bin/bash

################################################################################
# Custom version checker for Terraria, as it doens't go through steam.
#
# INPUT:
# - INSTALLED_VERSION must be passed as a parameter when calling the script
#
# OUTPUT:
# - Exit Code 0: No new version found
# - Exit Code 1: New version found, written to STDERR
################################################################################

# Params
if [ $# -eq 0 ]; then
  echo "Installed version not provided as parameter"
  exit 2
fi

# Params
INSTALLED_VERSION=$1
WORKING_DIR="/home/$USER/servers/minecraft"
MC_VERSIONS_CACHE="$WORKING_DIR/version_cache.json"

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
