#!/bin/bash
gcc -o Server-linux-gcc Server.c -lpthread -DSERVER_SELECT
gcc -o Client-linux-gcc Client.c -lpthread
