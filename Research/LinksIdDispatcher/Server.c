
// TCP/IP-server (Linux, Windows).

long long int requestsCount = 0;

#define _DEBUG 1
#define BUFSIZE 32

#ifdef __linux__

#include <stdlib.h> // atoi(), exit()

#include <sys/types.h> // socket(), setsockopt(), ...
#include <sys/socket.h>

#include <arpa/inet.h> // htons()
#include <netinet/tcp.h> // TCP_NODELAY

#include <stdio.h> // perror(), printf()

#include <signal.h> // SIGINT

#include <pthread.h> // pthread_*()

#define BACKLOG 100
#define TRUE 1

int ListenSocket = 0;
#if defined(SERVER_SELECT)
#define MAX_SOCKETS 1000
int ClientSockets[MAX_SOCKETS];
int FreeSocketIndex = 0;
fd_set rfds;
int nfds = 0;
#else
int ClientSocket = 0;
#endif

int Func(void *p)
{
	char buffer[8];
	int result;
	int socketIndex;

	struct timeval tv;

#if defined(SERVER_SELECT)

	while (TRUE)
	{
		FD_ZERO(&rfds);
		for (socketIndex = 0; socketIndex < FreeSocketIndex; socketIndex++)
		{
			FD_SET(ClientSockets[socketIndex], &rfds);
		}
		tv.tv_sec = 0; // wait for five seconds
		tv.tv_usec = 500000;
		result = select(nfds + 1, &rfds, NULL, NULL, &tv);
		if (result == -1) perror("select()");
		if (result)
		{
			for (socketIndex = 0; socketIndex < FreeSocketIndex; socketIndex++)
			{
				if (FD_ISSET(ClientSockets[socketIndex], &rfds))
				{
					FD_CLR(ClientSockets[socketIndex], &rfds);
					recv(ClientSockets[socketIndex], buffer, BUFSIZE, 0);
					send(ClientSockets[socketIndex], buffer, BUFSIZE, 0);
					requestsCount++;
					if (_DEBUG) if (requestsCount % 100000 == 0) printf("requestsCount = %lld (%d) idx = %d\n", requestsCount, result, FreeSocketIndex);
				}
			}
		}
		else {
			printf("ignore ...\n");
		}
	}
#else
	while (TRUE)
	{
		recv(*clientSocket, buffer, BUFSIZE, 0);
		send(*clientSocket, buffer, BUFSIZE, 0);
		requestsCount++;
		if (_DEBUG) if (requestsCount % 100000 == 0) printf("requestsCount = %lld\n", requestsCount);
	}
#endif
}

#elif defined(__MINGW32__) || defined(__MINGW64__)

#define _WIN32_WINNT 0x0501

#include <winsock2.h>
#include <ws2tcpip.h>

WSADATA wsaData;

SOCKET ListenSocket = INVALID_SOCKET;
SOCKET ClientSocket = INVALID_SOCKET;

int Func(SOCKET *clientSocket)
{
	char buffer[8];
	int result;

	fd_set rfds;
	struct timeval tv;
	FD_ZERO(&rfds);
	FD_SET(0, &rfds);
	tv.tv_sec = 5; // wait for five seconds
	tv.tv_usec = 0;

	while (TRUE)
	{
		result = select(1, &rfds, NULL, NULL, &tv);
		result = recv(*clientSocket, buffer, BUFSIZE, 0);
		if (result == SOCKET_ERROR)
		{
			if (_DEBUG) printf("recv failed: %d\n", WSAGetLastError());
			closesocket(*clientSocket);
			WSACleanup();
			return -1;
		}
		result = send(*clientSocket, buffer, BUFSIZE, 0);
		if (result == SOCKET_ERROR)
		{
			if (_DEBUG) printf("send failed: %d\n", WSAGetLastError());
			closesocket(*clientSocket);
			WSACleanup();
			return -1;
		}
		requestsCount++;
		if (_DEBUG) if (requestsCount % 1000 == 0) printf("requestsCount = %lld\n", requestsCount);
	}
	return 0;
}

#endif


int ServerInitialize(char *hostname, char *port)
{
	// Only ListenSocket, not ClientSocket[s].
#ifdef __linux__

	ListenSocket = socket(AF_INET, SOCK_STREAM, 0);
	if (ListenSocket < 0)
	{
		if (_DEBUG) perror("socket()");
		exit(EXIT_FAILURE);
	}
	else
	{
		if (_DEBUG) printf("socket(): Success\n");
	}
	const int isOn = 1;
	if (setsockopt(ListenSocket, SOL_SOCKET, SO_REUSEADDR, &isOn, sizeof(isOn)) < 0)
	{
		if (_DEBUG) perror("socket()");
		exit(EXIT_FAILURE);
	}

	struct sockaddr_in serverAddress;
	serverAddress.sin_family = AF_INET;
	serverAddress.sin_addr.s_addr = INADDR_ANY;
	serverAddress.sin_port = htons(atoi(port));
	int res = bind(ListenSocket, (struct sockaddr *)&serverAddress, sizeof(struct sockaddr_in));
	if (res < 0)
	{
		if (_DEBUG) perror("bind()");
		exit(EXIT_FAILURE);
	}
	else
	{
		if (_DEBUG) printf("bind(): Success\n");
	}

	res = listen(ListenSocket, BACKLOG);
	if (res < 0)
	{
		if (_DEBUG) perror("listen()");
		exit(EXIT_FAILURE);
	}
	else
	{
		if (_DEBUG) printf("listen(): Success\n");
	}

#elif defined(__MINGW32__) || defined(__MINGW64__)

	int winsockResult;
	// Initialize Winsock
	winsockResult = WSAStartup(MAKEWORD(2,2), &wsaData);
	if (winsockResult != 0)
	{
		if (_DEBUG) printf("WSAStartup failed: %d\n", winsockResult);
		return 1;
	}

	struct addrinfo *result = NULL,
		*ptr = NULL,
		hints;

	// * AI_PASSIVE indicates the caller intends to use the returned socket address
	//   structure in a call to the bind() function.
	ZeroMemory( &hints, sizeof(hints) );
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	// Resolve the local address and port to be used by the server.
	winsockResult = getaddrinfo(NULL, port, &hints, &result);
	if (winsockResult != 0)
	{
		if (_DEBUG) printf("getaddrinfo failed: %d\n", winsockResult);
		WSACleanup();
		return 1;
	}

	// Create a SOCKET for the server to listen for client connections.
	ListenSocket = socket(result->ai_family, result->ai_socktype,
		result->ai_protocol);

	if (ListenSocket == INVALID_SOCKET)
	{
		if (_DEBUG) printf("Error at socket(): %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	// Setup the TCP listening socket.
	winsockResult = bind(ListenSocket, result->ai_addr, (int)result->ai_addrlen);
	if (winsockResult == SOCKET_ERROR)
	{
		if (_DEBUG) printf("bind failed with error: %d\n", WSAGetLastError());
		freeaddrinfo(result);
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}

	if (listen(ListenSocket, SOMAXCONN) == SOCKET_ERROR)
	{
		if (_DEBUG) printf("Listen failed with error: %ld\n", WSAGetLastError());
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}

	freeaddrinfo(result);
#endif
	if (_DEBUG) printf("initialized.\n");
	return 0;
}

// Signal event handler for SIGINT.
void FinalizeCallback(int signal)
{
	exit(EXIT_SUCCESS); // This calls ServerFinalize().
}

void ServerFinalize()
{
#ifdef __linux__
#if defined(SERVER_SELECT)
	int socketIndex;
	for (socketIndex = 0; socketIndex < FreeSocketIndex; socketIndex++) {
		shutdown(ClientSockets[socketIndex], 2);
		close(ClientSockets[socketIndex]);
	}
	shutdown(ListenSocket, 2);
	close(ListenSocket);
#else
	shutdown(ListenSocket, 2);
	shutdown(ClientSocket, 2);
	close(ListenSocket);
	close(ClientSocket);
#endif
#elif defined(__MINGW32__) || defined(__MINGW64__)
#if defined(SERVER_SELECT)
	int socketIndex;
	for (socketIndex = 0; socketIndex < FreeSocketIndex; socketIndex++) {
		closesocket(ClientSockets[socketIndex]);
	}
	closesocket(ListenSocket);
	WSACleanup();
#else
	closesocket(ListenSocket);
	closesocket(ClientSocket);
	WSACleanup();
#endif
#endif
}

int main(int argumentsCount, char **arguments)
{

	if (argumentsCount < 3) return EXIT_SUCCESS;

	char *hostname = arguments[1];
	char *port = arguments[2];

#ifdef __linux__
	atexit((void(*)())ServerFinalize);
	signal(SIGINT, FinalizeCallback);
#elif defined(__MINGW32__) || defined(__MINGW64__)
#endif

	ServerInitialize(hostname, port);

#if defined(SERVER_SELECT)
	pthread_t threadFunc;
	int err = pthread_create(&threadFunc, NULL, (void * (*)(void *))(&Func), NULL);
	if (err != 0) { perror("pthread_create()"); }
#endif

	while (TRUE)
	{
#if defined(SERVER_SELECT)
		ClientSockets[FreeSocketIndex] = accept(ListenSocket, NULL, NULL); // Blocking accept().
#else
		ClientSocket = accept(ListenSocket, NULL, NULL); // Blocking accept().
#endif

#ifdef __linux__
		int yes = 1;

#if defined(SERVER_SELECT)
		if (setsockopt(ClientSockets[FreeSocketIndex], IPPROTO_TCP, TCP_NODELAY, (char *)&yes, sizeof(yes)) < 0)
#else
		if (setsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *)&yes, sizeof(yes)) < 0)
#endif
		{
			if (_DEBUG) perror("setsockopt()");
			return -EXIT_FAILURE; // -1
		}
#elif defined(__MINGW32__) || defined(__MINGW64__)

#if defined(SERVER_SELECT)
		if (ClientSockets[FreeSocketIndex] == INVALID_SOCKET)
#else
		if (ClientSocket == INVALID_SOCKET)
#endif

		{
			if (_DEBUG) printf("accept failed: %d\n", WSAGetLastError());
			closesocket(ListenSocket);
			WSACleanup();
			return 1;
		}
		int optionYes = 1;
		int optionYesLen = sizeof(optionYes);

#if defined(SERVER_SELECT)
		int winsockResult = getsockopt(ClientSockets[FreeSocketIndex], IPPROTO_TCP, TCP_NODELAY, (char *) &optionYes, &optionYesLen);
#else
		int winsockResult = getsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *) &optionYes, &optionYesLen);
#endif
		if (winsockResult == SOCKET_ERROR) {
			if (_DEBUG) printf("getsockopt for SO_KEEPALIVE failed with error: %u\n", WSAGetLastError());
		}
		else {
			if (_DEBUG) printf("TCP_NODELAY Value: %d\n", optionYes);
		}
#endif

		if (_DEBUG) printf("[accepted]\n");

#ifdef __linux__

#if defined(SERVER_SELECT)
		printf("%d\n", ClientSockets[FreeSocketIndex]);
		if (nfds < ClientSockets[FreeSocketIndex]) nfds = ClientSockets[FreeSocketIndex];
		FreeSocketIndex++;
#else
		Func(&ClientSocket);
#endif

#elif defined(__MINGW32__) || defined(__MINGW64__)

		HANDLE _thread;
		DWORD _thread_id;
		_thread = CreateThread(
			NULL,
			0,
			(LPTHREAD_START_ROUTINE)Func,
			&i,
			0,
			&_thread_id
		);

#endif
	}

	ServerFinalize();
	return EXIT_SUCCESS;
}
