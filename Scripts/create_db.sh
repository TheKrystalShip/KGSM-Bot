#!/bin/bash

DB_NAME="info.db"

sqlite3 "$DB_NAME" <"schema.sql"

if [ "$1" = "--populate" ]; then
    sqlite3 "$DB_NAME" <"data.sql"
fi
