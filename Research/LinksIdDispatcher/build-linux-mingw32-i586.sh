#!/bin/bash
i586-mingw32msvc-gcc -o Server-linux-mingw32-i586.exe Server.c -lws2_32
i586-mingw32msvc-gcc -o Client-linux-mingw32-i586.exe Client.c -lws2_32
