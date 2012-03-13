NAME=test
OPTS=-O3 -std=c99

${NAME}	:	${NAME}.c sbt.h sbt.c tests
		gcc ${OPTS} -c -o sbt.o sbt.c
		gcc ${OPTS} -c -o ${NAME}.o ${NAME}.c
		gcc -o ${NAME} ${NAME}.o sbt.o

clean:		
		rm -f ${NAME}
		rm -f ${NAME}.o
		rm -f sbt.o
