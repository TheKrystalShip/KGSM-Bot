#!/bin/bash

PROCESS=$1
LINES=10

journalctl -n "$LINES" -u "$PROCESS"
