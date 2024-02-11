#!/bin/bash

source /etc/environment

# Passed as args
service=""
online_string=""
offline_string=""

# Static
[[ -z "${STATUS_WATCHDOG_RABBITMQ_URI}" ]] && rabbitmq_uri='default' || rabbitmq_uri="${STATUS_WATCHDOG_RABBITMQ_URI}"
#rabbitmq_uri="$STATUS_WATCHDOG_RABBITMQ_URI"
rabbitmq_routing_key="service_status"

#Set the options of the getopt command
format=$(getopt -n "$0" -l "service:,online-string:,offline-string:" -- -- "$@")
if [ $# -lt 4 ]; then
    echo "Wrong number of arguments are passed. Set --service, --online-string and --offline-string"
    exit
fi
eval set -- "$format"

#Read the argument values
while [ $# -gt 0 ]; do
    case "$1" in
    --service)
        service="$2"
        shift
        ;;
    --online-string)
        online_string="$2"
        shift
        ;;
    --offline-string)
        offline_string="$2"
        shift
        ;;
    --) shift ;;
    esac
    shift
done

#service="projectzomboid"
#online_string="SERVER STARTED"
#offline_string="Stopped Project Zomboid Dedicated Server"

function init() {
    # shellcheck disable=SC2155
    local startup_status=$(exec systemctl is-active "$service")
    # shellcheck disable=SC2155
    local is_active=$(! [ "$startup_status" == "active" ]; echo $? )
    local previous_status="$is_active"
    local status_string=""

    echo "$service listener stated with PID $$"

    journalctl -fqn0 -o cat -u "$service" |
        while read -r line; do

            # Trigger if active
            echo "$line" | rg "$online_string"
            if [ $? = 0 ]; then
                is_active=1
                status_string="active"
            fi

            # Trigger if inactive
            echo "$line" | rg "$offline_string"
            if [ $? = 0 ]; then
                is_active=0
                status_string="inactive"
            fi

            # Prevent spamming RabbitMQ with messages if new_status hasn't changed
            if [[ $is_active = "$previous_status" ]]; then
                continue
            fi

            # Got this far, publish new_status
            publish_update "$service" "$status_string"
            previous_status="$is_active"
        done
}

function publish_update() {
    local service=$1
    local new_status=$2

    json_string=$(jq -n \
        --arg service "$service" \
        --arg status "$new_status" \
        '{service: $service, status: $status}')

    echo "$json_string"
    amqp-publish --uri="$rabbitmq_uri" --exchange="" --routing-key="$rabbitmq_routing_key" --body="$json_string"
    if [ $? -ne 0 ]; then
        echo "ERROR publishing"
    fi
}

init
