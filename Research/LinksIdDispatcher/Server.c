
// TCP/IP-server (Linux, Windows).

long long int i = 0;

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

void Func(int *clientSocket)
{
	char buffer[8];
	while(TRUE)
	{
		read(*clientSocket, buffer, 8);
		write(*clientSocket, buffer, 8);
		i++;
		if (i % 1000 == 0) printf("i = %lld\n", i);
	}
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
	while(TRUE)
	{
		result = read(*clientSocket, buffer, 8);
		if (result == SOCKET_ERROR)
		{
			printf("read failed: %d\n", WSAGetLastError());
			closesocket(*clientSocket);
			WSACleanup();
			return -1;
		}
		result = write(*clientSocket, buffer, 8);
		if (result == SOCKET_ERROR)
		{
			printf("write failed: %d\n", WSAGetLastError());
			closesocket(*clientSocket);
			WSACleanup();
			return -1;
		}
		i++;
		if (i % 1000 == 0) printf("i = %lld\n", i);
	}
	return 0;
}

#endif


#define _DEBUG 1


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

	struct sockaddr_in serverAddressStruct;
	serverAddressStruct.sin_family = AF_INET;
	serverAddressStruct.sin_addr.s_addr = INADDR_ANY;
	serverAddressStruct.sin_port = htons(atoi(port));
	int res = bind(ListenSocket, (struct sockaddr *)&serverAddressStruct, sizeof(struct sockaddr_in));
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
		printf("WSAStartup failed: %d\n", winsockResult);
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
	winsockResult = getaddrinfo(NULL, port, &hints, &result);
	if (winsockResult != 0)
	{
		printf("getaddrinfo failed: %d\n", winsockResult);
		WSACleanup();
		return 1;
	}

	// Create a SOCKET for the server to listen for client connections
	ListenSocket = socket(result->ai_family, result->ai_socktype,
		result->ai_protocol);

	if (ListenSocket == INVALID_SOCKET)
	{
		printf("Error at socket(): %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	// Setup the TCP listening socket
	winsockResult = bind( ListenSocket, result->ai_addr, (int)result->ai_addrlen);
	if (winsockResult == SOCKET_ERROR)
	{
		printf("bind failed with error: %d\n", WSAGetLastError());
		freeaddrinfo(result);
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}

	if ( listen( ListenSocket, SOMAXCONN ) == SOCKET_ERROR )
	{
		printf( "Listen failed with error: %ld\n", WSAGetLastError() );
		closesocket(ListenSocket);
		WSACleanup();
		return 1;
	}

	freeaddrinfo(result);
#endif
	printf("initialized.");
	return 0;
}

// signal event handler for SIGINT
void FinalizeCallback(int signal)
{
	exit(EXIT_SUCCESS); // this calls pserver_fini()
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
	signal(SIGINT, FinalizeCallback); // this calls pserver_terminate()
#elif defined(__MINGW32__) || defined(__MINGW64__)
#endif

	ServerInitialize(hostname, port);

	while (TRUE)
	{
		ClientSocket = accept(ListenSocket, NULL, NULL); // blocking accept()
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
			printf("accept failed: %d\n", WSAGetLastError());
			closesocket(ListenSocket);
			WSACleanup();
			return 1;
		}
		int optionYes = 1;
		int optionYesLen = sizeof(optionYes);
		int winsockResult = getsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *) &optionYes, &optionYesLen);
		if (winsockResult == SOCKET_ERROR) {
			printf("getsockopt for SO_KEEPALIVE failed with error: %u\n", WSAGetLastError());
		}
		else {
			printf("SO_KEEPALIVE Value: %d\n", optionYes);
		}

#endif
		printf("[accepted]\n");
		Func(&ClientSocket);
	}

	ServerFinalize();
	return EXIT_SUCCESS;
}
