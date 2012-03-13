NAME = test
OPTS = -O3 -std=c99 -I..
SRCS = $(wildcard *.c)
OBJS = $(subst ,,$(SRCS:.c=.o))

all:		$(OBJS)

%.o:		%.c
		gcc -c $(OPTS) $<
		gcc -o $* $*.o ../sbt.o

clean:
		rm -f $(BINS)
		rm -f $(OBJS)
