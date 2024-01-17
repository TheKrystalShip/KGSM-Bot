#!/bin/bash

# Move into directory because server uses pwd as a config
(cd ./install/linux && exec ./starbound_server)
