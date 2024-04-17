#!/bin/sh

cp Font/NotoSansMono-Regular.ttf /usr/local/share/fonts
fc-cache -f -v
fc-list
