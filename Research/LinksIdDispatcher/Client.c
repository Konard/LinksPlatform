
// TCP/IP-client (Linux, Windows).

long long int i = 0;

#ifdef __linux__

#include <stdlib.h> // atoi(), exit()

#include <sys/types.h> // socket(), setsockopt(), ...
#include <sys/socket.h>

#include <arpa/inet.h> // htons()

#include <stdio.h> // perror(), printf()

#include <signal.h> // SIGINT

#define BACKLOG 100
#define TRUE 1

int ClientSocket = 0;

void Func(int *clientSocket)
{
	char buffer[8];
	while(TRUE)
	{
		write(*clientSocket, buffer, 8);
		read(*clientSocket, buffer, 8);
		i++;
		if (i%1000 == 0) printf("i = %lld\n", i);
	}
}

#elif defined(__MINGW32__) || defined(__MINGW64__)

#define _WIN32_WINNT 0x0501

#include <winsock2.h>
#include <ws2tcpip.h>

WSADATA WSAData;

SOCKET ListenSocket = INVALID_SOCKET;
SOCKET ClientSocket = INVALID_SOCKET;

void Func(SOCKET *pClientSocket) {
}

#endif


#define _DEBUG 1


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
		if (_DEBUG) perror("socket()");
		return -EXIT_FAILURE; // -1
	}

	struct sockaddr_in hostAddressStruct;
	hostAddressStruct.sin_family = AF_INET;
	inet_aton(hostname, &hostAddressStruct.sin_addr);
	hostAddressStruct.sin_port = htons(atoi(port));
	if (_DEBUG) printf("hostname:port = %s\n", hostname, port);

	int res = connect(ClientSocket, (struct sockaddr *)&hostAddressStruct, sizeof(hostAddressStruct));

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

	int iResult;
	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2,2), &WSAData);
	if (iResult != 0)
	{
		printf("WSAStartup failed: %d\n", iResult);
		return 1;
	}

#endif
	return 0;
}

// signal event handler for SIGINT
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
	signal(SIGINT, FinalizeCallback); // this calls pserver_terminate()
#elif defined(__MINGW32__) || defined(__MINGW64__)
#endif

	ClientInitialize(hostname, port);

	Func(&ClientSocket);

	ClientFinalize();
	return EXIT_SUCCESS;
}
