#include <Windows.h>

#include "Common.h"
#include "Time.h"

long long LastTimestamp = 0;

long long GetTimestamp()
{
	FILETIME fileTime;
	GetSystemTimeAsFileTime(&fileTime);

	{
		long long time = (((long long) fileTime.dwHighDateTime) << 32) | fileTime.dwLowDateTime;

		if (time <= LastTimestamp)
			return ++LastTimestamp;
		else
			return LastTimestamp = time;
	}
}