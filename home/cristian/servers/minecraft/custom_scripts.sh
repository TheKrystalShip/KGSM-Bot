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

    local mc_versions_cache="$SERVICE_TEMP_DIR/version_cache.json"

    # Fetch latest version manifest
    curl -sS https://launchermeta.mojang.com/mc/game/version_manifest.json >"$mc_versions_cache"

    new_version=$(cat "$mc_versions_cache" | jq -r '{latest: .latest.release} | .[]')

    # Cleanup
    rm "$mc_versions_cache"

    if [ "$installed_version" != "$new_version" ]; then
        # Output new version to stdout
        echo "$new_version"
        exit 0
    fi

    exit 1
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

    local mc_versions_cache="$SERVICE_TEMP_DIR/version_cache.json"
    local release_json="$SERVICE_TEMP_DIR/_release.json"

    # Pick URL
    local release_url="$(cat "$mc_versions_cache" | jq -r "{versions: .versions} | .[] | .[] | select(.id == \"$new_version\") | {url: .url} | .[]")"
    echo "Release URL: $release_url"

    curl -sS "$release_url" >"$release_json"

    local release_server_jar_url="$(cat "$release_json" | jq -r '{url: .downloads.server.url} | .[]')"

    echo "Release .jar URL:  $release_server_jar_url"

    local local_release_jar="$SERVICE_TEMP_DIR/minecraft_server.$new_version.jar"

    if [ ! -f "$local_release_jar" ]; then
        curl -sS "$release_server_jar_url" -o "$local_release_jar"
    fi
    echo "Release .jar:  $local_release_jar"

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
