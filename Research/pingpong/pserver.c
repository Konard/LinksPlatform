
#ifdef __linux__

#include <stdlib.h> // atoi(), exit()

#include <sys/types.h> // socket(), setsockopt(), ...
#include <sys/socket.h>

#include <arpa/inet.h> // htons()

#include <stdio.h> // perror(), printf()

#include <signal.h> // SIGINT

#define BACKLOG 100

int sock = 0;
int sock_c = 0;

void func(int *sock_c) {
}

#elif defined(__MINGW32__) || defined(__MINGW64__)

#include <winsock2.h>
#include <ws2tcpip.h>

WSADATA wsaData;

SOCKET ListenSocket = INVALID_SOCKET;
SOCKET ClientSocket = INVALID_SOCKET;

void func(SOCKET *pClientSocket) {
}

#endif


#define _DEBUG 1

int i = 123;

int pserver_init(char *hostname, char *s_port) {
#ifdef __linux__
  int port = atoi(s_port);
  sock = socket(AF_INET, SOCK_STREAM, 0);
  if (sock < 0) {
    if (_DEBUG) perror("socket()");
    exit(EXIT_FAILURE);
  }
  else {
    if (_DEBUG) printf("socket(): Success\n");
  }
  const int on = 1;
  if (setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, &on, sizeof(on)) < 0) {
    if (_DEBUG) perror("socket()");
    exit(EXIT_FAILURE);
  }

  struct sockaddr_in serv_addr;
  serv_addr.sin_family = AF_INET;
  serv_addr.sin_addr.s_addr = INADDR_ANY;
  serv_addr.sin_port = htons(port);
  int res = bind(sock, (struct sockaddr *)&serv_addr, sizeof(struct sockaddr_in));
  if (res < 0) {
    if (_DEBUG) perror("bind()");
    exit(EXIT_FAILURE);
  }
  else {
    if (_DEBUG) printf("bind(): Success\n");
  }

  res = listen(sock, BACKLOG);
  if (res < 0) {
    if (_DEBUG) perror("listen()");
    exit(EXIT_FAILURE);
  }
  else {
    if (_DEBUG) printf("listen(): Success\n");
  }
#elif defined(__MINGW32__) || defined(__MINGW64__)

  int iResult;
  // Initialize Winsock
  iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
  if (iResult != 0) {
    printf("WSAStartup failed: %d\n", iResult);
    return 1;
  }

  struct addrinfo *result = NULL,
                  *ptr = NULL,
                  hints;
  ZeroMemory( &hints, sizeof(hints) );
  hints.ai_family = AF_INET;
  hints.ai_socktype = SOCK_STREAM;
  hints.ai_protocol = IPPROTO_TCP;
  hints.ai_flags = AI_PASSIVE;

  // Resolve the local address and port to be used by the server
  iResult = getaddrinfo(NULL, s_port, &hints, &result);
  if (iResult != 0) {
    printf("getaddrinfo failed: %d\n", iResult);
    WSACleanup();
    return 1;
  }

  // Create a SOCKET for the server to listen for client connections
  ListenSocket = socket(result->ai_family, result->ai_socktype, 
    result->ai_protocol);

  if (ListenSocket == INVALID_SOCKET) {
    printf("Error at socket(): %ld\n", WSAGetLastError());
    freeaddrinfo(result);
    WSACleanup();
    return 1;
  }

  // Setup the TCP listening socket
  iResult = bind( ListenSocket, result->ai_addr, (int)result->ai_addrlen);
  if (iResult == SOCKET_ERROR) {
    printf("bind failed with error: %d\n", WSAGetLastError());
    freeaddrinfo(result);
    closesocket(ListenSocket);
    WSACleanup();
    return 1;
  }

  if ( listen( ListenSocket, SOMAXCONN ) == SOCKET_ERROR ) {
    printf( "Listen failed with error: %ld\n", WSAGetLastError() );
    closesocket(ListenSocket);
    WSACleanup();
    return 1;
  }

  freeaddrinfo(result);
#endif
  return 0;
}

// signal event handler for SIGINT
void finish(int sig) {
  exit(EXIT_SUCCESS); // this calls pserver_fini()
}

void pserver_fini() {
#ifdef __linux__
  shutdown(sock, 2); // send
  close(sock);
#elif defined(__MINGW32__) || defined(__MINGW64__)
  closesocket(ListenSocket);
  WSACleanup();
#endif
}

int main(int argc, char **argv) {
  if (argc < 3) return EXIT_SUCCESS;

  char *hostname = argv[1];
  char *port = argv[2];
  pserver_init(hostname, port);
#ifdef __linux__
  atexit((void(*)())pserver_fini);
  signal(SIGINT, finish); // this calls pserver_terminate()
#elif defined(__MINGW32__) || defined(__MINGW64__)
#endif

#ifdef __linux__
  while (1) {
    // blocking accept()
    sock_c = accept(sock, NULL, NULL);
    printf("[accepted]\n");
    func(&sock_c);
  }
#elif defined(__MINGW32__) || defined(__MINGW64__)
  while (TRUE) {
    // blocking accept()
    ClientSocket = INVALID_SOCKET;
    // Accept a client socket
    ClientSocket = accept(ListenSocket, NULL, NULL);
    if (ClientSocket == INVALID_SOCKET) {
      printf("accept failed: %d\n", WSAGetLastError());
      closesocket(ListenSocket);
      WSACleanup();
      return 1;
    }
    printf("[accepted]\n");
    func(&ClientSocket);
  }
#endif

  pserver_fini();
  return EXIT_SUCCESS;
}
