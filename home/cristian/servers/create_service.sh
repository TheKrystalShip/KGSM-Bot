#!/bin/bash

if [ $# -eq 0 ]; then
    echo "Service name not supplied"
    exit 1
fi

SERVICE=$1
SERVICE_FILES_DIR="/etc/systemd/system"

init() {

    echo "Creating service file..."

    # Create service file first
    local service_file="$SERVICE_FILES_DIR/$SERVICE.service"
    local is_service_file_created=1
    is_service_file_created=$(create_service_file "$service_file")
    if [ "$is_service_file_created" -ne 0 ]; then
        echo "Failed to create service file, exiting"
        exit 1
    fi

    echo "File $service_file created."

    echo "Reloading systemd daemon..."

    # Reload daemon if service file was created successfully
    local is_daemon_reloaded=1
    is_daemon_reloaded=$(reload_systemd_daemon)
    if [ "$is_daemon_reloaded" -ne 0 ]; then
        echo "Failed to reload systemd daemon, exiting"
        exit 1
    fi

    echo "Systemd daemon reloaded."

    echo "Service created successfully, exiting."
    exit 0
}

crate_service_file() {
    local file=$1
    exit_code=$(exec "cat <<EOF >$file
[Unit]
Description=${SERVICE^} Dedicated Server
StartLimitIntervalSec=10

[Service]
User=${USER}
WorkingDirectory=/home/${USER}/servers/${SERVICE}/install
ExecStart=/home/${USER}/servers/${SERVICE}/start.sh

# optional items below
Restart=on-failure
RestartSec=5
StartLimitBurst=1

[Install]
WantedBy=multi-user.target
EOF")

    if [ "$exit_code" -ne 0 ]; then
        return 1
    fi

    return 0
}

reload_systemd_daemon() {
    if systemctl deemon-reload; then
        return 0
    fi

    return 1
}

init
