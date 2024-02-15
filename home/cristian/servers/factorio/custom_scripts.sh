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
    local new_version=$1

    # If new version exists, echo it and exit 1
    if [ "$new_version" != "$installed_version" ]; then
        echo "$new_version"
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

    # Downaload and unpack latest version of the stable headless server
    wget https://factorio.com/get-download/stable/headless/linux64 -O "$SERVICE_TEMP_DIR/factorio_headless.tar.xz"
    tar -xf "$SERVICE_TEMP_DIR/factorio_headless.tar.xz" --strip-components=1 -C "$SERVICE_TEMP_DIR"
    rm factorio_headless.tar.xz

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
