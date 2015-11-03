CC=gcc
OPTS=-O3 --std=c99 -fPIC -W -Wall -Wextra -pedantic
DEFS=-DLINKS_DLL -D_XOPEN_SOURCE=700
Platform.Data.Kernel.a	: Link.h Link.c
	${CC} ${OPTS} ${DEFS} -c -o Common.o Common.c
	${CC} ${OPTS} ${DEFS} -c -o Timestamp.o Timestamp.c
	${CC} ${OPTS} ${DEFS} -c -o Link.o Link.c
	${CC} ${OPTS} ${DEFS} -c -o PersistentMemoryManager.o PersistentMemoryManager.c
	${CC} -shared -o Platform.Data.Kernel.dll Link.o PersistentMemoryManager.o Common.o Timestamp.o
	${CC} ${OPTS} -o test test.c -L. -l:Platform.Data.Kernel.dll
