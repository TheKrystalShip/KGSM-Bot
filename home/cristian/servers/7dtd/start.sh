#!/bin/bash

WORKING_DIR="/home/$USER/servers/7dtd"

exec "${WORKING_DIR}/install/latest/startserver.sh" -configfile="${WORKING_DIR}/install/latest/serverconfig.xml"
