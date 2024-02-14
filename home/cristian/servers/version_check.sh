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
# - Exit Code 0: New version found, written to STDOUT
# - Exit Code 1: No new version
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
    echo "ERROR: Service name not supplied"
    exit 2
fi

SERVICE=$1
DB_FILE="/home/$USER/servers/info.db"

# Select the entire row, each service only has one row so no need to check
# for multiple rows being returned
result=$(sqlite3 "$DB_FILE" "SELECT * from services WHERE name = '$SERVICE'")

if [ -z "$result" ]; then
    echo "ERROR: Didn't get any result back from DB, exiting"
    exit 2
fi

# Result is a string with all values glued together by a | character, split
IFS='|' read -r -a COLS <<<"$result"

if [ -z "${COLS[0]}" ]; then
    echo "ERROR: Failed to parse result, exiting"
    exit 2
fi

# $COLS is now an array, all indexes match the DB schema described above.
SERVICE_NAME="${COLS[1]}"
SERVICE_WORKING_DIR="${COLS[2]}"
SERVICE_INSTALLED_VERSION="${COLS[3]}"
SERVICE_APP_ID="${COLS[4]}"

# 0 (false), 1 (true)
IS_STEAM_GAME=$(
    ! [ "$SERVICE_APP_ID" != "0" ]
    echo $?
)

function init() {
    local new_version_number="0"

    if [ "$IS_STEAM_GAME" -eq '1' ]; then
        # It's a steam game, get the app_id and use steam to check for new version
        echo "Running Steam version check..."
        new_version_number=$(steamcmd +login anonymous +app_info_update 1 +app_info_print "$SERVICE_APP_ID" +quit | tr '\n' ' ' | grep --color=NEVER -Po '"branches"\s*{\s*"public"\s*{\s*"buildid"\s*"\K(\d*)')
    else
        # Non-steam game, will have custom way to check for new version, use that
        echo "Running Custom version check..."
        local custom_scripts_file="$SERVICE_WORKING_DIR/custom_scripts.sh"

        if test -f "$custom_scripts_file"; then
            # Custom file exists, source it

            # shellcheck source=/dev/null
            source "$custom_scripts_file"
        else
            echo "ERROR: No custom_scripts file found for $SERVICE_NAME, exiting"
            exit 2
        fi

        if type -t custom_run_version_check; then
            new_version_number=$(custom_run_version_check "$SERVICE_NAME")
        else
            echo "Error: No custom version check function found, exiting"
            exit 2
        fi
    fi

    if [ "$new_version_number" != "$SERVICE_INSTALLED_VERSION" ]; then
        echo "$new_version_number" | tr -d '\n'
        exit 0
    fi

    exit 1
}

init
