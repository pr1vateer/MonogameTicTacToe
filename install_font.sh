#!/bin/sh

cp Font/NotoSansMono-Regular.ttf ~/.fonts
fc-cache -f -v
fc-list
