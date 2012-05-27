
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

#define BACKLOG 100
#define TRUE 1

int ListenSocket = 0;
int ClientSocket = 0;

int Func(int *clientSocket)
{
	char buffer[8];
	int result;

#if defined(SERVER_SELECT)
	fd_set rfds;
	struct timeval tv;
	FD_ZERO(&rfds);
	FD_SET(*clientSocket, &rfds);
	tv.tv_sec = 0; // wait for five seconds
	tv.tv_usec = 5000;

	while (TRUE)
	{
		result = select(1, &rfds, NULL, NULL, &tv);
		if (result) {
			recv(*clientSocket, buffer, BUFSIZE, 0);
			send(*clientSocket, buffer, BUFSIZE, 0);
			requestsCount++;
			if (_DEBUG) if (requestsCount % 1000 == 0) printf("requestsCount = %lld\n", requestsCount);
		}
		else if (result == -1) perror("select()");
	}
#else
	while (TRUE)
	{
		recv(*clientSocket, buffer, BUFSIZE, 0);
		send(*clientSocket, buffer, BUFSIZE, 0);
		requestsCount++;
		if (_DEBUG) if (requestsCount % 1000 == 0) printf("requestsCount = %lld\n", requestsCount);
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
	shutdown(ListenSocket, 2);
	shutdown(ClientSocket, 2);
	close(ListenSocket);
	close(ClientSocket);
#elif defined(__MINGW32__) || defined(__MINGW64__)
	closesocket(ListenSocket);
	closesocket(ClientSocket);
	WSACleanup();
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

	while (TRUE)
	{
		ClientSocket = accept(ListenSocket, NULL, NULL); // Blocking accept().
#ifdef __linux__
		int yes = 1;
		if (setsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *)&yes, sizeof(yes)) < 0)
		{
			if (_DEBUG) perror("setsockopt()");
			return -EXIT_FAILURE; // -1
		}
#elif defined(__MINGW32__) || defined(__MINGW64__)
		if (ClientSocket == INVALID_SOCKET)
		{
			if (_DEBUG) printf("accept failed: %d\n", WSAGetLastError());
			closesocket(ListenSocket);
			WSACleanup();
			return 1;
		}
		int optionYes = 1;
		int optionYesLen = sizeof(optionYes);
		int winsockResult = getsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *) &optionYes, &optionYesLen);
		if (winsockResult == SOCKET_ERROR) {
			if (_DEBUG) printf("getsockopt for SO_KEEPALIVE failed with error: %u\n", WSAGetLastError());
		}
		else {
			if (_DEBUG) printf("TCP_NODELAY Value: %d\n", optionYes);
		}

#endif
		if (_DEBUG) printf("[accepted]\n");
		Func(&ClientSocket);
	}

	ServerFinalize();
	return EXIT_SUCCESS;
}
