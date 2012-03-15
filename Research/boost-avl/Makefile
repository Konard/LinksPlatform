OPTS = -O3 -I..
LIBS = -lboost
SRCS = $(wildcard *.cpp)
OBJS = $(subst ,,$(SRCS:.cpp=.o))
BINS = $(subst ,,$(SRCS:.cpp=))

all:		$(BINS)

%:		%.cpp
		g++ -c $(OPTS) $<
		g++ -o $* $*.o $(LIBS)

clean:
		rm -f $(BINS)
		rm -f $(OBJS)
