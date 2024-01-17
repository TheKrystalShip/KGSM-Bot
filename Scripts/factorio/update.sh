#!/bin/sh

# Downaload and unpack latest version of the stable headless server
wget https://factorio.com/get-download/stable/headless/linux64 -O factorio_headless.tar.xz
tar -xf factorio_headless.tar.xz --strip-components=1 -C /home/$USER/servers/factorio/install
rm factorio_headless.tar.xz
