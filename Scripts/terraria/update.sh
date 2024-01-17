#!/bin/bash

################################################################################
# > MODIFY THIS
################################################################################
PROCESS="terraria"

################################################################################
# > Don't modify further
################################################################################

################################################################################
# > Properties
################################################################################

BASE_DIR="/home/$USER/servers"
WORKING_DIR="$BASE_DIR/$PROCESS"
SCRIPTS_FOLDER="$BASE_DIR/scripts"
LATEST_INSTALLED_DIR="$WORKING_DIR/install/latest"
TEMP_DOWNLOAD_FOLDER="$WORKING_DIR/install/temp_download"
INSTALLED_VERSION_FILE="$WORKING_DIR/installed_version"
VERSION_CHECK_FILE="$WORKING_DIR/versionCheck.sh"

################################################################################
# > Funcitions
################################################################################
init() {
    echo "Working directory is $WORKING_DIR"
    echo "Checking for new version..."

    # Check for new version number
    new_version_number="$(is_new_version_available "$VERSION_CHECK_FILE")"
    if [ "$new_version_number" -eq 0 ]; then
        echo "No new version found, exiting."
        exit 1
    fi

    echo "New version found: $new_version_number"
    echo "Downloading $new_version_number..."

    # Download new version in temp folder
    is_new_version_downloaded="$(download_version "$new_version_number")"
    if [ "$is_new_version_downloaded" -eq 1 ]; then
        echo "Failed to download new version, exiting."
        exit 2
    fi

    echo "Download finished"
    echo "Checking service running status"

    # Check if the service is running before doing anything further
    local service_needs_restoring=false

    is_service_running=$(get_service_state $PROCESS)
    if [ "$is_service_running" = "active" ]; then
        echo "Service currently running, shutting down first..."
        service_needs_restoring=true
    fi

    echo "Creating backup of current version..."

    # Backup exiting release
    backup_folder="$(get_backup_folder "$LATEST_INSTALLED_DIR")"
    if [ "$backup_folder" -eq 1 ]; then
        echo "Failed to create backup, exiting."
        exit 3
    fi

    echo "Backup complete in folder: $backup_folder"
    echo "Deploying $new_version_number..."

    # Deploy new version
    is_new_version_deployed=$(deploy_new_version)
    if [ "$is_new_version_deployed" -eq 1 ]; then
        echo "Failed to deploy ${new_version_number}, exiting."
        exit 4
    fi

    echo "Deployment complete."

    # Update version number in installed_version file
    update_version_file "$new_version_number"

    # Restore previous service state
    if [ "$service_needs_restoring" ]; then
        echo "Starting the service back up"

        local restore_service_result=0
        restore_service_result=$(restore_service_state $PROCESS)
        if [ "$restore_service_result" -eq 1 ]; then
            echo "Failed to restore service to running state, exiting."
            exit 5
        fi

        echo "Service started successfully"
    fi

    echo "Update finished successfully, exiting"
    exit 0
}

is_new_version_available() {

    # Grab from param $1
    local version_check_file=$1

    # Check if a new version has been found
    local latest_version_available=0
    latest_version_available=$(exec "$version_check_file")

    # versionCheck returns exit code 0 if there's no new version.
    if [ $? -eq 0 ]; then
        echo 0
    fi

    # New version number will be saved, return it
    echo "$latest_version_available"
}

download_version() {
    local new_version=$1

    # Create temp folder if it doesn't exist
    if [ ! -d "$TEMP_DOWNLOAD_FOLDER" ]; then
        mkdir -p "$TEMP_DOWNLOAD_FOLDER"
    fi

    wget_exit_code=$(exec wget "https://terraria.org/api/download/pc-dedicated-server/terraria-server-${new_version}.zip")

    if [ "$wget_exit_code" -ne 0 ]; then
        return 1
    fi

    unzip_exit_code=$(exec unzip "terraria-server-${new_version}.zip" -d "$TEMP_DOWNLOAD_FOLDER")

    if [ "$unzip_exit_code" -ne 0 ]; then
        return 1
    fi

    rm_exit_code=$(exec rm "terraria-server-${new_version}.zip")

    if [ "$rm_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

get_service_state() {
    # Default to "inactive" just to be sure it won't start up after updating
    local service_state="inactive"

    # Possible output: "active" / "inactive"
    service_state=$(exec "$SCRIPTS_FOLDER/is-active.sh $1")

    echo "$service_state"
}

get_backup_folder() {

    local source=$1

    # Get currently installed version
    local installed_version=""
    installed_version=$(cat "$WORKING_DIR/installed_version")

    # Create backup
    local datetime=""
    datetime=$(exec date +"%Y-%m-%d%T")
    local destination_folder="${WORKING_DIR}/install/${installed_version}-backup.${datetime}"

    # Create backup folder if it doesn't exit
    if [ ! -d "$destination_folder" ]; then
        mkdir -p "$destination_folder"
    fi

    # Move currently deployed version to backup folder
    mv_command_exit_code=$(exec mv -f "$source" "$destination_folder")

    if [ "$mv_command_exit_code" -ne 0 ]; then
        return 1
    fi

    echo "$destination_folder"
    return 0
}

deploy_new_version() {
    # Ensure 'latest' folder actually exists
    if [ ! -d "$LATEST_INSTALLED_DIR" ]; then
        if ! mkdir -p "$LATEST_INSTALLED_DIR"; then
            echo "Error creating $LATEST_INSTALLED_DIR folder"
            return 1
        fi
    fi

    # Move newly downloaded version into "latest" folder
    mv_deploy_version_exit_code=$(mv "$TEMP_DOWNLOAD_FOLDER" "$LATEST_INSTALLED_DIR")

    if [ "$mv_deploy_version_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

restore_service_state() {
    # Start up service
    local service_start_exit_code=0
    service_start_exit_code=$(exec "$SCRIPTS_FOLDER/start.sh $1")

    if [ "$service_start_exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

update_version_file() {
    echo "$1" >"$INSTALLED_VERSION_FILE"
}

# Start the script
init
