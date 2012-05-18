#!/bin/bash
i586-mingw32msvc-gcc -o pserver-linux-mingw32-i586.exe pserver.c -lws2_32
i586-mingw32msvc-gcc -o pclient-linux-mingw32-i586.exe pclient.c -lws2_32
