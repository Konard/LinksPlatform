NAME=test
OPTS=-std=c99
${NAME}	:	${NAME}.c sbt.h sbt.c
		gcc ${OPTS} -c -o sbt.o sbt.c
		gcc ${OPTS} -c -o ${NAME}.o ${NAME}.c
		gcc -o ${NAME} ${NAME}.o sbt.o
