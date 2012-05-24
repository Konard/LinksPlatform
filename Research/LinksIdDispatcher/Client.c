
// TCP/IP-client (Linux, Windows).

long long int requestsCount = 0;

#define _DEBUG 1

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

int ClientSocket = 0;

int Func(int *clientSocket)
{
	char buffer[8];
	while (TRUE)
	{
		write(*clientSocket, buffer, 8);
		read(*clientSocket, buffer, 8);
		requestsCount++;
		if (_DEBUG) if (requestsCount % 1000 == 0) printf("requestsCount = %lld\n", requestsCount);
	}
	return 0;
}

#elif defined(__MINGW32__) || defined(__MINGW64__)

#define _WIN32_WINNT 0x0501

#include <winsock2.h>
#include <ws2tcpip.h>

WSADATA WSAData;

SOCKET ListenSocket = INVALID_SOCKET;
SOCKET ClientSocket = INVALID_SOCKET;

int Func(SOCKET *clientSocket) {
	char buffer[8];
	int result;
	while (TRUE)
	{
		result = send(*clientSocket, buffer, 8, 0);
		if (result == SOCKET_ERROR)
		{
			printf("send failed: %d\n", WSAGetLastError());
			if (_DEBUG) perror("send()");
			closesocket(*clientSocket);
			WSACleanup();
			return -1;
		}
		result = recv(*clientSocket, buffer, 8, 0);
		if (result == SOCKET_ERROR)
		{
			printf("recv failed: %d\n", WSAGetLastError());
			if (_DEBUG) perror("recv()");
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


int ClientInitialize(const char *hostname, const char *port)
{
#ifdef __linux__

	ClientSocket = socket(AF_INET, SOCK_STREAM, 0);
	if (ClientSocket < 0)
	{
		if (_DEBUG) perror("socket()");
		return -EXIT_FAILURE;
	}
	else
	{
		if (_DEBUG) printf("socket(): Success\n");
	}
	const int isOn = 1;
	if (setsockopt(ClientSocket, SOL_SOCKET, SO_REUSEADDR, &isOn, sizeof(isOn)) < 0)
	{
		if (_DEBUG) perror("setsockopt()");
		return -EXIT_FAILURE; // -1
	}
	int yes = 1;
	if (setsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *)&yes, sizeof(yes)) < 0)
	{
		if (_DEBUG) perror("setsockopt()");
		return -EXIT_FAILURE; // -1
	}

	struct sockaddr_in hostAddress;
	hostAddress.sin_family = AF_INET;
	inet_aton(hostname, &hostAddress.sin_addr);
	hostAddress.sin_port = htons(atoi(port));
	if (_DEBUG) printf("hostname:port = %s\n", hostname, port);

	int res = connect(ClientSocket, (struct sockaddr *)&hostAddress, sizeof(hostAddress));

	if (res < 0)
	{
		if (_DEBUG) perror("connect()");
		return -EXIT_FAILURE;
	}
	else
	{
		if (_DEBUG) printf("connect(): Success\n");
	}

#elif defined(__MINGW32__) || defined(__MINGW64__)

	int winsockResult;
	// Initialize Winsock2 system.
	winsockResult = WSAStartup(MAKEWORD(2,2), &WSAData);
	if (winsockResult != 0)
	{
		if (_DEBUG) printf("WSAStartup failed: %d\n", winsockResult);
		return 1;
	}

	struct addrinfo *result = NULL, hints;
	
	// We can provide hints about the type of socket supported through an addrinfo structure
	// pointed to by the pHints parameter:
	// * AF_UNSPEC indicates the caller will accept any protocol family
	// * SOCK_STREAM indicates the caller will accept this socket type
	// * IPPROTO_TCP indicates the caller will accept TCP protocol
	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	// Resolve the server address and port.
	winsockResult = getaddrinfo(hostname, port, &hints, &result);
	if (winsockResult != 0) {
		if (_DEBUG) printf("getaddrinfo failed: %d\n", winsockResult);
		WSACleanup();
		return 1;
	}

	// Create a SOCKET for connecting to server.
	ClientSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
	if (ClientSocket == INVALID_SOCKET) {
		if (_DEBUG) printf("Error at socket(): %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	int optionYes = 1;
	int optionYesLen = sizeof(optionYes);
	winsockResult = getsockopt(ClientSocket, IPPROTO_TCP, TCP_NODELAY, (char *) &optionYes, &optionYesLen);
	if (winsockResult == SOCKET_ERROR) {
		if (_DEBUG) printf("getsockopt for SO_KEEPALIVE failed with error: %u\n", WSAGetLastError());
	}
	else {
		if (_DEBUG) printf("TCP_NODELAY Value: %d\n", optionYes);
	}

	// Connect to server.
	winsockResult = connect(ClientSocket, result->ai_addr, (int)result->ai_addrlen);
	if (winsockResult == SOCKET_ERROR) {
		closesocket(ClientSocket);
		ClientSocket = INVALID_SOCKET;
	}

	// Should really try the next address returned by getaddrinfo
	// if the connect call failed.
	// But for this simple example we just free the resources
	// returned by getaddrinfo and print an error message.
	freeaddrinfo(result);
	if (ClientSocket == INVALID_SOCKET) {
		if (_DEBUG) printf("Unable to connect to server!\n");
		WSACleanup();
		return 1;
	}

#endif
	return 0;
}

// Signal event handler for SIGINT.
void FinalizeCallback(int signal)
{
	exit(EXIT_SUCCESS); // this calls pserver_fini()
}

void ClientFinalize()
{
#ifdef __linux__
	shutdown(ClientSocket, 2);
	close(ClientSocket);
#elif defined(__MINGW32__) || defined(__MINGW64__)
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
	atexit((void(*)())ClientFinalize);
	signal(SIGINT, FinalizeCallback);
#elif defined(__MINGW32__) || defined(__MINGW64__)
#endif

	ClientInitialize(hostname, port);

	Func(&ClientSocket);

	ClientFinalize();
	return EXIT_SUCCESS;
}
