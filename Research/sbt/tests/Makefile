NAME = test
OPTS = -O3 -std=c99 -I.. -Wall
SRCS = $(wildcard *.c)
OBJS = $(subst ,,$(SRCS:.c=.o))
BINS = $(subst ,,$(SRCS:.c=))

all:		$(OBJS)

%.o:		%.c ../sbt.o Makefile
		gcc -c $(OPTS) $<
		gcc -o $* $*.o ../sbt.o

clean:
		rm -f $(BINS)
		rm -f $(OBJS)
