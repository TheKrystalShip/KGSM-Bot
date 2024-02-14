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
# run_version_check
# run_download
# run_deploy
################################################################################

custom_run_version_check() {
    ############################################################################
    # Custom version checker for Terraria, as it doesn't go through steam.
    #
    # INPUT:
    # - INSTALLED_VERSION must be passed as a parameter when calling the script
    #
    # OUTPUT:
    # - Exit Code 0: No new version found
    # - Exit Code 1: New version found, written to STDOUT
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
    # Custom version checker for Terraria, as it doesn't go through steam.
    #
    # INPUT:
    # - NEW_VERSION must be passed as a parameter when calling the script
    #
    # OUTPUT:
    # - Exit Code 0: New version downloaded successfully
    # - Exit Code 1: Error, check output
    ############################################################################
    local new_version=$1

    # Create temp folder if it doesn't exist
    if [ ! -d "$TEMP_DOWNLOAD_FOLDER" ]; then
        echo ">>> WARNING: $TEMP_DOWNLOAD_FOLDER doesn't exist, creating it now"
        mkdir -p "$TEMP_DOWNLOAD_FOLDER"
    fi

    local wget_exit_code=$(exec wget "https://terraria.org/api/download/pc-dedicated-server/terraria-server-${new_version}.zip")
    if [ "$wget_exit_code" -ne 0 ]; then
        echo ">>> ERROR: wget https://terraria.org/api/download/pc-dedicated-server/terraria-server-${new_version}.zip command didn't finish with code 0, exiting"
        return 1
    fi

    local unzip_exit_code=$(exec unzip "terraria-server-${new_version}.zip" -d "$TEMP_DOWNLOAD_FOLDER")
    if [ "$unzip_exit_code" -ne 0 ]; then
        echo ">>> ERROR: unzip terraria-server-${new_version}.zip -d $TEMP_DOWNLOAD_FOLDER command didn't finish with code 0, exiting"
        return 1
    fi

    local rm_exit_code=$(exec rm "terraria-server-${new_version}.zip")
    if [ "$rm_exit_code" -ne 0 ]; then
        echo ">>> ERROR: 'rm terraria-server-${new_version}.zip' command didn't finish with code 0, exiting"
        return 1
    fi

    return 0
}
