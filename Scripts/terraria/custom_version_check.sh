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

INSTALLED_VERSION=$1

# Fetch latest version

NEW_VERSION_FULL_NAME=$(curl -s 'https://terraria.org/api/get/dedicated-servers-names' | python3 -c "import sys, json; print(json.load(sys.stdin)[0])")
# Expected: terraria-server-1449.zip

IFS='-' read -r -a new_version_unformatted <<<"$NEW_VERSION_FULL_NAME"
TEMP=${new_version_unformatted[2]}
# Expected: 1449.zip

IFS='.' read -r -a version_number <<<"$TEMP"
NEW_VERSION=${version_number[0]}
# Expected: 1449

# If new version exists, echo it and exit 1
if [ "$NEW_VERSION" != "$INSTALLED_VERSION" ]; then
  # Output new version number to stderr
  echo >&2 "$NEW_VERSION"
  exit 1
fi

exit 0
