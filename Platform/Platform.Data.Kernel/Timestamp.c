// Модуль с функцией вычисления "штампа времени" (time-stamp).

#include "Common.h"
#include "Timestamp.h"

#if defined(WINDOWS) 
#include <windows.h>
#endif
#if defined(LINUX)
#include <time.h>
#define DELTA_EPOCH_IN_MICROSECS  11644473600000000ULL
#endif

signed_integer LastTimestamp = 0;

// Получить число 100-наносекундных интервалов от 1 января 1601 года.
signed_integer GetTimestamp()
{
    signed_integer timestamp;

#if defined(WINDOWS)
    // This structure is a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601.
    // The actual resolution depends on the system, http://msdn.microsoft.com/en-us/library/windows/desktop/ms724397%28v=vs.85%29.aspx
    FILETIME osTime;
    GetSystemTimeAsFileTime(&osTime);
    timestamp = (((signed_integer)osTime.dwHighDateTime) << 32) | osTime.dwLowDateTime;
#elif defined(LINUX)
    // see 3.5 in http://citforum.ru/programming/c_unix/gl_3.shtml
    time_t osTime = time((time_t *)NULL);
    timestamp = osTime;
    // see http://suacommunity.com/dictionary/gettimeofday-entry.php
    timestamp += DELTA_EPOCH_IN_MICROSECS;
    timestamp *= 10;
#endif

    if (timestamp <= LastTimestamp)
        return ++LastTimestamp;
    else
        return LastTimestamp = timestamp;
}
