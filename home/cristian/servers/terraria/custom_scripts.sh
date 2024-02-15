#!/bin/bash

################################################################################
# Main file: /home/$USER/servers/update.sh
#
# These are the functions available in the main script file.
# The script will look for any of these functions that start with custom_
# and run them instead of the original ones.
#
# Example: custom_run_download, custom_run_create_backup_folder, etc.
#
# Methods that can be "overwritten":
#
# run_version_check
# run_download
# run_deploy
#
# Available vars:
#
# DB_FILE
# SERVICE_NAME
# SERVICE_WORKING_DIR
# SERVICE_INSTALLED_VERSION
# SERVICE_APP_ID
# IS_STEAM_GAME
# BASE_DIR
# GLOBAL_SCRIPTS_DIR
# GLOBAL_VERSION_CHECK_FILE
# SERVICE_LATEST_DIR
# SERVICE_TEMP_DIR
# SERVICE_BACKUPS_FOLDER
################################################################################

custom_run_version_check() {
    ############################################################################
    # INPUT:
    # - $1: Installed version
    #
    # OUTPUT:
    # - 0: No new version found
    # - 1: New version found, written to STDERR
    ############################################################################
    local installed_version=$1
    local newest_version=0

    # Fetch latest version
    local newest_version_full_name=$(curl -s 'https://terraria.org/api/get/dedicated-servers-names' | python3 -c "import sys, json; print(json.load(sys.stdin)[0])")
    # Expected: terraria-server-1449.zip
    IFS='-' read -r -a new_version_unformatted <<<"$newest_version_full_name "
    local temp=${new_version_unformatted[2]}
    # Expected: 1449.zip

    IFS='.' read -r -a version_number <<<"$temp"
    newest_version=${version_number[0]}
    # Expected: 1449

    # If new version exists, echo it and exit 1
    if [ "$newest_version" != "$installed_version" ]; then
        # Output new version number to stderr
        echo "$newest_version" | tr -d '\n'
        return 1
    fi

    return 0
}

custom_run_download() {
    ############################################################################
    # INPUT:
    # - $1: New version
    #
    # OUTPUT:
    # - 0: Success
    # - 1: Error
    ############################################################################

    local new_version=$1

    local wget_exit_code=$(exec wget "https://terraria.org/api/download/pc-dedicated-server/terraria-server-${new_version}.zip")
    if [ "$wget_exit_code" -ne 0 ]; then
        echo ">>> ERROR: wget https://terraria.org/api/download/pc-dedicated-server/terraria-server-${new_version}.zip command didn't finish with code 0, exiting"
        return 1
    fi

    local unzip_exit_code=$(exec unzip "terraria-server-${new_version}.zip" -d "$SERVICE_TEMP_DIR")
    if [ "$unzip_exit_code" -ne 0 ]; then
        echo ">>> ERROR: unzip terraria-server-${new_version}.zip -d $SERVICE_TEMP_DIR command didn't finish with code 0, exiting"
        return 1
    fi

    local rm_exit_code=$(exec rm "terraria-server-${new_version}.zip")
    if [ "$rm_exit_code" -ne 0 ]; then
        echo ">>> ERROR: 'rm terraria-server-${new_version}.zip' command didn't finish with code 0, exiting"
        return 1
    fi

    return 0
}

custom_run_deploy() {
    ############################################################################
    # INPUT:
    # - $1: Source folder, absolute path
    # - $2: Destination folder, absolute path
    #
    # OUTPUT:
    # - 0: Success
    # - 1: Error
    ############################################################################
    local source=$1
    local destination=$2

    return 1
}
