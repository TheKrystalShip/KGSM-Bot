#!/bin/bash

# Params
WORKING_DIR="/home/$USER/servers/terraria"
VERSION_FILE="$WORKING_DIR/installed_version"

# Check for existing file or create new
test -f "$VERSION_FILE" || touch "$VERSION_FILE"

# Read old version number
INSTALLED_VERSION=$(cat "$VERSION_FILE")

# Fetch latest version from Steam
NEW_VERSION_FULL_NAME=$(curl -s 'https://terraria.org/api/get/dedicated-servers-names' | python3 -c "import sys, json; print(json.load(sys.stdin)[0])")
# Expected: terraria-server-1449.zip

# Do some parsing from the file name
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
