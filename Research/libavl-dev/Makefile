OPTS = -O3 -std=c99 -I..
LIBS = -lavl
SRCS = $(wildcard *.c)
OBJS = $(subst ,,$(SRCS:.c=.o))
BINS = $(subst ,,$(SRCS:.c=))

all:		$(BINS)

%:		%.c
		gcc -c $(OPTS) $<
		gcc -o $* $*.o $(LIBS)

clean:
		rm -f $(BINS)
		rm -f $(OBJS)
