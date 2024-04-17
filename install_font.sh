#!/bin/sh

cp Font/NotoSansMono-Regular.ttf ~/.local/share/fonts
fc-cache -f -v
fc-list
