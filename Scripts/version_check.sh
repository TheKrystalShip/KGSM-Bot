#!/bin/bash

################################################################################
# Script to check if a new version of a game has been released, comparing it
# to the currently running version on the server.
# Will work for all steam games and also custom games not on steam
# (provided they offer a script to check for new releases).
#
# INPUT:
# - Must provide a service name (must match the name in the DB, folder name,
#   service file name, etc.)
#
# OUTPUT:
# - Exit Code 0: No new version found
# - Exit Code 1: New version found, written to STDERR
# - Exit Code 2: Other error, check output
#
# DB table schema for reference
#┌──┬────┬───────────┬─────────────────┬──────┐
#│0 | 1  | 2         | 3               | 4    │
#├──┼────┼───────────┼─────────────────┼──────┤
#|id|name|working_dir|installed_version|app_id|
#└──┴────┴───────────┴─────────────────┴──────┘
################################################################################

# Params
if [ $# -eq 0 ]; then
    echo "Service name not supplied"
    exit 2
fi

SERVICE=$1
DB_FILE="/home/$USER/servers/info.db"

# Select the entire row, each service only has one row so no need to check
# for multiple rows being returned
result=$(sqlite3 "$DB_FILE" "SELECT * from services WHERE name = '$SERVICE'")

if [ -z "$result" ]; then
    echo "Didn't get any result back from DB, exiting"
    exit 2
fi

# Result is a string with all values glued together by a | character, split
IFS='|' read -r -a COLS <<<"$result"

if [ -z "${COLS[0]}" ]; then
    echo "Failed to parse result, exiting"
    exit 2
fi

# $COLS is now an array, all indexes match the DB schema described above.
# Example: echo "${COLS[4]}" would return the app_id

# Read currently installed version number
INSTALLED_VERSION="${COLS[3]}"
NEW_VERSION="0"

# If app_id is 0, the game is not from steam
APP_ID="${COLS[4]}"

if [ "$APP_ID" -ne '0' ]; then
    # It's a steam game, get the app_id and use steam to check for new version
    NEW_VERSION=$(steamcmd +login anonymous +app_info_update 1 +app_info_print "$APP_ID" +quit | tr '\n' ' ' | grep --color=NEVER -Po '"branches"\s*{\s*"public"\s*{\s*"buildid"\s*"\K(\d*)')
else
    # Non-steam game, will have custom way to check for new version, use that
    working_dir="${COLS[2]}"
    custom_version_check_file="$working_dir/custom_version_check.sh"

    if test -f "$custom_version_check_file"; then
        # Custom file exists, run in
        NEW_VERSION=$(exec "$custom_version_check_file" "$INSTALLED_VERSION")
    else
        # No custom file found, error and exit
        echo "No custom_version_check file found for $SERVICE, exiting"
        # Important to exit with something other than 0 or 1
        exit 2
    fi
fi

# If new version exists, echo it and exit 1
if [ "$NEW_VERSION" != "$INSTALLED_VERSION" ]; then
    # Output new version number to stderr
    echo >&2 "$NEW_VERSION"
    exit 1
fi

exit 0
