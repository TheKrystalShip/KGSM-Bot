#!/bin/bash

################################################################################
# Script used for updating a game server.
# Steps:
#   1. Check if new version is available
#   2. Download new version in temporary folder
#   3. Create backup of running version
#   4. Deploy newly downloaded version
#   5. Restore service state
#   6. Update version number in DB
#
# INPUT:
# - Must provide a game name (must match the name in the DB, folder name,
#   service file name, etc.)
#
# OUTPUT:
# - Exit Code 0: Update successful
# - Exit Code 1-n: Error, check output
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
    echo "Game name not supplied"
    exit 2
fi

SERVICE_NAME=$1
DB_FILE="/home/$USER/servers/info.db"

# Select the entire row, each service only has one row so no need to check
# for multiple rows being returned
result=$(sqlite3 "$DB_FILE" "SELECT * from services WHERE name = '$SERVICE_NAME'")

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

################################################################################
# > Properties
################################################################################

SERVICE_NAME="${COLS[1]}"
SERVICE_WORKING_DIR="${COLS[2]}"
SERVICE_INSTALLED_VERSION="${COLS[3]}"
SERVICE_APP_ID="${COLS[4]}"

# 0 (false), 1 (true)
IS_STEAM_GAME=$(
    ! [ "$SERVICE_APP_ID" != "0" ]
    echo $?
)

BASE_DIR="/home/$USER/servers"
GLOBAL_SCRIPTS_DIR="$BASE_DIR/scripts"
GLOBAL_VERSION_CHECK_FILE="$BASE_DIR/version_check.sh"
SERVICE_LATEST_DIR="$SERVICE_WORKING_DIR/install/latest"

# Create temp folder if it doesn't exist
SERVICE_TEMP_DIR="$SERVICE_WORKING_DIR/install/temp_download"
if [ ! -d "$SERVICE_TEMP_DIR" ]; then
    mkdir -p "$SERVICE_TEMP_DIR"
fi

################################################################################
# > Functions
################################################################################
# Final
function init() {
    echo "Update process started for $SERVICE_NAME."

    # Check if the game is from steam or not, check for a custom_scripts.sh
    # file and if it exists, source it
    if [ "$IS_STEAM_GAME" -eq 0 ]; then
        # Dealing with a non-steam game, source the custom scripts
        echo "INFO: Non-Steam game, importing custom scripts file"

        local custom_scripts_file="$SERVICE_WORKING_DIR/custom_scripts.sh"

        if ! test -f "$custom_scripts_file"; then
            echo ">>> ERROR: Could not locate custom_scripts.sh file for $SERVICE_NAME, exiting."
            exit 1
        fi

        # shellcheck source=/dev/null
        source "$custom_scripts_file"
    fi

    echo "1- Checking for new version..."

    # Check for new version number
    local new_version_number=0
    if type -t custom_run_version_check; then
        echo "Running custom_run_version_check"
        new_version_number=$(custom_run_version_check "$SERVICE_NAME")
    else
        new_version_number=$(run_version_check "$SERVICE_NAME")
    fi

    if [ "$new_version_number" -eq 1 ]; then
        echo ">>> ERROR: No new version found, exiting."
        exit 1
    fi

    echo "New version found: $new_version_number"
    echo "2- Downloading $new_version_number..."

    # Download new version in temp folder
    local is_new_version_downloaded=1
    if type -t custom_run_download; then
        echo "Running custom_run_download"
        is_new_version_downloaded=$(custom_run_download "$SERVICE_NAME")
    else
        is_new_version_downloaded=$(run_download "$SERVICE_NAME")
    fi

    if [ "$is_new_version_downloaded" -eq 1 ]; then
        echo ">>> ERROR: Failed to download new version, exiting."
        exit 2
    fi

    echo "Download finished"
    echo "3- Checking service running status"

    # Check if the service is running before doing anything further
    local service_needs_restoring=false

    local is_service_running=$(run_get_service_state "$PROCESS")
    if [ "$is_service_running" = "active" ]; then
        echo "WARNING: Service currently running, shutting down first..."
        service_needs_restoring=true
    fi

    echo "4- Creating backup of current version..."

    # Backup exiting release
    local backup_folder=$(run_create_backup_folder "$SERVICE_LATEST_DIR")
    if [ "$backup_folder" -eq 1 ]; then
        echo ">>> ERROR: Failed to create backup, exiting."
        exit 3
    fi

    echo "Backup complete in folder: $backup_folder"
    echo "5- Deploying $new_version_number..."

    # Deploy new version
    local is_new_version_deployed=1
    if type -t custom_run_deploy; then
        echo "Running custom_run_deploy"
        is_new_version_deployed=$(custom_run_deploy)
    else
        is_new_version_deployed=$(run_deploy)
    fi

    if [ "$is_new_version_deployed" -eq 1 ]; then
        echo ">>> ERROR: Failed to deploy ${new_version_number}, exiting."
        exit 4
    fi

    echo "Deployment complete."

    # Restore previous service state
    if [ "$service_needs_restoring" ]; then
        echo "5.5- Starting the service back up"

        local restore_service_result=0
        restore_service_result=$(run_service_restore "$PROCESS")
        if [ "$restore_service_result" -eq 1 ]; then
            echo ">>> ERROR: Failed to restore service to running state, exiting."
            exit 5
        fi

        echo "Service started successfully"
    fi

    # Update version number
    if [ "$(run_version_update "$new_version_number")" -eq 1 ]; then
        echo ">>> ERROR: Failed to update version number in DB"
    fi

    echo "6- Update finished, exiting"
    exit 0
}

# Overridable
function run_version_check() {

    local service=$1
    local latest_version_available=1

    latest_version_available=$(exec "$GLOBAL_VERSION_CHECK_FILE" "$service")

    # versionCheck returns exit code 1 if there's no new version.
    if [ "$latest_version_available" -eq 1 ]; then
        return 1
    fi

    # New version number will be saved, echo it
    echo "$latest_version_available"
}

# Overridable
function run_download() {
    local new_version=$1

    # Read from ENV
    local user=$STEAM_USERNAME
    local password=$STEAM_PASSWORD
    local update_exit_code=1

    update_exit_code=$(steamcmd +@sSteamCmdForcePlatformType linux +force_install_dir "$SERVICE_TEMP_DIR" +login "$user" "$password" +app_update "$SERVICE_APP_ID" -beta none validate +quit)
    if [ "$update_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

# Final
function run_get_service_state() {
    # Default to "inactive" just to be sure it won't start up after updating
    local service=$1
    local service_state="inactive"

    # Possible output: "active" / "inactive"
    service_state=$(exec "$GLOBAL_SCRIPTS_DIR/is-active.sh" "$service")

    echo "$service_state"
}

# Final
function run_create_backup_folder() {

    local service=$1
    local source_dir=$SERVICE_LATEST_DIR
    local installed_version=$SERVICE_INSTALLED_VERSION

    # Create backup
    local datetime=""
    datetime=$(exec date +"%Y-%m-%d%T")
    local destination_folder="${SERVICE_WORKING_DIR}/install/${installed_version}-backup.${datetime}"

    # Create backup folder if it doesn't exit
    if [ ! -d "$destination_folder" ]; then
        mkdir -p "$destination_folder"
    fi

    # Move currently deployed version to backup folder
    mv_command_exit_code=$(exec mv -f "$source_dir" "$destination_folder")

    if [ "$mv_command_exit_code" -ne 0 ]; then
        return 1
    fi

    echo "$destination_folder"
    return 0
}

# Overridable
function run_deploy() {
    # Ensure 'latest' folder actually exists
    if [ ! -d "$SERVICE_LATEST_DIR" ]; then
        if ! mkdir -p "$SERVICE_LATEST_DIR"; then
            echo ">>> ERROR: Error creating $SERVICE_LATEST_DIR folder"
            return 1
        fi
    fi

    # Move newly downloaded version into "latest" folder
    mv_deploy_version_exit_code=$(mv "$SERVICE_TEMP_DIR" "$SERVICE_LATEST_DIR")

    if [ "$mv_deploy_version_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

# Final
function run_service_restore() {
    # Start up service
    local service=$1
    local service_start_exit_code=0
    service_start_exit_code=$(exec "$GLOBAL_SCRIPTS_DIR/start.sh" "$service")

    if [ "$service_start_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

# Final
function run_version_update() {
    # Save new version number in DB
    local new_version=$1
    local sql_script="UPDATE services \
                      SET installed_version = '${new_version}' \
                      WHERE name = '${SERVICE_NAME}';"

    sql_execute_result=$(sqlite3 "$DB_FILE" "$sql_script")
    if [ "$sql_execute_result" -ne 0 ]; then
        return 1
    fi

    return 0
}

function run_ctrl_c() {
    # shellcheck disable=SC2317
    echo "** Update process cancelled **"
}

# Trap CTRL-C
trap run_ctrl_c INT

# Start the script
init
