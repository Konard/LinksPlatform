
NAME = test
OPTS = -O3 -std=c99 -I..
SRCS = $(wildcard *.c)
OBJS = $(subst ,,$(SRCS:.c=.o))
BINS = $(subst ,,$(SRCS:.c=))

tests:		$(BINS)

$(BINS):	$(OBJS)
		gcc -o $@ $< ../sbt.o

$(OBJS):	$(SRCS)
		gcc ${OPTS} -c -o $@ $<
clean:
	rm -f $(OBJS)
