
#include "Common.h"
#include "Time.h"

#ifdef _MFC_VER
long long LastTimestamp = 0;
#endif

#ifdef __LINUX__
uint64_t LastTimestamp = 0;
#endif

#ifdef __APPLE__
        // to do ...
#endif


// Получить число 100-наносекундных интервалов от 1 января 1601 года.

#ifdef _MFC_VER
#include <Windows.h>
long long GetTimestamp()
{

	FILETIME fileTime;

        // This structure is a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601.
	// The actual resolution depends on the system, http://msdn.microsoft.com/en-us/library/windows/desktop/ms724397%28v=vs.85%29.aspx

	GetSystemTimeAsFileTime(&fileTime);

	{
		long long time = (((long long) fileTime.dwHighDateTime) << 32) | fileTime.dwLowDateTime;

		if (time <= LastTimestamp)
			return ++LastTimestamp;
		else
			return LastTimestamp = time;
	}
}
#endif

#ifdef __LINUX__
uint64_t GetTimestamp()
{

#include <stdint.h>

#define DELTA_EPOCH_IN_MICROSECS  11644473600000000ULL

	// see 3.5 in http://citforum.ru/programming/c_unix/gl_3.shtml
	time_t t = time((time_t *)NULL);
	uint64_t time = t;

	// see http://suacommunity.com/dictionary/gettimeofday-entry.php
	time += DELTA_EPOCH_IN_MICROSECS
	time *= 10;

	if (time <= LastTimestamp)
		return ++LastTimestamp;
	else
		return LastTimestamp = time;
}
#endif

#ifdef __APPLE__
	// to do ...
#endif

