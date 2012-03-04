#!/bin/sh
CXX=g++
NAME=demo
OPTS="-O3 -g -Wall"
INCS="-I/usr/include/freetype2"
LIBS="-lfreeimage -lGL -lGLU -lglut -lfreetype"
$CXX $OPTS $INCS -c -o ${NAME}.o ${NAME}.c
$CXX $OPTS $LIBS -o $NAME ${NAME}.o
strip --strip-all $NAME
upx $NAME
