#!/bin/sh

cd ../sbt
make
cd ../glrotate

CXX=gcc
NAME=demo
OPTS="-O3 -g -Wall -std=c99"
INCS="-I/usr/include/freetype2 -I../sbt"
LIBS="-lfreeimage -lGL -lGLU -lglut -lfreetype -lm"
$CXX $OPTS $INCS -c -o ${NAME}.o ${NAME}.c
$CXX $OPTS $LIBS -o $NAME ${NAME}.o ../sbt/sbt.o
strip --strip-all $NAME
upx $NAME
