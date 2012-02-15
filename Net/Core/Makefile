CC=gcc
OPTS=-O3 --std=c99
liblinkscore.a	: Link.h Link.c
	${CC} ${OPTS} -c -o Timestamp.o Timestamp.c
	${CC} ${OPTS} -c -o Link.o Link.c
	${CC} -shared -o liblinkscore.a Link.o Timestamp.o
